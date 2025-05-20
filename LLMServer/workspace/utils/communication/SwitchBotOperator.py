import asyncio
import time
import requests
import  time, uuid, hmac, hashlib, base64
import httpx  # type: ignore
from dotenv import load_dotenv
import os

class SwitchBotOperator:
    def __init__(self, token: str, secret: str):
        self.token = token if token != "" else os.getenv("SB_TOKEN")
        self.secret = secret  if secret != "" else os.getenv("SB_SECRET")
        

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
    

    async def send_command(self, client: httpx.AsyncClient, device_id: str, command: str, parameter="default"):
        url = f"https://api.switch-bot.com/v1.1/devices/{device_id}/commands"
        body = {"command": command, "parameter": parameter, "commandType": "command"}
        headers = self._generate_headers()
        resp = await client.post(url, headers=headers, json=body)
        print(f"[{command}] {device_id} => {resp.status_code}")
        return resp



    """
    実際に外から使われるメソッド。

    """
    async def send_switchbot_request(self, devices: list[dict]):
        async with httpx.AsyncClient() as client:
            tasks = []
            for d in devices:
                dev_id = d["connector_topic"]
                # 電源
                if "state" in d:
                    cmd = "turnOn" if d["state"] else "turnOff"
                    tasks.append(self.send_command(client, dev_id, cmd))
                    if not d["state"]:
                        continue
                # 明るさ
                if d.get("state") and "intensity" in d:
                    tasks.append(self.send_command(client, dev_id, "setBrightness", str(d["intensity"])))
                # 色
                if d.get("state") and "color" in d:
                    c = d["color"]
                    rgb = f"{c['r']}:{c['g']}:{c['b']}"
                    tasks.append(self.send_command(client, dev_id, "setColor", rgb))
            # すべてを並列実行
            results = await asyncio.gather(*tasks, return_exceptions=True)
            for r in results:
                if isinstance(r, Exception):
                    print(f"[ERROR] {r}")




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



if __name__ == '__main__':
    import os
    import dotenv
    dotenv.load_dotenv("../../../.env")
    secret = os.getenv("SB_SECRET")
    token = os.getenv('SB_TOKEN')
    

    sb = SwitchBotOperatorSecure(token=token, secret=secret)
