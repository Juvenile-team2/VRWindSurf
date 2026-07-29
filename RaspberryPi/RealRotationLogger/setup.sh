#!/bin/bash
# Raspberry Pi側の実行環境をvenvでセットアップする。
# 使い方: cd RaspberryPi/RealRotationLogger && ./setup.sh
set -e

cd "$(dirname "$0")"

python3 -m venv venv
source venv/bin/activate
pip install --upgrade pip
pip install -r requirements.txt

echo ""
echo "セットアップ完了。サーバーを起動するには:"
echo "  source venv/bin/activate"
echo "  python3 server.py"
