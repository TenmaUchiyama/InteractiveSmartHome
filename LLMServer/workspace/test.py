from agents.device_filter_agent.filter_tool import getDeviceAroundFurniture



test = getDeviceAroundFurniture.invoke({"furniture_type": "TV", "range": 5})


print(test)