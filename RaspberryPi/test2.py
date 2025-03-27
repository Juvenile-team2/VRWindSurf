import time
import motoron
from __future__ import print_function
import socket

backlog = 1
size = 1024
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind(('192.168.1.109', 12345))  # サーバーのIPアドレスとポートを指定
s.listen(1)  # クライアントの接続を待機

mc = motoron.MotoronI2C()
mc.reinitialize()
mc.disable_crc()
mc.clear_reset_flag()
mc.disable_command_timeout()  # タイムアウトを無効化

mc.set_max_acceleration(1, 800)
mc.set_max_deceleration(1, 800)

mc.set_max_acceleration(2, 800)
mc.set_max_deceleration(2, 800)

mc.set_speed(1, 800)
mc.set_speed(2, 800)
time.sleep(4.5)  # 100ms ごとに送信
mc.set_speed(1, 0)  # モーターを停止
mc.set_speed(2, 0)

while True:
    print('Waiting for a connection...')
    connection, client_address = s.accept()
    print('Connected from', client_address)

    try:
        # 初回データ受信
        data = connection.recv(1024)
        if data:
            print('Received:', data.decode('utf-8'))  # 受信データを文字列として表示
            connection.sendall(b'This is the server')  # クライアントに応答

        # クライアントからのデータを受信・エコーバック
        while True:
            data = connection.recv(16)
            if not data:  # 受信データが空ならクライアントが切断
                print('Client disconnected:', client_address)
                break
            data.decode('utf-8')
            speed = int(data*16)
            #print('Received:', data.decode('utf-8'))
            #connection.sendall(data)  # 受信データをそのまま返す（エコーバック）
            mc.set_speed(1, speed)  # モーターを回転
            mc.set_speed(2, speed*(-1))  # モーターを回転

    except Exception as e:
        print('Error:', e)

    finally:
        print("Closing connection")
        connection.close()  # クライアント接続を閉じる

"""
while True:
    try:
        input_speed = float(input("モーターを動かす秒数を入力（-50~+50）: "))
        speed = input_speed*16

        print("3秒間モーターを動かします！")

        mc.set_speed(1, speed)  # モーターを回転
        mc.set_speed(2, speed*(-1))  # モーターを回転
        time.sleep(3)  # 100ms ごとに送信
        mc.set_speed(1, 0)  # モーターを停止
        print("モーターを停止しました。")

    except KeyboardInterrupt:
        print("\nプログラムを終了します。")
        break
"""