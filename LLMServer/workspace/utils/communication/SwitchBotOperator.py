import time
import requests
from utils.communication.DeviceOperator import DeviceOperator




class SwitchBotOperator(DeviceOperator):
    def __init__(self, token: str):
        self.token = token
        self.headers = {
            "Authorization": self.token,
            "Content-Type": "application/json; charset=utf8"
        }

    def send_command(self, device_id, command, parameter="default", command_type="command"):
        url = f"https://api.switch-bot.com/v1.0/devices/{device_id}/commands"
        body = {
            "command": command,
            "parameter": parameter,
            "commandType": command_type
        }
        response = requests.post(url, headers=self.headers, json=body)
        print(f"[{command}] to {device_id} => {response.status_code}: {response.text}")
        return response

    def send_operate_request(self, devices):
        for device in devices:
            device_id = device["id"]

            # ON/OFF
            if "state" in device:
                command = "turnOn" if device["state"] else "turnOff"
                self.send_command(device_id, command)
                time.sleep(0.3)

            # 明るさ
            if "intensity" in device:
                intensity = str(device["intensity"])
                self.send_command(device_id, "setBrightness", intensity)
                time.sleep(0.3)

            # 色変更
            if "color" in device:
                color = device["color"]
                rgb_str = f"{color['r']}:{color['g']}:{color['b']}"
                self.send_command(device_id, "setColor", rgb_str)
                time.sleep(0.3)
