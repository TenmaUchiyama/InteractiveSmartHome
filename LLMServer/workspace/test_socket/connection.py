
import uuid
from typing import Any, Dict
from pydantic import ValidationError
from models import ServerRequestMessage, ClientResponseMessage
import asyncio

class ConnectionManager:
    def __init__(self, websocket):
        self.websocket = websocket
        self.pending_requests: Dict[str, asyncio.Future] = {}
        self.lock = asyncio.Lock()
    
    async def send_request(self, action: str, body: Any) -> Any:
        """
        クライアントにリクエストを送り、レスポンスを待つ関数。
        """
        request_id = str(uuid.uuid4())
        message = ServerRequestMessage(action=action, request_id=request_id, body=body)
        await self.websocket.send_json(message.dict())
        future = asyncio.get_event_loop().create_future()
        async with self.lock:
            self.pending_requests[request_id] = future
        return await future  # レスポンスを待つ
    
    async def handle_response(self, message: Dict):
        """
        クライアントからのレスポンスを処理する関数。
        """
        try:
            response = ClientResponseMessage(**message)
        except ValidationError as e:
            print(f"Invalid response format: {e}")
            return
        request_id = response.request_id
        async with self.lock:
            future = self.pending_requests.pop(request_id, None)
        if future:
            if response.status == "success":
                future.set_result(response.body)
            else:
                future.set_exception(Exception(response.error or "Unknown error"))
    
    async def close(self):
        """
        WebSocket接続を閉じる関数。
        """
        await self.websocket.close()
