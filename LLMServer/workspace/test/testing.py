import httpx

url = "http://localhost:7070/fov"
payload = {"isInFov": True, "order": "proximity"}

response = httpx.post(url, json=payload)

print(response.status_code)
print(response.json())
devices = response.json()["devices"]

print("==========================")
print(devices[0])

operate = [
    {
        "id" : devices[0]["id"],
        "state" : True,
        "intensity" : 100, 
        "color" : {"r": 255, "g": 0, "b": 255}
    }
]


url = "http://localhost:7070/operate"
response = httpx.post(url, json=operate)
print(response.status_code)

print(response.json())
