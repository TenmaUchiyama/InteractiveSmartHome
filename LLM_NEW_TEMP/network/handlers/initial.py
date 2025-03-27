import uuid
from network.server.session import WebSocketSession
from network.msg_types.server_types import InitialDataMessage
from network.msg_types.client_types import ClientMessageType


async def handle_initial_data(session: WebSocketSession, message: InitialDataMessage):
    print(f"Initial data from {session.session_id}: {message['data']}")
    request_id = str(uuid.uuid4())
    # クライアントに「追加データちょうだい」とリクエスト
    await session.send(ClientMessageType.REQUEST_DATA, request_id=request_id)
    session.custom_data[request_id] = message['data']
