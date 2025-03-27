from network.server.session import WebSocketSession
from network.msg_types.client_types import ClientMessageType
from network.msg_types.server_types import AdditionalDataMessage

async def handle_additional_data(session: WebSocketSession, message: AdditionalDataMessage):

    print("[ADDITIONAL] received", message)
    original_data = session.custom_data.pop(message.get("request_id"), None)
    if original_data:
        await session.send(ClientMessageType.RESULT, data=f"{original_data} + {message['data']}")
    else:
        await session.send(ClientMessageType.ERROR, message="Invalid request ID")
