#!/usr/bin/env python3
"""
Raspberry Pi 側: 実験セッションごとに IMU (BNO086) の実回転角度をロギングするTCPサーバー。

プロトコル:
  1. Unity (クライアント) が接続する。
  2. 接続直後、Unity は保存先フォルダ名を1行 ("\n" 区切り) で送る。
     例: "P001_Wave-Normal_Act-50_20260716_235051"
     （Unity側の Assets/Jarnal/Result/Log/Cube/ 配下のセッションフォルダ名と同じ）
  3. サーバーは <BASE_DIR>/<フォルダ名>/real_rotation.csv を作成し、
     IMU から読み取った角度を一定間隔でログし続ける。
  4. Unity が実験終了時（待機シーン遷移時）に "END\n" を送ると、
     サーバーは書き込みを止めてファイルを閉じ、その内容を
     "<バイト数>\n" に続けてバイナリでUnityへ送り返す。
  5. Unity からの応答を待たずに接続が切れた場合（アプリ強制終了など）は、
     ファイルを閉じるだけでUnityへの送信は行わない。
  6. 再び次の接続を待つ。

必要ライブラリ (SparkFun純正のQwiic Python版BNO08xパッケージはPyPIに存在しないため、
I2C経由で動くAdafruitのCircuitPythonライブラリ+Blinka互換レイヤーを使う):
  pip install adafruit-blinka adafruit-circuitpython-bno08x
"""

import math
import os
import socket
import time

try:
    import board
    import busio
    from adafruit_bno08x import BNO_REPORT_ROTATION_VECTOR
    from adafruit_bno08x.i2c import BNO08X_I2C
except ImportError:
    board = None

HOST = "0.0.0.0"
PORT = 12346
LOG_INTERVAL_SEC = 0.1  # 10Hz
BASE_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Result", "Log", "Cube")
BNO08X_I2C_ADDRESS = 0x4B  # `sudo i2cdetect -y 1` で確認したアドレス（デフォルト）


def open_imu():
    """BNO086 を初期化して返す。

    リセット直後にenable_featureを呼ぶと、初期化時に送られてくる
    アドバタイズ/コマンド応答パケットの処理でadafruit_bno08x側が
    RuntimeError("Unprocessable Batch bytes", ...) を出すことがあるため、
    少し待ってからリトライする。
    """
    if board is None:
        raise RuntimeError(
            "adafruit_bno08x が見つかりません。"
            "`pip install adafruit-blinka adafruit-circuitpython-bno08x` を実行してください。"
        )
    i2c = busio.I2C(board.SCL, board.SDA)
    imu = BNO08X_I2C(i2c, address=BNO08X_I2C_ADDRESS)
    time.sleep(0.5)  # リセット直後の初期パケット処理が落ち着くのを待つ

    last_error = None
    for _ in range(5):
        try:
            imu.enable_feature(BNO_REPORT_ROTATION_VECTOR)
            return imu
        except RuntimeError as e:
            last_error = e
            time.sleep(0.5)

    raise RuntimeError(f"BNO086の初期化に失敗しました: {last_error}")


def read_euler_deg(imu):
    """IMUの現在の回転をXYZ(度)で返す。"""
    qi, qj, qk, qreal = imu.quaternion

    sinr_cosp = 2 * (qreal * qi + qj * qk)
    cosr_cosp = 1 - 2 * (qi * qi + qj * qj)
    roll = math.degrees(math.atan2(sinr_cosp, cosr_cosp))

    sinp = max(-1.0, min(1.0, 2 * (qreal * qj - qk * qi)))
    pitch = math.degrees(math.asin(sinp))

    siny_cosp = 2 * (qreal * qk + qi * qj)
    cosy_cosp = 1 - 2 * (qj * qj + qk * qk)
    yaw = math.degrees(math.atan2(siny_cosp, cosy_cosp))

    return roll, pitch, yaw


def recv_line(conn):
    """1行分（"\n"区切り）のテキストを受信する。切断時は None を返す。"""
    buf = b""
    while True:
        chunk = conn.recv(1)
        if not chunk:
            return None  # 切断
        if chunk == b"\n":
            return buf.decode("utf-8").strip()
        buf += chunk


def poll_command(conn, cmd_buf):
    """ノンブロッキングで受信バッファを更新する。
    戻り値: ("end" | "disconnected" | "none", 更新後のcmd_buf)
    """
    conn.setblocking(False)
    try:
        data = conn.recv(4096)
        if data == b"":
            return "disconnected", cmd_buf
        cmd_buf += data
        if b"END" in cmd_buf:
            return "end", cmd_buf
        return "none", cmd_buf
    except BlockingIOError:
        return "none", cmd_buf
    except (ConnectionResetError, BrokenPipeError, OSError):
        return "disconnected", cmd_buf
    finally:
        conn.setblocking(True)


def send_file_to_client(conn, path):
    """ログファイルの内容を "<バイト数>\n" + バイナリ本体 の形式で送信する。"""
    try:
        with open(path, "rb") as f:
            data = f.read()
        conn.setblocking(True)
        conn.sendall(f"{len(data)}\n".encode("utf-8"))
        conn.sendall(data)
        print(f"real_rotation.csv をUnityへ送信しました（{len(data)}バイト）")
    except Exception as e:
        print(f"CSV送信エラー: {e}")


def handle_session(conn, imu):
    conn.settimeout(None)
    session_name = recv_line(conn)
    if not session_name:
        print("セッション名を受信できませんでした。接続を終了します。")
        return

    session_dir = os.path.join(BASE_DIR, session_name)
    os.makedirs(session_dir, exist_ok=True)
    log_path = os.path.join(session_dir, "real_rotation.csv")

    print(f"[{session_name}] ログ開始: {log_path}")
    start_time = time.time()
    cmd_buf = b""
    status = "none"

    with open(log_path, "w", encoding="utf-8") as f:
        f.write("time_sec,angle_x,angle_y,angle_z\n")
        f.flush()

        while True:
            status, cmd_buf = poll_command(conn, cmd_buf)
            if status != "none":
                break

            try:
                x, y, z = read_euler_deg(imu)
            except Exception as e:
                print(f"IMU読み取りエラー: {e}")
                time.sleep(LOG_INTERVAL_SEC)
                continue

            t = time.time() - start_time
            f.write(f"{t:.3f},{x:.4f},{y:.4f},{z:.4f}\n")
            f.flush()

            time.sleep(LOG_INTERVAL_SEC)

    if status == "end":
        print(f"[{session_name}] 終了コマンドを受信。ログを閉じてUnityへ送信します。")
        send_file_to_client(conn, log_path)
    else:
        print(f"[{session_name}] 切断を検知（終了コマンドなし）。ログを閉じます。")


def main():
    imu = open_imu()

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((HOST, PORT))
    server.listen(1)
    print(f"接続待機中... {HOST}:{PORT}")

    try:
        while True:
            conn, addr = server.accept()
            print(f"接続: {addr}")
            try:
                handle_session(conn, imu)
            finally:
                conn.close()
            print("次の接続を待機します...")
    except KeyboardInterrupt:
        print("終了します。")
    finally:
        server.close()


if __name__ == "__main__":
    main()
