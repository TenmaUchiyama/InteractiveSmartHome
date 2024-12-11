
import httpx
from langchain.tools import tool
from typing import Annotated, Dict, List
from uuid import UUID
from llm.utils.mqtt import MQTTPublisher
import os
from llm.models import DBDeviceData
from llm.utils.mqtt import mqtt_publisher



MR_SERVER_URL = os.getenv("MR_SERVER_API")
DB_SERVER_URL = os.getenv("DB_SERVER_API")





@tool
def getDevices(
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (right to left order)
          - "high" (high to low order with y value)
    """] = "proximity",
    range: Annotated[float, """
    - range (float): The distance range from user to search for devices (in meters). 
          Optional, default is no range limitation.
    """] = None
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
        request_body = {
        "function": "sight",
        "isInFov" : in_sight,
        "order": order ,
        "range" : range,
    }
        print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
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
def operateDevice(device_ids: Annotated[List[str],  
    "A list of device IDs to operate. The number of IDs will be CAREFULLY adjusted based on user input. "]) -> str:
    """
    This function operates devices based on their IDs.
    """
    


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
    

# import httpx


# MR_SERVER_URL = "http://localhost:4049"
# if __name__ == "__main__":
#     try:
#         request_body = {
#         "function": "sight",
#         "args": ["True"]
#     }
#         print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
#         response = httpx.post(MR_SERVER_URL, json=request_body)
        
#         if response.status_code == 200:
#             response_data = response.json()
#             print(f"Response from server: {response_data}")
#             for device in response_data:
#                 mqtt_publisher.send_data(device['mqtt_topic'], "True")
            
#             if response_data.get("status") == "success":
#                 return response_data["status"]
#             else:
#                 return {"status": "error", "message": "Server responded with an error", "details": response_data}
#         else:
#             return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
#     except Exception as e:
#         return {"status": "error", "message": str(e)}
