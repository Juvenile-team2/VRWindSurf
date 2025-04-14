import socket
import time
import RPi.GPIO as GPIO
from hx711py.hx711 import HX711

# Raspberry Pi の IPアドレスとポート
HOST = "0.0.0.0"  # 全てのIPからの接続を許可
PORT = 12345

# ピン設定
PIN_DAT = 5
PIN_CLK = 6
referenceUnit = 112.271  # キャリブレーション値

# HX711の初期化
def initialize_hx711():
    hx = HX711(PIN_DAT, PIN_CLK)
    hx.set_reading_format("MSB", "MSB")
    hx.set_reference_unit(referenceUnit)
    hx.reset()
    hx.tare()
    print("Tare done! Ready to measure weight.")
    return hx

def start_server():
    hx = initialize_hx711()
    
    # ソケット作成
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server:
        server.bind((HOST, PORT))
        server.listen(5)  # 最大5接続待機
        print(f"Server listening on {HOST}:{PORT}")

        while True:
            conn, addr = server.accept()
            print(f"Connected by {addr}")
            with conn:
                try:
                    while True:
                        weight = hx.get_weight(5)
                        message = f"{weight}\n"
                        conn.sendall(message.encode("utf-8"))  # データ送信
                        print(f"Sent weight: {weight}g")
                        hx.power_down()
                        hx.power_up()
                        time.sleep(0.5)  # 0.5秒ごとに送信
                except (ConnectionResetError, BrokenPipeError):
                    print("Client disconnected.")
                finally:
                    GPIO.cleanup()

if __name__ == "__main__":
    start_server()

