from abc import ABC, abstractmethod
from utils.Mqtt import MqttPublisher 
import httpx
import os
import json


class DeviceOperator(ABC): 

    def __init__(self):
        self.mqtt_publisher = MqttPublisher("localhost", 1883) 
        self.simulation_id = os.getenv("XR_SERVER_API") + "/device/operate"
        print(self.simulation_id)
        
        
    
    

    def send_simulation_request(self, devices):
        print("Sending Operate Request to Test Server.") 
        response = httpx.post(self.simulation_id, json=devices)
        output = ""
        print("CODE: ", response.status_code)
        if response.status_code == 200:
            output = f"[Status] {response.status_code}\n[Message] {response.json()['message']}"
            print("All Devices Are Operated Successfully.")
        else:
            output = f"[Status] {response.status_code}\n[Message] Failed to Operate Devices."
            print("Failed to Operate Devices.")
        return output



    
    def send_operator(self, devices):
        all_devices = httpx.get("http://localhost:4049/device/get-all").json()
        
        mqtts = []
        switchbots = []
    
        for device in all_devices:
            conn_type = device.get("connection_type")
            if conn_type == "mqtt":
                mqtts.append(device)
            elif conn_type == "switchbot":
                switchbots.append(device)
    
        self.send_mqtt_request(mqtts)
        self.send_switchbot_request(switchbots)
    
        return all_devices


    def send_mqtt_request(self, devices): 
        for device in devices: 
            topic = getattr(device, "topic", None)
            if not topic:
                print(f"Device missing topic: {device}")
                continue
    
            try:
                payload = device.json()
            except AttributeError:
                payload = json.dumps(device)  # 辞書など別形式なら fallback
            self.mqtt_publisher.send_data(topic, payload)

    def send_switchbot_request(self, devices: List[dict]):
        url = "https://api.switch-bot.com/v1.1/devices"
        headers = {
            "Authorization": self.switchbot_token,
            "Content-Type": "application/json"
        }

        for raw in devices:
            try:
                # dict → Pydanticモデルに変換
                data = DeviceControlData(**raw)
                switchbot_id = raw.get("topic")  # 実際はDevice ID

                # 電源
                power_cmd = {
                    "command": "turnOn" if data.state else "turnOff",
                    "parameter": "default",
                    "commandType": "command"
                }
                httpx.post(f"{url}/{switchbot_id}/commands", headers=headers, json=power_cmd)

                # 明るさ
                brightness_cmd = {
                    "command": "setBrightness",
                    "parameter": str(data.intensity),
                    "commandType": "command"
                }
                httpx.post(f"{url}/{switchbot_id}/commands", headers=headers, json=brightness_cmd)

                # 色
                rgb_str = f"{data.color.r}:{data.color.g}:{data.color.b}"
                color_cmd = {
                    "command": "setColor",
                    "parameter": rgb_str,
                    "commandType": "command"
                }
                httpx.post(f"{url}/{switchbot_id}/commands", headers=headers, json=color_cmd)

                print(f"SwitchBot操作完了: {switchbot_id}")

            except Exception as e:
                print(f"[ERROR] SwitchBot制御中にエラー: {e}")
            
