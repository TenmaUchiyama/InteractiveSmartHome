# client.py
import asyncio
import websockets
import json
import uuid
from typing import Any, Dict, Callable, Awaitable

# --- WebSocketSession の実装（サーバー側と同一） ---
class WebSocketSession:
    def __init__(self, websocket):
        self.websocket = websocket
        self.session_id = str(uuid.uuid4())
        self.pending_requests: Dict[str, asyncio.Future] = {}
        self.request_handlers: Dict[str, Callable[[Dict[str, Any]], Awaitable[Any]]] = {}

    async def send(self, message_type: str, **kwargs: Any):
        message = {"type": message_type, **kwargs}
        await self.websocket.send(json.dumps(message))

    def register_request_handler(self, method: str, handler: Callable[[Dict[str, Any]], Awaitable[Any]]):
        self.request_handlers[method] = handler

    async def requestMethod(self, method: str) -> Any:
        request_id = str(uuid.uuid4())
        future = asyncio.get_event_loop().create_future()
        self.pending_requests[request_id] = future

        request_message = {
            "request_id": request_id,
            "method": method,
            "type": "request"
        }
        await self.websocket.send(json.dumps(request_message))
        response = await future

        if (response.get("request_id") == request_id and
            response.get("method") == method and
            response.get("type") == "response"):
            return response.get("responseData")
        else:
            raise Exception("Invalid response received.")

    async def process_message(self, raw_message: str):
        try:
            message = json.loads(raw_message)
            msg_type = message.get("type")
            if msg_type == "request":
                method = message.get("method")
                request_id = message.get("request_id")
                if method in self.request_handlers:
                    response_data = await self.request_handlers[method](message)
      
                    response_message = {
                        "request_id": request_id,
                        "method": method,
                        "type": "response",
                        "responseData": response_data,
                    }

                    await self.websocket.send(json.dumps(response_message))
                else:

                    response_message = {
                        "request_id": request_id,
                        "method": method,
                        "type": "response",
                        "responseData": {"error": f"No handler for method {method}"},
                    }
                    await self.websocket.send(json.dumps(response_message))
            elif msg_type == "response":
                request_id = message.get("request_id")
                if request_id and request_id in self.pending_requests:
                    future = self.pending_requests.pop(request_id)
                    future.set_result(message)
                else:
                    print(f"Received unexpected response: {message}")
            else:
                print(f"Unknown message type: {msg_type}")
        except json.JSONDecodeError:
            print("Invalid JSON received")

# --- クライアントメイン処理 ---
async def client_main():
    uri = "ws://localhost:8765"
    async with websockets.connect(uri) as websocket:
        session = WebSocketSession(websocket)

        print("Connected:" , session.session_id)
        # サーバーから "GetName" リクエストが来たときのハンドラを登録
        session.register_request_handler("getName", get_name_handler)

        # バックグラウンドで受信処理ループを起動
        async def receive_loop():
            async for message in websocket:
                await session.process_message(message)
        asyncio.create_task(receive_loop())

        # クライアントはサーバーに "GetGreeting" リクエストを送信
        response = await session.requestMethod("greeting")
        print("Client received response:", response.get("data"))

# --- サーバーからの "GetName" リクエストを処理するハンドラ ---
async def get_name_handler(message: Dict[str, Any]) -> Any:
    print("Received GetName request from server.")
    # ここでクライアントが持つ名前を返す（例: "Alice"）
    return {"name": "Alice"}

if __name__ == "__main__":
    asyncio.run(client_main())
