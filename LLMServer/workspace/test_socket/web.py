import asyncio
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from starlette.middleware.cors import CORSMiddleware
import uvicorn
from pydantic import BaseModel, ValidationError
from typing import Union, Literal, List, Dict
import json
import threading
from test_func import test_func

app = FastAPI()

# CORS Middleware Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Adjust as needed
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Sample Data
sample_data = {
    "id": 1,
    "name": "sample data",
    "value": 42
}




# Manage connected clients
connected_clients = set()
client_lock = threading.Lock()
# Define the Message Schema
class Message(BaseModel):
    request_type: Literal["llm_agent", "device", "function"]
    data: Dict  # The payload can vary based on request_type

# Handlers for each request_type
async def handle_llm(data: Dict) -> Dict:
    
    test_func()
    return {"status": "success", "llm_response": data}

async def handle_device(data: Dict) -> Dict:
    # Implement your device handling logic here
    # For example, return device metadata
    device_metadata = {
        "id": data.get("id", 0),
        "type": "sensor",
        "status": "active"
    }
    return {"status": "success", "device_metadata": device_metadata}

async def handle_function(data: Dict) -> Dict:
    # Implement your function handling logic here
    # For example, update sample_data
    new_value = data.get("value")
    if new_value is not None:
        sample_data["value"] = new_value
        return {"status": "success", "updated_data": sample_data}
    else:
        return {"status": "error", "message": "No 'value' provided."}

# Mapping of request_type to handler functions
request_handlers = {
    "llm_agent": handle_llm,
    "device": handle_device,
    "function": handle_function
}



@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    with client_lock:
        connected_clients.add(websocket)
    client_address = websocket.client.host
    print(f"Client connected: {client_address}")
    try:
        while True:
            raw_data = await websocket.receive_text()
            print(f"Received from {client_address}: {raw_data}")
            try:
                message = Message.parse_raw(raw_data)
                handler = request_handlers.get(message.request_type)
                if handler:
                    response = await handler(message.data)
                else:
                    response = {"status": "error", "message": f"Unknown request_type: {message.request_type}"}
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
    Receive input from the server console and broadcast to all clients.
    Runs in a separate thread.
    """
    while True:
        user_input = input("Enter message to send to clients: ")
        # Construct the message with request_type "function" as an example
        message = {
            "request_type": "function",
            "data": {
                "value": user_input  # Assuming user_input is intended to update 'value'
            }
        }
        message_text = json.dumps(message)
        
        with client_lock:
            clients = list(connected_clients)
        for ws in clients:
            # Schedule the send_text coroutine in the event loop
            asyncio.run_coroutine_threadsafe(ws.send_text(message_text), loop)

@app.on_event("startup")
async def startup_event():
    """
    Start the input thread when the application starts.
    """
    loop = asyncio.get_event_loop()
    thread = threading.Thread(target=input_thread, args=(loop,), daemon=True)
    thread.start()

if __name__ == "__main__":
    uvicorn.run("web:app", host="localhost", port=7070, reload=True)
