import httpx
from langchain.tools import tool
from langchain_openai import ChatOpenAI
from typing import Annotated, Dict
import os 

base_url = os.getenv("XR_SERVER_API")






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
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}

    except Exception as e:
        return {"status": "error", "message": str(e)}




@tool
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
        "direction": direction,
        "order": order,
        "range" : range
    }
    
    try:
        print(f"Sending POST request to {base_url}/device/direction with body: {request_body}")
        response = httpx.post(f"{base_url}/device/direction", json=request_body)
        
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
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    





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
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    



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
<<<<<<< HEAD
        response = httpx.post(f"{base_url}/furniture/get", json=request_body)
        output = {}
=======
        response = httpx.post(f"{base_url}/around_furniture", json=request_body)
>>>>>>> 1bc836a9da806b4f10ab8613f7735ae8dd36c185
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




if "__main__" == __name__:
    print(getDeviceInFov.invoke({"isInFov": True, "order": "proximity", "range": 0.0}))

