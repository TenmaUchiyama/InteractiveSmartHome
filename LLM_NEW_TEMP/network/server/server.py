import asyncio
import websockets
import json
import uuid
from typing import Dict, Any, Callable, Awaitable
from network.msg_types.server_types import ServerMessageType
from network.server.session import WebSocketSession

class WebSocketServer:
    def __init__(self, host: str = "localhost", port: int = 8765):
        self.host = host
        self.port = port
        self.sessions: Dict[str, WebSocketSession] = {}
        self.handlers: Dict[str, Callable[[WebSocketSession, Dict[str, Any]], Awaitable[Any]]] = {}

        # WebSocketServer.external_handler には、サーバーインスタンスの process_server_message を設定
        WebSocketServer.external_handler = self.process_server_message

    async def handle_client(self, websocket, _path):
        """新しいクライアント接続時にセッションを作成し、登録されたハンドラを適用"""
        session = WebSocketSession(websocket)
        self.sessions[session.session_id] = session
        print(f"New connection: {session.session_id}")

        # セッションにサーバー側で登録されたハンドラを適用
        for method, handler in self.handlers.items():
     
            session.register_request_handler(method, handler)

        # 未処理のメッセージは WebSocketServer 側で処理
        session.external_handler = self.process_server_message

        try:

            async for message in websocket:
              
                await session.process_message(message)
        except websockets.exceptions.ConnectionClosed:
            print(f"Connection lost: {session.session_id}")
        finally:
            del self.sessions[session.session_id]
            print(f"Connection closed: {session.session_id}")

    async def process_server_message(self, session: WebSocketSession, message: Dict[str, Any]):
        """request/response 以外のメッセージをサーバーハンドラで処理"""
        message_type = message.get("type")
        handler = self.handlers.get(message_type)

        print("Processing server message:", message)
        if handler:
            await handler(session, message)
        else:
            print(f"Unknown message type: {message_type}")

    def add_handler(self, method: str, handler: Callable[[WebSocketSession, Dict[str, Any]], Awaitable[Any]]):
        """動的にサーバーハンドラを登録"""
        self.handlers[method] = handler

    async def run(self):
        """WebSocket サーバーを起動"""
        async with websockets.serve(self.handle_client, self.host, self.port):
            print(f"Server started at ws://{self.host}:{self.port}")
            await asyncio.Future()  # 無期限に待機
