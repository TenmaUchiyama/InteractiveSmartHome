from enum import Enum
from typing import Dict, Any, Coroutine
from network.handlers.additional import handle_additional_data
from network.handlers.initial import handle_initial_data
from network.handlers.test_complex import greeting_handler
from network.msg_types.server_types import ServerMessageType





# 追加のハンドラーを作れば import で集約していく

# メッセージタイプとハンドラーを一括管理
HANDLERS: Dict[ServerMessageType, Coroutine] = {
    ServerMessageType.INITIAL_DATA: handle_initial_data,
    ServerMessageType.ADDITIONAL_DATA: handle_additional_data,
    ServerMessageType.Greeting: greeting_handler
}



