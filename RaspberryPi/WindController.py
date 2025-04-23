from websocket_server import WebsocketServer
import threading

clients = {
    "sender": None,
    "receiver": None,
}

def process_data(data):
    # ここで任意の処理をする（例：大文字に変換）
    return data.upper()

def new_client(client, server):
    print(f"New client connected: {client['id']}")

def client_left(client, server):
    print(f"Client disconnected: {client['id']}")
    if clients.get("sender") == client:
        clients["sender"] = None
    elif clients.get("receiver") == client:
        clients["receiver"] = None

def message_received(client, server, message):
    global clients

    # 最初のメッセージで役割を決める
    if clients["sender"] is None and message == "sender":
        clients["sender"] = client
        print("Sender registered.")
    elif clients["receiver"] is None and message == "receiver":
        clients["receiver"] = client
        print("Receiver registered.")
    elif clients["sender"] == client:
        print(f"Received from sender: {message}")
        processed = process_data(message)
        if clients["receiver"]:
            server.send_message(clients["receiver"], processed)
        else:
            print("Receiver not connected.")

server = WebsocketServer(host='0.0.0.0', port=8765)
server.set_fn_new_client(new_client)
server.set_fn_client_left(client_left)
server.set_fn_message_received(message_received)

print("Server started.")
server.run_forever()
