import time
import requests
import  time, uuid, hmac, hashlib, base64

class SwitchBotOperatorSecure:
    def __init__(self, token: str, secret: str):
        self.token = token
        self.secret = secret

    def _generate_headers(self):
        nonce = str(uuid.uuid4())  # UUID文字列でOK
        t = str(int(round(time.time() * 1000)))  # UNIXミリ秒
        string_to_sign = f"{self.token}{t}{nonce}"

        # HMAC-SHA256で署名し、base64エンコード
        sign = base64.b64encode(
            hmac.new(self.secret.encode(), msg=string_to_sign.encode(), digestmod=hashlib.sha256).digest()
        ).decode()

        return {
            "Authorization": self.token,
            "Content-Type": "application/json",
            "charset": "utf8",
            "t": t,
            "sign": sign,
            "nonce": nonce
        }

    def send_command(self, device_id: str, command: str, parameter: str = "default", command_type: str = "command"):
        url = f"https://api.switch-bot.com/v1.1/devices/{device_id}/commands"
        body = {
            "command": command,
            "parameter": parameter,
            "commandType": command_type
        }
        headers = self._generate_headers()

        try:
            response = requests.post(url, headers=headers, json=body)
            print(f"[{command}] to {device_id} => {response.status_code}: {response.text}")
            return response
        except requests.exceptions.RequestException as e:
            print(f"Error: {e}")
            return None

    def get_device_list(self):
        url = "https://api.switch-bot.com/v1.1/devices"
        headers = self._generate_headers()

        try:
            response = requests.get(url, headers=headers)
            if response.ok:
                devices = response.json()
                for d in devices.get("body", {}).get("deviceList", []):
                    print(f"Device Name: {d['deviceName']}, ID: {d['deviceId']}, Type: {d['deviceType']}")
                return devices
            else:
                print(f"Failed to get devices: {response.status_code} {response.text}")
        except requests.exceptions.RequestException as e:
            print(f"Error fetching device list: {e}")
        return None




# class SwitchBotOperator():
#     def __init__(self, token: str):
#         self.token = token
#         self.headers = {
#             "Authorization": self.token,
#             "Content-Type": "application/json; charset=utf8"
#         }

#     def send_command(self, device_id, command, parameter="default", command_type="command"):
#         url = f"https://api.switch-bot.com/v1.0/devices/{device_id}/commands"
#         body = {
#             "command": command,
#             "parameter": parameter,
#             "commandType": command_type
#         }
#         response = requests.post(url, headers=self.headers, json=body)
#         print(f"[{command}] to {device_id} => {response.status_code}: {response.text}")
#         return response

#     def send_operate_request(self, devices):
#         for device in devices:
#             device_id = device["id"]

#             # ON/OFF
#             if "state" in device:
#                 command = "turnOn" if device["state"] else "turnOff"
#                 self.send_command(device_id, command)
#                 time.sleep(0.3)

#             # 明るさ
#             if "intensity" in device:
#                 intensity = str(device["intensity"])
#                 self.send_command(device_id, "setBrightness", intensity)
#                 time.sleep(0.3)

#             # 色変更
#             if "color" in device:
#                 color = device["color"]
#                 rgb_str = f"{color['r']}:{color['g']}:{color['b']}"
#                 self.send_command(device_id, "setColor", rgb_str)
#                 time.sleep(0.3)



if __name__ == '__main__':
    import os
    import dotenv
    dotenv.load_dotenv("../../../.env")
    secret = os.getenv("SB_SECRET")
    token = os.getenv('SB_TOKEN')

    sb = SwitchBotOperatorSecure(token=token, secret=secret)
