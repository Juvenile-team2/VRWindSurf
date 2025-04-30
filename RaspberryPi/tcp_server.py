import socket
import threading
import subprocess
import json

connected_clients = {}

def handle_client(conn, addr):
    print(f"Connected from {addr}")

    role = None

    try:
        while True:
            data = conn.recv(1024)
            if not data:
                print(f"{addr} disconnected")
                break

            data_str = data.decode().strip()
            print(f"Received from {addr}: {data_str}")

            if data_str.startswith("REGISTER:"):
                role = data_str.split(":")[1]
                connected_clients[role] = conn
                print(f"Registered role: {role}")

            elif data_str == "{1}":
                print("Message from ClientA received")

                try:
                    result = subprocess.run(
                        ["python3", "irrp.py", "-p", "-g18", "-f", "ir_data", "wind1:on"],
                        check=True,
                        stdout=subprocess.PIPE,
                        stderr=subprocess.PIPE
                    )
                    print("Command executed:", result.stdout.decode())
                except subprocess.CalledProcessError as e:
                    print("Subprocess error:", e.stderr.decode())

                client_b = connected_clients.get("ClientB")
                if client_b:
                    try:
                        vector_str = "(1, 2, 3)\n"  # Unity側と互換性のある形式
                        client_b.sendall(vector_str.encode())
                        print("Sent to ClientB:", msg)
                    except Exception as send_err:
                        print("Failed to send to ClientB:", send_err)
                else:
                    print("ClientB not connected")

    except Exception as e:
        print("Error with client:", e)

    finally:
        if role and connected_clients.get(role) == conn:
            del connected_clients[role]
            print(f"Unregistered {role}")
        conn.close()

# メインのサーバー起動処理
def start_server(host='0.0.0.0', port=12345):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server:
        server.bind((host, port))
        server.listen(5)
        print(f"TCP server started on {host}:{port}")

        while True:
            conn, addr = server.accept()
            thread = threading.Thread(target=handle_client, args=(conn, addr), daemon=True)
            thread.start()

if __name__ == '__main__':
    start_server()
