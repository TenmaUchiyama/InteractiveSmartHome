import json
import uuid
import asyncio
from typing import Awaitable, Callable, Dict, Any, Optional

from network.msg_types.client_types import ClientMessageType

class WebSocketSession:
    def __init__(self, websocket):
        self.websocket = websocket
        self.session_id = str(uuid.uuid4())
        # 模擬 Request/Response 用の pending_requests
        self.pending_requests: Dict[str, asyncio.Future] = {}
        # 受信したリクエストの method ごとのハンドラ
        self.request_handlers: Dict[str, Callable[[Dict[str, Any]], Awaitable[Any]]] = {}

    async def send(self, message_type: str, **kwargs: Any):
        message = {"type": message_type, **kwargs}
        await self.websocket.send(json.dumps(message))

    def register_request_handler(self, method: str, handler: Callable[[Dict[str, Any]], Awaitable[Any]]):
        
        self.request_handlers[method] = handler

    async def requestMethod(self, method: str) -> Any:
        """指定 method のリクエストを送信し、同一 request_id・method のレスポンスを待つ"""
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
                print(method in self.request_handlers   )
                if method in self.request_handlers:
                    
                    # ハンドラ実行（例：greeting_handler）
                    response_data = await self.request_handlers[method](self,message)
                    response_message = {
                        "request_id": request_id,
                        "method": method,
                        "type": "response",
                        "responseData": response_data,
                    }
                    await self.websocket.send(json.dumps(response_message))
                else:
                    # ハンドラが無い場合はエラー情報を返す
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