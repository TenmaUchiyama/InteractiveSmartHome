import httpx
from dotenv import load_dotenv
load_dotenv("../../.env")
import sys
import os

# workspace を sys.path に追加
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from agents.device_filter_agent.no_tool_filter_nodes import getDeviceInDirection



# url = "http://127.0.0.1:7070/device/operate"
# payload = [{'id': '0cbbbed7-ff3c-4932-878d-63a0af564aa8', 'state': True, 'intensity': 100, 'color': {'r': 255, 'g': 255, 'b': 255}}]




# response = httpx.post(url, json=payload)
# print(response.status_code)

url = "http://127.0.0.1:7070/device/direction"
payload = {"direction": "Back", "order": "proximity"}



res = getDeviceInDirection.invoke({
    "params": {
        "direction": "Back",
        "order": "proximity",
        "range": 0.0
    }
})


print(res)

# response = httpx.post(url, json=payload)

# print(response.status_code)
# print(response.json())
# devices = response.json()["devices"]

# print("==========================")
# print(devices[0])

# operate = [
#     {
#         "id" : devices[0]["id"],
#         "state" : True,
#         "intensity" : 100, 
#         "color" : {"r": 255, "g": 0, "b": 255}
#     }
# ]


# url = "http://localhost:7070/operate"
# response = httpx.post(url, json=operate)
# print(response.status_code)

# print(response.json())
