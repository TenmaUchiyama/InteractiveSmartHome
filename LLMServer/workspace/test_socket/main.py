

import asyncio
import json
import threading
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from jsonschema import ValidationError
from starlette.middleware.cors import CORSMiddleware
from typing import Dict

import uvicorn
from models import LLMActionMessage
from handlers import  action_handlers
from connection import ConnectionManager
from manager import ConnectionManagerList
from utils import generate_unique_id

app = FastAPI()

# CORSミドルウェアの設定
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # 必要に応じて制限
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# 接続中のクライアントを管理するマネージャーリスト
connected_managers = ConnectionManagerList()

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    manager = ConnectionManager(websocket)
    await connected_managers.add_manager(manager)
    client_address = websocket.client.host
    print(f"Client connected: {client_address}")
    try:
        while True:
            data = await websocket.receive_json()
            print(f"Received from {client_address}: {data}")
            if "request_id" in data:
                # サーバーからのリクエストに対するクライアントのレスポンス
                await manager.handle_response(data)
            else:
                # クライアントからのアクションメッセージ
                try:
                    message = LLMActionMessage.parse_obj(data)
                    action = message.action
                    if action in action_handlers:
                        handler = action_handlers[action]
                        response = handler(message)
                    else:
                        response = {"status": "error", "message": f"Unknown action: {action}"}
                except ValidationError as e:
                    response = {"status": "error", "message": "Invalid message format.", "details": e.errors()}
                except Exception as e:
                    response = {"status": "error", "message": f"Server error: {str(e)}"}
                
                # リクエストIDを含まないレスポンスを送信
                await websocket.send_json(response)
                print(f"Sent to {client_address}: {response}")
    except WebSocketDisconnect:
        await connected_managers.remove_manager(manager)
        print(f"Client disconnected: {client_address}")
    except Exception as e:
        await connected_managers.remove_manager(manager)
        print(f"Client disconnected with error: {client_address}, error: {e}")

async def send_message_to_clients(message: str):
    """
    接続されている全てのクライアントにメッセージを送信し、レスポンスを待つ関数。
    """
    managers = await connected_managers.get_managers()
    for manager in managers:
        try:
            # 例えば、'echo' アクションとして送信
            response = await manager.send_request(action="echo", body={"message": message})
            print(f"Received echo response from client {manager.websocket.client.host}: {response}")
        except Exception as e:
            print(f"Error sending message to client {manager.websocket.client.host}: {e}")

def input_thread_func(loop):
    """
    入力を受け取り、接続されている全てのクライアントにメッセージを送信する関数。
    この関数は別スレッドで実行されます。
    """
    asyncio.set_event_loop(loop)
    while True:
        user_input = input("Enter message to send to clients: ")
        asyncio.run_coroutine_threadsafe(send_message_to_clients(user_input), loop)

@app.on_event("startup")
async def startup_event():
    """
    アプリケーションの起動時に実行されるイベントハンドラ。
    入力スレッドを開始します。
    """
    loop = asyncio.get_event_loop()
    thread = threading.Thread(target=input_thread_func, args=(loop,), daemon=True)
    thread.start()





if __name__ == "__main__":
    uvicorn.run("main:app", host="localhost", port=7070, reload=True)