# RealRotationLogger

Unityの実験セッションに合わせて、Raspberry Piに接続したIMU (BNO086) の実回転角度をログするTCPサーバー。

## 構成

- `server.py` : ロギング用TCPサーバー本体
- `requirements.txt` : 必要なPythonパッケージ
- `setup.sh` : venv環境のセットアップスクリプト

## セットアップ

### 1. I2Cを有効化

```bash
sudo raspi-config
```
`Interface Options` → `I2C` → `Yes` を選択して再起動。

### 2. 配線

BNO086モジュールをQwiicケーブル（またはGPIO 1/3/5/6 = 3V3/GND/SDA/SCL）でRaspberry Piに接続する。

接続確認:
```bash
sudo i2cdetect -y 1
```
`0x4b`（デフォルト）または`0x4a`が表示されればOK。

### 3. Python環境（venv）

`python3-venv`が入っていない場合は先にインストール:
```bash
sudo apt install python3-venv
```

セットアップスクリプトを実行:
```bash
cd RaspberryPi/RealRotationLogger
./setup.sh
```

`venv`の作成 → `pip`アップグレード → `requirements.txt`のインストールまで一括で行われる。

## サーバー起動

```bash
source venv/bin/activate
python3 server.py
```

`接続待機中... 0.0.0.0:12346` と表示されれば待機状態。

## Unity側の設定

`ExperimentManager` の Inspector で以下を設定する。

- `Use Rotation Logger` : チェックを入れるとラズパイへの接続・ログ要求を行う（外すと一切行わない）
- `Rotation Logger Host` : Raspberry PiのIPアドレス（Piで`hostname -I`で確認）
- `Rotation Logger Port` : `12346`（`server.py`の`PORT`と一致させる）

## 動作の流れ

1. `server.py`を起動したまま待機
2. Unityで実験シーンを再生すると接続し、セッションフォルダ名を送信（Pi側に`接続: (アドレス, ポート)`と表示）
3. 接続中、IMUから読んだXYZ角度を10Hzで内部ファイルに書き込み続ける
4. 実験終了・待機シーン遷移時、Unityが終了コマンドを送信すると、Pi側はログ書き込みを止めてファイルを閉じ、内容をUnityへ送り返す（`終了コマンドを受信...Unityへ送信します`と表示）
5. Unity側は受け取った内容を `Assets/Jarnal/Result/Log/Cube/<セッション名>/real_rotation.csv` として保存
6. サーバーは再び次の接続を待つ

## 注意点

- 常駐させたい場合は`systemd`サービス化や`tmux`/`screen`での多重起動を検討する
- Unity実行PCとRaspberry Piが同一LANにあり、ポート12346が疎通できる必要がある（ファイアウォール要確認）
- `qwiic_bno08x`のAPI（`enable_rotation_vector`など）はライブラリのバージョンによってメソッド名が異なる場合がある。エラーが出た場合は実際のライブラリのサンプルコードと突き合わせて`server.py`内の`open_imu`/`read_euler_deg`を調整する
