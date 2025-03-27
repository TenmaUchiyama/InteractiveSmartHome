from typing import Any, Dict
import uuid
from network.msg_types.client_types import ClientMessageType
from network.msg_types.server_types import TestComplexRequestMessage
from network.server.session import WebSocketSession




async def greeting_handler(session: WebSocketSession, request_message: Dict[str, Any]) -> Any:
    print("Received GetGreeting request from client.")
    # サーバーはクライアントに "GetName" をリクエストして名前を取得
    name_response = await session.requestMethod(ClientMessageType.GET_NAME.value)
    if name_response and "name" in name_response:
        name = name_response["name"]
        return {"data": f"hello {name} from Server"}
    else:
        return {"error": "Failed to get name"}