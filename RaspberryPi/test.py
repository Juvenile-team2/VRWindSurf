import time
import motoron

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
    try:
        input_speed = float(input("モーターを動かす秒数を入力（-50~+50）: "))
        speed = int(input_speed*16)

        print("3秒間モーターを動かします！")

        mc.set_speed(1, speed)  # モーターを回転
        mc.set_speed(2, speed*(-1))  # モーターを回転
        time.sleep(3)  # 100ms ごとに送信
        mc.set_speed(1, 0)  # モーターを停止
        print("モーターを停止しました。")

    except KeyboardInterrupt:
        print("\nプログラムを終了します。")
        break
