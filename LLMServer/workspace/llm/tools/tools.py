
import httpx
from langchain.tools import tool
from typing import Annotated, Any, Dict, List, Union
from uuid import UUID
from llm.utils.mqtt import MQTTPublisher
import os
from llm.models import DBDeviceData
from llm.utils.mqtt import mqtt_publisher




MR_SERVER_URL = os.getenv("MR_SERVER_API")
DB_SERVER_URL = os.getenv("DB_SERVER_API")



received_devices = []

@tool
def getDevices(
    order: Annotated[str, """
    - order (str): The sorting method for devices. Possible values are:
          - "proximity" (closest first, default)
          - "right" (when you need to sort the devices from right to left)
          - "high" (when you need to sort the devices from high to low with y value)
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

        print()
        print("=====================[TOOL] getDevices=====================")
        print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            response_data["order"] = order
            
            if response_data.get("status") == "success":
                print("===================================================")
                print()
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
          - "right" (when you need to sort the devices from right to left)
          - "high" (when you need to sort the devices from high to low with y value)
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
                print("================================================")
                print()
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
          - "right" (when you need to sort the devices from right to left)
          - "high" (when you need to sort the devices from high to low with y value)
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
        "function": "sight",
        "isInFov" : in_sight,
        "order": order ,
        "range" : range,
    }
        

        
        print(f"Sending POST request to {MR_SERVER_URL}/ with body: {request_body}")
        response = httpx.post(MR_SERVER_URL, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            global received_devices
            received_devices = response_data
            response_data["order"] = order
            if response_data.get("status") == "success":
                print(response_data)
                
                print("================================================")
                print()
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}


@tool
def sortDevices(
    device_list: Annotated[List[Dict[str, Any]], """
        A list of device dictionaries to be sorted.
        Each dictionary must contain 'id', 'distance_from_user', and 'angle'.
                       
        e.g . [{'id': 'test_id', 'name': 'test_led', 'position': {'x': '0', 'y': '0', 'z': '0'}, 'distance_from_user': '0', 'angle': '0'}]
    """],
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





@tool
def operateDevice(devices: Annotated[ Union[List[Dict[str, Any]], Dict[str, Any]],  
    "A list of device control data. Each item should include 'id', 'state', 'intensity', and optionally 'color'."]) -> str:
    """
    This function operates devices based on provided control data.
    Example input:
    [
        {
            "id": "test_light_id",
            "state": true,
            "intensity": 100,
            "color": {"r": 255, "g": 255, "b": 255}  # Optional
        },
         {
            "id": "test_fan_id",
            "state": true,
            "intensity": 100,
           
        }
    ]
    """
    try:
        print()
        print("=====================[TOOL] operateDevice=====================")
        # デバイスデータを取得
        response = httpx.get(f"{DB_SERVER_URL}/device/get-all")
        if response.status_code != 200:
            print("デバイスデータの取得に失敗しました。")
            return "Failed to retrieve device data."
       
        all_devices: List[DBDeviceData] = [DBDeviceData(**device) for device in response.json()]
        
        # 送信対象のデバイスデータをフィルタリング
     
        filtered_devices = []
        if isinstance(devices, dict):  # `devices` がオブジェクトの場合
            matching_device = next(
                (d for d in all_devices if d.device_id == devices["id"]), None
            )
            if matching_device:
                filtered_devices.append((devices, matching_device))
        elif isinstance(devices, list):  # `devices` が配列の場合
            for device in devices:
                matching_device = next(
                    (d for d in all_devices if d.device_id == device["id"]), None
                )
                if matching_device:
                    filtered_devices.append((device, matching_device))
        else:

            print("無効なデバイスデータ形式です。")
            return "Invalid device data format."

        
        if not filtered_devices:
            print("該当するデバイスデータが見つかりませんでした。")
            return "No matching devices found."
        
        # 各デバイスにデータを送信
        results = {"success": [], "failed": []}
        for device_control, matching_device in filtered_devices:
            mqtt_topic = matching_device.mqtt_topic
            payload = {
                "id": device_control["id"],
                "state": device_control["state"],
            }

            if "intensity" in device_control:
                payload["intensity"] = device_control["intensity"]
                
            if "color" in device_control:
                payload["color"] = device_control["color"]
            
            try:
          
                
                print(f"Sending payload to {mqtt_topic}: {payload}")
                res = mqtt_publisher.send_data(mqtt_topic, payload)
                if res is False: 
                    results["failed"].append((mqtt_topic, device_control["id"]))
                    continue
                print(f"Message successfully sent to {matching_device.device_name} on topic {mqtt_topic}.")
                results["success"].append((mqtt_topic, device_control["id"]))
            except Exception as mqtt_error:
                print(f"Failed to send message to {mqtt_topic}: {mqtt_error}")
                results["failed"].append((mqtt_topic, device_control["id"]))

        # 結果の集計
        summary = f"Operation completed. Success: {len(results['success'])}, Failed: {len(results['failed'])}."
        print(summary)
        print("===================================================")
        print()
        return summary

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
