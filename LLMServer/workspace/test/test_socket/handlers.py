

from typing import Any, Callable, Dict
from models import LLMActionMessage, DeviceActionMessage

# アクションハンドラーの型定義
ActionHandler = Callable[[Any], Dict]

# アクションハンドラーを格納する辞書
action_handlers: Dict[str, ActionHandler] = {}

def register_action(action: str):
    """
    アクションハンドラーを登録するデコレータ。
    """
    def decorator(func: ActionHandler):
        action_handlers[action] = func
        return func
    return decorator

# サンプルデータ
sample_data = {
    "id": 1,
    "name": "sample data",
    "value": 42
}

@register_action("get_data")
def get_data_handler(request: Dict) -> Dict:
    """
    'get_data'アクションのハンドラー。
    """
    return {"status": "success", "data": sample_data}

@register_action("echo")
def echo_handler(request: Dict) -> Dict:
    """
    'echo'アクションのハンドラー。
    """
    message = request.get("message", "")
    return {"status": "success", "echo": message}

@register_action("update_data")
def update_data_handler(request: Dict) -> Dict:
    """
    'update_data'アクションのハンドラー。
    """
    new_value = request.get("value")
    if new_value is not None:
        sample_data["value"] = new_value
        return {"status": "success", "data": sample_data}
    else:
        return {"status": "error", "message": "No 'value' provided."}

@register_action("llm_agent")
def llm_agent_handler(message: LLMActionMessage) -> Dict:
    """
    'llm_agent'アクションのハンドラー。
    """
    user_message = message.body
    # ここでLLMエージェントの処理を実装
    response_message = f"LLM Agent received: {user_message}"
    return {"status": "success", "response": response_message}

@register_action("device")
def device_handler(message: DeviceActionMessage) -> Dict:
    """
    'device'アクションのハンドラー。
    """
    device_data = message.body
    # ここでデバイスメタデータの処理を実装
    return {"status": "success", "devices_received": len(device_data)}
