import asyncio
from network.server.server import WebSocketServer
from network.handlers import HANDLERS   

def main():
    server = WebSocketServer()

    # handlers/ フォルダ内で定義している HANDLERS を一括登録
    for message_type, handler_func in HANDLERS.items():
        server.add_handler(message_type.value, handler_func)

    asyncio.run(server.run())

if __name__ == "__main__":
    main()