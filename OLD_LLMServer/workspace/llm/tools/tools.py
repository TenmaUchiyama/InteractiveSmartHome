<<<<<<< HEAD

import json
import httpx
from langchain.tools import tool
from typing import Annotated, Any, Dict, List, Optional, Union
from pydantic import BaseModel, Extra
import os
=======
from dataclasses import Field
import httpx
from langchain.tools import tool
from langchain_openai import ChatOpenAI
from typing import Annotated, Dict, List
import os 
from utils.operator.TestOperator import TestOperator
base_url = os.getenv("XR_SERVER_API")
>>>>>>> stack




<<<<<<< HEAD
MR_SERVER_URL = os.getenv("MR_SERVER_API")
DB_SERVER_URL = os.getenv("DB_SERVER_API")



received_devices = []

@tool
def getDevices(
    device_type : Annotated[str, """
    - specify device type to get. Possible devices are: 
                            - Light
                            - Curtain
    """] = "Light",
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (when you need to sort the devices from right to left)
          - "high" (when you need to sort the devices from high to low with z value)
    """] = "proximity",
    range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          Optional, default is no range limitation.
    """] = None,

) -> Dict:
    

    """
    This function retrieves all devices in the room.
    return value is a list of devices in which its order is according to the argument.
"""




    request_body = {
        # "function": "all-device",
        # "device_type": device_type,
        "order": order,
        "range" : range
    }
    
    try:

        print()
        print("=====================[TOOL] getDevices=====================")
        print(f"Sending POST request to {MR_SERVER_URL}/all with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        global received_devices
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            response_data["order"] = order
            received_devices = response_data["devices"]
            if response_data.get("status") == "success":
                print("===================================================")
                print()
                return response_data
                
=======


@tool
def getDeviceInFov(
    isInFov: Annotated[bool, """True if retrieving devices currently visible to the user. Default: True"""],
    order: Annotated[str, """Sorting order of the devices. Options: 'proximity' (closest first), 'right' (from left to right), 'high' (from lowest to highest). Default: 'proximity'"""],
    range: Annotated[float, """Optional distance range in meters. Use 0.0 if not specifying a limit."""] 
) -> Dict:
    """
    Retrieves devices currently within the user's visible area.
    Use when the instruction involves visual terms (e.g., "devices I can see", "the nearest device", "rightmost one", "leftmost").
    """
    
    try:
        # print("\n[TOOL FOV] getDevicesInFov")
        
        request_body = {
            "isInFov": isInFov,
            "order": order
        }

        if range != 0.0:
            request_body["range"] = range

        # print(f"Sending POST request to {base_url}/fov with body: {request_body}")
        response = httpx.post(f"{base_url}/device/fov", json=request_body)
        print("Received Devices")

        if response.status_code == 200:
            response_data = response.json()

            # `param` が存在しない場合は作成する
            response_data.setdefault("param", {})

            # `param` に値を追加
            response_data["param"]["filter_type"] = "fov"
            response_data["param"]["isInFov"] = isInFov
            response_data["param"]["order"] = order
            if range != 0.0:
                response_data["param"]["range"] = range

            if response_data.get("status") == "success":
                # print(f"[TOOL FOV] {response_data}")
                return response_data
>>>>>>> stack
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
<<<<<<< HEAD
    except Exception as e:
        return {"status": "error", "message": str(e)}
    
=======

    except Exception as e:
        return {"status": "error", "message": str(e)}

>>>>>>> stack



@tool
<<<<<<< HEAD
def getDevicesUserAngle(
    device_type : Annotated[str, """
    - specify device type to get. Possible devices are: 
                            - Light
                            - Curtain
    """] = "Light",
    direction: Annotated[str, """
    - direction (str): The direction to search for devices. Possible values are:
          - "Front" (devices in front of the user)
          - "Back" (devices behind the user)
          - "Left" (devices to the left of the user)
          - "Right" (devices to the right of the user)
    """] = "Front",
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (when you need to sort the devices from right to left)
          - "high" (when you need to sort the devices from high to low with  z value)
    """] = "proximity",
    range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          Optional, default is no range limitation.
    """] = None
) -> Dict:
    

    """
    This function retrieves devices based on the user's direction and order.

    return value is a list of devices in which its order is according to the argument.
    """
    print()
    print("=====================[TOOL] getDevicesUserAngle=====================")
    

    request_body = {
        # "function": "direction",
        # "device_type" : device_type,
=======
def getDeviceInDirection(
    direction: Annotated[str, """Direction relative to the user's body. Must be one of: 'Front', 'Back', 'Left', 'Right'."""],
    order: Annotated[str, """Sorting order of devices. Options: 'proximity' (closest first), 'right' (from left to right), 'high' (from lowest to highest). Default: 'proximity'"""],
    range: Annotated[float, """Optional distance range in meters. Use 0.0 if not specifying a limit."""] 
) -> Dict:
    """
    Retrieves devices based on a clear body-relative direction.
    Use when the user explicitly references a direction relative to their body (e.g., "behind me", "to my left", "on my right").
    """


    request_body = {
>>>>>>> stack
        "direction": direction,
        "order": order,
        "range" : range
    }
    
    try:
<<<<<<< HEAD
        print(f"Sending POST request to {MR_SERVER_URL}/direction with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        global received_devices
        if response.status_code == 200:
            response_data = response.json()

            received_devices = response_data["devices"]
            print(f"Response from server: {response_data}")
            response_data["order"] = order
            
            if response_data.get("status") == "success":
                print("================================================")
                print()
                return response_data

=======
        print(f"Sending POST request to {base_url}/device/direction with body: {request_body}")
        response = httpx.post(f"{base_url}/direction", json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            response_data.setdefault("param", {})
            if response_data.get("status") == "success":
                response_data["param"]["filter_type"] = "direction"
                response_data["param"]["direction"] = direction
                response_data["param"]["order"] = order
                if range != 0:
                    response_data["param"]["range"] = range

                return response_data
>>>>>>> stack
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    



<<<<<<< HEAD
@tool
def getDevicesInSights(
    # device_type : Annotated[str, """
    # - specify device type to get. Possible devices are: 
    #                         - Light
    #                         - Curtain
    # """] = "Light",
    in_sight: Annotated[bool, """
    - in_sight (bool): If True, returns devices that are within the user's line of sight.
    """] = "True",
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (when you need to sort the devices from right to left)
          - "high" (when you need to sort the devices from high to low with  z value)
    """] = "proximity",

       range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          Optional, default is no range limitation.
    """] = None


) -> Dict:
    """
    This function retrieves devices that are within the user's line of sight.
    return value is a list of devices in which its order is according to the argument.
    """
    
    try:
        print()
        print("=====================[TOOL] getDevicesInSights================")
        request_body = {
        # "function": "sight",
        # "device_type" : device_type, 
        "isInFov" : in_sight,
        "order": order ,
        "range" : range,
    }
        

        
        print(f"Sending POST request to {MR_SERVER_URL}/fov with body: {request_body}")
        response = httpx.post(f"{MR_SERVER_URL}/fov", json=request_body)
        global received_devices
        if response.status_code == 200:
            response_data = response.json()
            
            received_devices = response_data["devices"]
            response_data["order"] = order
            if response_data.get("status") == "success":
                print(response_data)
                
                print("================================================")
                print()
=======


@tool
def getDevices(
    order: Annotated[str, """Sorting order of devices. Options: 'proximity' (closest first), 'right' (from left to right), 'high' (from lowest to highest). Default: 'proximity'"""],
    range: Annotated[float, """Optional distance range in meters. Use 0.0 if not specifying a limit."""] 
) -> Dict:
    """
    Retrieves all available devices without any spatial constraints.
    Use for general commands without spatial conditions (e.g., "all lights", "everything", "around me").
    """
    try:
        # print()
        # print("=====================[TOOL] getDevices================")
        request_body = {
        "order": order ,
        }



        
        if range != 0: 
            request_body["range"] = range
        
        # print(f"Sending POST request to {base_url}/all with body: {request_body}")
        response = httpx.post(f"{base_url}/device/all", json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            response_data.setdefault("param", {})
            if response_data.get("status") == "success":
                print(response_data)
                response_data["param"]["filter_type"] = "all"
                response_data["param"]["order"] = order
                if range != 0:
                    response_data["param"]["range"] = range
                # print("================================================")
                # print()
>>>>>>> stack
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
<<<<<<< HEAD


@tool
def sortDevices(

    order: Annotated[str, """
      The sorting method for devices. Possible values are:
              - "proximity" (closest first, default)
              - "right" (sort from right to left based on angle)
    """],
) -> List[Dict[str, Any]]:
    """
    This function sorts a list of device data based on specific criteria. 

    
    Returns:
    - A sorted list of devices.
    """
    print()
    print("=====================[TOOL] sortDevices=====================")
    # Ensure `devices` is provided
    global received_devices
    device_list = received_devices
    if not device_list or not isinstance(device_list, list):
        return "The `device_list` argument is required and must be a list of device dictionaries."

    try:
        # Validate required fields in each device
        required_fields = {"id", "distance_from_user", "angle"}
        for device in device_list:
            if not required_fields.issubset(device.keys()):
                raise ValueError(
                    f"Device {device.get('id', 'unknown')} is missing required fields."
                )

        # Sorting logic based on the 'order' parameter
        if order == "proximity":
            sorted_devices = sorted(device_list, key=lambda x: float(x["distance_from_user"]))
        elif order == "right":
            sorted_devices = sorted(device_list, key=lambda x: float(x["angle"]))
        else:
            raise ValueError(
                f"Invalid order parameter: {order}. Supported values are 'proximity' and 'right'."
            )
        outputDevice = {
            "status" : "success",
            "devices" : sorted_devices,
            "order" : order

        }

        print("===================================================")
        print()
        return outputDevice

    except Exception as e:
        raise ValueError(f"An error occurred during sorting: {str(e)}")



class DeviceControlData(BaseModel):
    id: str
    state: bool
    intensity: int
    color: Optional[Dict[str, int]]

@tool
def operateDevice(devices: Annotated[ Union[List[DeviceControlData]],  
    "A list of device control data. Each item should include 'id', 'state', 'intensity', and optionally 'color'."]) -> str:
    """
=======
    



@tool
def getDeviceAroundFurniture(
    furniture_type: Annotated[str, """Type of furniture serving as the reference point. Must be 'TV' or 'TABLE'."""],
    range: Annotated[float, """Optional distance range around the furniture in meters. Default is 5 meters if not specified."""]
) -> Dict:
    """
    Retrieves devices around specified furniture items.
    Use when the user's instruction explicitly refers to furniture (e.g., "near the TV", "on the table", "around the table").
    """
    try:
        request_body = {
            "furnitureType": furniture_type,
             "range" : 5 if range is None else range,
        }
        print(f"Sending POST request to {base_url}/around_furniture with body: {request_body}")
        response = httpx.post(f"{base_url}/furniture/get", json=request_body)
        output = {}
        if response.status_code == 200:
            response_data = response.json()
            output.setdefault("param", {})
            if response_data.get("status") == "success":
                output["param"]["filter_type"] = "around_furniture"
                output["status"] = response_data.get('status')
                output["devices"] = response_data.get("devices")
                output["param"]["furniture_data"] = response_data["furniture_data"]
                return output
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}







class RGBColor(BaseModel):
    r: int = Field(..., description="Red (0-255)")
    g: int = Field(..., description="Green (0-255)")
    b: int = Field(..., description="Blue (0-255)")

class DeviceControlData(BaseModel):
    id: str = Field(..., description="Device ID to control.")
    state: bool = Field(..., description="Power state. True = ON, False = OFF.")
    intensity: int = Field(..., description="Brightness level from 0 to 100.")
    color: RGBColor = Field(..., description="Color as RGB values.")
testOperator = TestOperator()



@tool
def operateDevice(   devices: List[DeviceControlData]) -> str:
    """
    COLOR MUUUUUSSSSTT BE INCLUDED
>>>>>>> stack
    This function operates devices based on provided control data.
    Example input:
    [
        {
            "id": "test_light_id",
            "state": true,
            "intensity": 100,
<<<<<<< HEAD
            "color": {"r": 255, "g": 255, "b": 255}  # Optional
=======
            "color": {"r": 255, "g": 255, "b": 255}  # default is white
>>>>>>> stack
        },
         {
            "id": "test_curtain_id",
            "state": true,
            "intensity": 100,
<<<<<<< HEAD
=======
            "color": {"r": 255, "g": 255, "b": 255}
>>>>>>> stack
        }
    ]


    **For curtain, 0 = open, 100 = close    
    """


    try:

        print()
<<<<<<< HEAD
        print("=====================[TOOL] operateDevice=====================")
        # デバイスデータを取得
        return "成功しました！"
        print("DEVICE DATA: ", devices)
=======
        print("=====================[OPERATOR TOOL] operateDevice=====================")
        # デバイスデータを取得
        convert_data =  [device.dict() for device in devices]
        response = testOperator.send_operate_request(convert_data)
        print("RESPONSE: ", response)
        return  f"RESULT: {response}"
>>>>>>> stack
        sending_data = json.dumps([device.dict() for device in devices])



        response = httpx.post(f"{MR_SERVER_URL}/operate", data=sending_data)


        
        response_data = response.json()
        
        print(f"Response from server: {response_data}")

        if response.status_code != 200:
            print("デバイスデータの取得に失敗しました。")
            return "Failed to retrieve device data."

        print("===================================================")

        return response_data
    
    except Exception as e:
<<<<<<< HEAD

=======
        print("ERROR OCCURRED DURING OPERATION TOOL: ", e)
>>>>>>> stack
        return f"エラーが発生しました: {e}"



<<<<<<< HEAD

















# Mqtt
# @tool
# def operateDevice(devices: Annotated[ Union[List[Dict[str, Any]], Dict[str, Any]],  
#     "A list of device control data. Each item should include 'id', 'state', 'intensity', and optionally 'color'."]) -> str:
#     """
#     This function operates devices based on provided control data.
#     Example input:
#     [
#         {
#             "id": "test_light_id",
#             "state": true,
#             "intensity": 100,
#             "color": {"r": 255, "g": 255, "b": 255}  # Optional
#         },
#          {
#             "id": "test_curtain_id",
#             "state": true,
#             "intensity": 100,
#         }
#     ]


#     **For curtain, 0 = open, 100 = close    
#     """
#     try:
#         print()
#         print("=====================[TOOL] operateDevice=====================")
#         # デバイスデータを取得
#         response = httpx.get(f"{DB_SERVER_URL}/device/get-all")
#         if response.status_code != 200:
#             print("デバイスデータの取得に失敗しました。")
#             return "Failed to retrieve device data."
       
#         all_devices: List[DBDeviceData] = [DBDeviceData(**device) for device in response.json()]
        
#         # 送信対象のデバイスデータをフィルタリング
     
#         filtered_devices = []
#         if isinstance(devices, dict):  # `devices` がオブジェクトの場合
#             matching_device = next(
#                 (d for d in all_devices if d.device_id == devices["id"]), None
#             )
#             if matching_device:
#                 filtered_devices.append((devices, matching_device))
#         elif isinstance(devices, list):  # `devices` が配列の場合
#             for device in devices:
#                 matching_device = next(
#                     (d for d in all_devices if d.device_id == device["id"]), None
#                 )
#                 if matching_device:
#                     filtered_devices.append((device, matching_device))
#         else:

#             print("無効なデバイスデータ形式です。")
#             return "Invalid device data format."

        
#         if not filtered_devices:
#             print("該当するデバイスデータが見つかりませんでした。")
#             return "No matching devices found."
        
#         # 各デバイスにデータを送信
#         results = {"success": [], "failed": []}
#         for device_control, matching_device in filtered_devices:
#             mqtt_topic = matching_device.mqtt_topic
#             payload = {
#                 "id": device_control["id"],
#                 "state": device_control["state"],
#             }

#             if "intensity" in device_control:
#                 payload["intensity"] = device_control["intensity"]
                
#             if "color" in device_control:
#                 payload["color"] = device_control["color"]
            
#             try:
          
                
#                 print(f"Sending payload to {mqtt_topic}: {payload}")
#                 res = mqtt_publisher.send_data(mqtt_topic, payload)
#                 if res is False: 
#                     results["failed"].append((mqtt_topic, device_control["id"]))
#                     continue
#                 print(f"Message successfully sent to {matching_device.device_name} on topic {mqtt_topic}.")
#                 results["success"].append((mqtt_topic, device_control["id"]))
#             except Exception as mqtt_error:
#                 print(f"Failed to send message to {mqtt_topic}: {mqtt_error}")
#                 results["failed"].append((mqtt_topic, device_control["id"]))

#         # 結果の集計
#         summary = f"Operation completed. Success: {len(results['success'])}, Failed: {len(results['failed'])}."
#         print(summary)
#         print("===================================================")
#         print()
#         return summary

#     except Exception as e:
#         print(f"エラーが発生しました: {e}")
#         return "An error occurred during device operation."
    
=======
>>>>>>> stack
