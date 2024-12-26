from uuid import UUID
import httpx
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage, ToolMessage
from langgraph.graph import StateGraph, START, END 
from langgraph.graph.message import add_messages
from typing import Dict, List, Tuple
from dataclasses import dataclass
from langchain.tools import tool
from typing import Annotated
from typing_extensions import TypedDict 
from IPython.display import display, Image
import json
import os 
import dotenv
from langchain.globals import set_debug
from langchain.callbacks.base import BaseCallbackHandler
from dataclasses import dataclass
from langgraph.graph import StateGraph, START, END
from langgraph.graph.message import add_messages
from typing import TypedDict 
from IPython.display import Image,display
import httpx
from langchain.callbacks.base import BaseCallbackHandler
from llm.utils.mqtt import mqtt_publisher 
from dotenv import load_dotenv

load_dotenv()




MR_SERVER_URL = os.getenv("MR_SERVER_API")
DB_SERVER_URL = os.getenv("DB_SERVER_API")



class CustomCallbackHandler(BaseCallbackHandler):
    def on_llm_start(self, serialized, prompts, **kwargs):
        print("\n--- 送信するテキスト ---")
        for i, prompt in enumerate(prompts):
            print(f"Prompt {i + 1}: {prompt}")
        print("\n")

    def on_llm_end(self, response, **kwargs):
        print("\n--- LLMからのレスポンス ---")
        print(response)
        print("\n")

@dataclass
class DeviceData:
    device_id: UUID
    device_name: str
    distance_from_user: float
    device_position: Tuple[float, float, float]

@dataclass 
class DBDeviceData:
    device_id: str
    device_name: str
    device_type: str
    mqtt_topic: str
    device_position: Dict[str, float]



custom_handler = CustomCallbackHandler()


debug = False
llm = ChatOpenAI(model="gpt-4o-mini", callbacks= [custom_handler] if debug else [] , verbose="True")




class State(TypedDict):
    input_message : HumanMessage 
    messages : Annotated[list, add_messages]
    devices : list 
    furnitures : list 
    




@tool
def getDevices(
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (right to left order)
          - "high" (high to low order with y value)
    """],
    range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          Optional, default is no range limitation.
    """]
) -> Dict:
    
    """
    This function retrieves all devices.
    return value is a list of devices in which its order is according to the argument.
    """
    request_body = {
        "function": "all-device",
        "order": order,
        "range" : range
    }
    
    try:
        print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            response_data["order"] = order
            
            if response_data.get("status") == "success":
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    



@tool
def getDevicesUserAngle(
    direction: Annotated[str, """
    - direction (str): The direction to search for devices. Possible values are:
          - "Front" (devices in front of the user)
          - "Back" (devices behind the user)
          - "Left" (devices to the left of the user)
          - "Right" (devices to the right of the user)
    """],
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (right to left order)
          - "high" (high to low order with y value) 
    """] ,
    range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          Optional, default is no range limitation.
    """]
) -> Dict:
    

    """
    This function retrieves devices based on the user's direction and order.

    return value is a list of devices in which its order is according to the argument.
    """
    request_body = {
        "function": "direction",
        "dir": direction,
        "order": order,
        "range" : range
    }
    
    try:
        print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            response_data["order"] = order
            
            if response_data.get("status") == "success":
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    



@tool
def getDevicesInSights(
    in_sight: Annotated[bool, """
    - in_sight (bool): If True, returns devices that are within the user's line of sight.
    """],
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (right to left order).
          - "high" (high to low order with y value) 
    """],

       range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          *Optional, if range is not specified, please input -1.*
    """] 


) -> Dict:
    """
    This function retrieves devices that are within the user's line of sight.
    return value is a list of devices in which its order is according to the argument.
    """
    try:
        request_body = {
        "function": "sight",
        "isInFov" : in_sight,
        "order": order ,
        "range" : range if range != -1 else None,
    }
        

        print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            response_data["order"] = order
            if response_data.get("status") == "success":
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}





tool_map = {
    "getDevices" : getDevices, 
    "getDevicesUserAngle" : getDevicesUserAngle, 
    "getDevicesInSights" : getDevicesInSights,
}







@tool 
def operateDevice(device_ids: Annotated[List[str],  
    "A list of device IDs to operate. The number of IDs will be CAREFULLY adjusted based on user input. "]) -> str:
    """
    This function operates devices based on their IDs.
    """
    print("=================FROM OPERATE DEVICE================")
    print(device_ids)
    print("====================================")


    try:
        # デバイスIDリストを送信して全デバイスのデータを取得する
        response = httpx.get(f"{DB_SERVER_URL}/device/get-all")
       
        if response.status_code != 200:
            print("デバイスデータの取得に失敗しました。")
            return "Failed to retrieve device data."
        
        all_devices: List[DBDeviceData] = [DBDeviceData(**device) for device in response.json()]
        
        # 取得した全デバイスのデータの中から、指定したデバイスIDのデータを照合して抽出
        filtered_devices: List[DBDeviceData] = [
            device for device in all_devices 
            if device.device_id in device_ids
        ]
        
        if not filtered_devices:
            print("該当するデバイスデータが見つかりませんでした。")
            return "No matching devices found."
        
        # それぞれのデバイスについて、mqtt_publisherを使ってトピックにメッセージを送信する
        for device in filtered_devices:
            print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
            print("Sending to ", device.device_name)
            print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
            mqtt_topic = device.mqtt_topic
            mqtt_publisher.send_data(mqtt_topic, "Send")
        print("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<")
        return "Successfully sent messages to the MQTT topics."
    
    except Exception as e:
        print(f"エラーが発生しました: {e}")
        return "An error occurred during device operation."
    


llm_with_tool = llm.bind_tools([getDevices, getDevicesUserAngle, getDevicesInSights], strict=True)

llm_with_operation = llm.bind_tools([operateDevice], strict=True)




def function_agent(state : State) -> State: 
    user_input = state["input_message"] 
    msgs = state["messages"]
    
    response = llm_with_tool.invoke([user_input, *msgs])    
    state["messages"].append(response)
    return state


def tool_node(state:State) -> State: 
    last_msg = state["messages"][-1]
    for tCall in last_msg.tool_calls: 
        tool_func = tool_map[tCall["name"]]    
        res = tool_func.invoke(tCall["args"])
        state["messages"].append(ToolMessage(content=res, tool_call_id=tCall["id"]))
        state["devices"] = res
    return state


def router(state : State) -> ["tool_node","device_select_agent"]:
   
    last_message = state["messages"][-1] 

    if last_message.tool_calls:
        return "tool_node"
    else:

        return "device_select_agent"

def device_select_agent(state:State) -> State: 
    
    devices_data = state["devices"]
 
    system_msg = SystemMessage(content= f"""
You are a **Device Selection Agent** for controlling smart home devices. 
Your role is to **select the most appropriate device(s)** from a provided list to fulfill the user's command. 

---

**Data Provided by Previous Agent:**
- **status**: Success or error status of the previous process.
- **body**: List of devices with attributes like "id", "type", "position", etc.
- **order**: How the device list is sorted (e.g., proximity, right, high).

---

### **Order Descriptions**
- **proximity**: Devices sorted from closest to farthest.
- **right**: Devices sorted from rightmost to leftmost.
- **high**: Devices sorted from highest to lowest on the y-axis.

---

### **Selection Rules**
1. Select the first device in the list according to **order**.
2. If the user's command implies multiple devices (e.g., "all devices"), select all devices matching the criteria.

---


expected output: Array of ids. For example [device1, device2, .... ]

**Device List**: {devices_data["data"]}
**Order**: {devices_data["order"]}
""")

    print("Devices Data: ", devices_data)
    input_msg = [state["input_message"], system_msg]
    res = llm_with_operation.invoke(input_msg)    
    print("====================================")
    print(res.tool_calls)
    for tCall in res.tool_calls:
        print("ARGS: ", tCall["args"])
        arg = tCall["args"]
        op_res = operateDevice.invoke(arg)
        state["messages"].append(ToolMessage(content=res, tool_call_id=tCall["id"]))  

    
    return state
    
    
g = StateGraph(State)
g.add_node("function_agent", function_agent)
g.add_node("tool_node", tool_node)
g.add_node("device_select_agent", device_select_agent)



g.set_entry_point("function_agent")
g.add_conditional_edges("function_agent", router)
g.add_edge("tool_node", "function_agent")
g.set_finish_point("device_select_agent")

runner = g.compile()




def invoke_llm_agent(user_input: str) -> str:
    state: State = {
    "messages": [
        SystemMessage(content="""
        You are a **Device Retrieval Agent** in a multi-agent system for controlling smart home devices. 
        Your role is to identify and retrieve information about devices based on spatial relationships and user commands. 
        The retrieved device data must be **passed to the next agent** for further processing or control actions. 
        You do not directly control devices yourself — you only retrieve and provide the relevant device data.

        **Responsibilities of the Device Retrieval Agent:**
        1. **Interpret User Commands:** Understand user instructions and determine which devices are relevant based on their spatial relationships with the user.
        2. **Retrieve Device Data:** Use the available functions to query the system for device information.
        3. **Pass Device Data:** Pass the retrieved device information to the next agent in the system.

        **Function Priority:**
        1. **Default**: Use the `getDevicesInSights` function to retrieve devices that are within the user's line of sight.
        2. If the user's command includes explicit directional terms such as "left", "right", "front", or "back", use the `getDevicesUserAngle` function to retrieve devices in that specific direction relative to the user's perspective.

        **Interpretation Guidelines:**
        - **Implied Direction Commands:** If the user says, "Turn off the rightmost light," interpret this as a request to identify the rightmost device within the user's line of sight. Use `getDevicesInSights(order="right")`.
        - **Explicit User-Centered Commands:** If the user says, "Turn off the light on my right," interpret this as a request to identify devices to the user's right (relative to their personal perspective). Use `getDevicesUserAngle(direction="Right")`.
        - **No Direction Specified:** If the user's command does not specify a clear direction, use `getDevicesInSights(order="proximity")` to retrieve the closest device within the user's line of sight.
        """)
    ]
}


    state["input_message"] = HumanMessage(content=user_input) 

    
    res = runner.invoke(state)

    return res["messages"][-1].content


























