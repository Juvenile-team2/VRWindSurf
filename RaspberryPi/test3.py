import socket
import time
import motoron

mc = motoron.MotoronI2C()
mc.reinitialize()
mc.disable_crc()
mc.clear_reset_flag()
mc.disable_command_timeout()  # タイムアウトを無効化

# 加減速の設定（スムーズにする）
mc.set_max_acceleration(1, 800)  # 変更: 800 → 400
mc.set_max_deceleration(1, 800)
mc.set_max_acceleration(2, 800)
mc.set_max_deceleration(2, 800)

# 速度の最大値を設定
max_speed = 800  # モーターの最大速度
interval = 0.1  # 送信間隔

# サーバーの設定
HOST = '0.0.0.0'  # サーバーのIPアドレス
PORT = 12345  # ポート番号

# ソケットの作成とバインド
while True:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind((HOST, PORT))
    s.listen(1)

    print("Waiting for a connection...")

    connection, client_address = s.accept()
    print(f"Connected from {client_address}")
    mc.set_speed(1, 800)
    mc.set_speed(2, 800)
    time.sleep(5)  # 100ms ごとに送信
    mc.set_speed(1, 0)  # モーターを停止
    mc.set_speed(2, 0)

    try:
        while True:
            data = connection.recv(1024)  # 1024バイト分受信
            if not data:
                print("Client disconnected")
                break  # データがない場合、接続終了
        
            # 受信データを文字列に変換
            data_str = data.decode('utf-8').strip()

            try:
                # 文字列を float に変換
                received_value = float(data_str)

                # Unity からの値は -50 ～ 50 の範囲なので、これを -1000 ～ 1000 にスケール変換
                speed = int((received_value / 50.0) * 1.5 * max_speed)

                # モーターを滑らかに動かす
                mc.set_speed(1, speed)
                mc.set_speed(2, -speed)  # 逆方向に回転
            
                print(f"モーターの速度を更新: {speed}")

                # クライアントに ACK を送信
                response = f"ACK: {speed}"
                connection.sendall(response.encode('utf-8'))

            except ValueError:
                print(f"Invalid float received: {data_str}")
                connection.sendall(b"Error: Invalid float")  # エラー通知を送信

    except Exception as e:
        print("Error:", e)

    finally:
        mc.set_speed(1, -800)
        mc.set_speed(2, -800)
        time.sleep(9)  # 100ms ごとに送信
        mc.set_speed(1, 0)  # モーターを停止
        mc.set_speed(2, 0)
        print("Closing connection")
        connection.close()
        s.close()
