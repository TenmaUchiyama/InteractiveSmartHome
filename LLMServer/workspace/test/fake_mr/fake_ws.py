import asyncio
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
import uvicorn
from starlette.middleware.cors import CORSMiddleware
import threading
from typing import Any, Callable, Dict
import json
from pydantic import BaseModel, ValidationError
from typing import Union, List, Literal

app = FastAPI()

# CORSミドルウェアの設定
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # 必要に応じて制限
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# サンプルデータ
sample_data = {
    "id": 1,
    "name": "sample data",
    "value": 42
}

# デバイスメタデータの構造を定義
class DeviceMetadata(BaseModel):
    id: int
    type: str
    status: str
    # 必要に応じて他のフィールドを追加

# "llm_agent" アクション用のメッセージモデル
class LLMActionMessage(BaseModel):
    action: Literal["llm_agent"]
    body: str

# "device" アクション用のメッセージモデル
class DeviceActionMessage(BaseModel):
    action: Literal["device"]
    body: List[DeviceMetadata]


class FunctionReqeuqst(BaseModel):
    function: str
    args: List[str]

# 全てのアクションメッセージを統一するためのUnion型
ActionMessage = Union[LLMActionMessage, DeviceActionMessage]

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

@register_action("get_data")
def get_data(request: Dict) -> Dict:
    """
    'get_data'アクションのハンドラー。
    """
    return {"status": "success", "data": sample_data}

@register_action("echo")
def echo(request: Dict) -> Dict:
    """
    'echo'アクションのハンドラー。
    """
    message = request.get("message", "")
    return {"status": "success", "echo": message}

@register_action("update_data")
def update_data(request: Dict) -> Dict:
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

# 接続中のクライアントを管理するセットとロック
connected_clients = set()
client_lock = threading.Lock()

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    with client_lock:
        connected_clients.add(websocket)
    client_address = websocket.client.host
    print(f"Client connected: {client_address}")
    try:
        while True:
            data = await websocket.receive_text()
            print(f"Received from {client_address}: {data}")
            try:
                # Pydanticでメッセージをバリデート
                message = ActionMessage.parse_raw(data)
                
                # action に基づいてハンドラーを呼び出す
                if message.action in action_handlers:
                    handler = action_handlers[message.action]
                    response = handler(message)
                else:
                    response = {"status": "error", "message": f"Unknown action: {message.action}"}
            except ValidationError as e:
                response = {"status": "error", "message": "Invalid message format.", "details": e.errors()}
            except Exception as e:
                response = {"status": "error", "message": f"Server error: {str(e)}"}
            
            response_text = json.dumps(response)
            await websocket.send_text(response_text)
            print(f"Sent to {client_address}: {response_text}")
    except WebSocketDisconnect:
        with client_lock:
            connected_clients.remove(websocket)
        print(f"Client disconnected: {client_address}")
    except Exception as e:
        with client_lock:
            connected_clients.remove(websocket)
        print(f"Client disconnected with error: {client_address}, error: {e}")

def input_thread(loop):
    """
    入力を受け取り、接続されている全てのクライアントにメッセージを送信する関数。
    この関数は別スレッドで実行されます。
    """
    while True:
        user_input = input("Enter message to send to clients: ")
        # ここでは "llm_agent" アクションとして送信

        function_request = FunctionReqeuqst(function="direction", args=[user_input])
        message_text = json.dumps(function_request.dict())
        
        with client_lock:
            clients = list(connected_clients)
        for ws in clients:
            # 非同期関数をスレッドから安全に実行
            asyncio.run_coroutine_threadsafe(ws.send_text(message_text), loop)

@app.on_event("startup")
async def startup_event():
    """
    アプリケーションの起動時に実行されるイベントハンドラ。
    入力スレッドを開始します。
    """
    loop = asyncio.get_event_loop()
    thread = threading.Thread(target=input_thread, args=(loop,), daemon=True)
    thread.start()

if __name__ == "__main__":
    uvicorn.run("fake_ws:app", host="localhost", port=7070, reload=True)
