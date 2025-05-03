from abc import ABC, abstractmethod
from utils.communication.Mqtt import MqttPublisher 
import httpx
import os
import json


class DeviceOperator(ABC): 

    def __init__(self):
        self.mqtt_publisher = MqttPublisher("localhost", 1883) 
        self.simulation_id = os.getenv("XR_SERVER_API") + "/device/operate"
        self.switchbot_token = os.getenv("SWITCHBOT_TOKEN")
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

    def send_switchbot_request(self, devices):
        url = "https://api.switch-bot.com/v1.1/devices"
        headers = {
            "Authorization": self.switchbot_token,
            "Content-Type": "application/json"
        }

        for device in devices:
            try:
                switchbot_id = device.get("topic")
                if not switchbot_id:
                    print("[ERROR] topicが見つかりません")
                    continue

                # 電源 ON/OFF
                power_cmd = {
                    "command": "turnOn" if device.get("state") else "turnOff",
                    "parameter": "default",
                    "commandType": "command"
                }
                httpx.post(f"{url}/{switchbot_id}/commands", headers=headers, json=power_cmd)

                # 明るさ
                brightness_cmd = {
                    "command": "setBrightness",
                    "parameter": str(device.get("intensity", 100)),
                    "commandType": "command"
                }
                httpx.post(f"{url}/{switchbot_id}/commands", headers=headers, json=brightness_cmd)

                # 色指定（r, g, bをネストされたcolorから取得）
                color = device.get("color", {})
                r = color.get("r", 255)
                g = color.get("g", 255)
                b = color.get("b", 255)
                rgb_str = f"{r}:{g}:{b}"
                color_cmd = {
                    "command": "setColor",
                    "parameter": rgb_str,
                    "commandType": "command"
                }
                httpx.post(f"{url}/{switchbot_id}/commands", headers=headers, json=color_cmd)

                print(f"[OK] SwitchBot操作完了: {switchbot_id}")

            except Exception as e:
                print(f"[ERROR] SwitchBot制御中に例外: {e}")