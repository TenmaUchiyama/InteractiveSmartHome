from dotenv import load_dotenv
# from Mqtt import MQTTPublisher 
import httpx # type: ignore
import os
import json
from utils.communication.SwitchBotOperator import SwitchBotOperator
from utils.communication.TestOperator import TestOperator
import asyncio
from typing import List, Dict, Tuple
import json



class DeviceOperator():
    def __init__(self):
        
        load_dotenv("../.env")
        self.switchbot = SwitchBotOperator(
            token=os.getenv("SB_TOKEN"),
            secret=os.getenv("SB_SECRET")
        )
        self.test_operator = TestOperator()
        # MQTT Publisher の初期化（使う場合）
        # self.mqtt_publisher = MQTTPublisher(...)





    def send_operator(self, llm_devices: List[dict]) -> Dict[str, List[dict]]:
        try:
            all_devs = self._fetch_all_devices()
        except Exception as e:
            print("[WARN] fetch_all_devices failed, using empty list instead. Error:", e)
            all_devs = []


        test_data = []
        for dev in llm_devices:
            if dev["id"] not in [d["device_id"] for d in all_devs]:
                test_data.append(dev)

        sb_map, mq_map = self._build_maps(all_devs)
        mq_reqs, sb_reqs = self._prepare_requests(llm_devices, sb_map, mq_map)

        if mq_reqs:
            self.send_mqtt_request(mq_reqs)
        if sb_reqs:
            asyncio.run(self.switchbot.send_switchbot_request(sb_reqs))
        if test_data:
            self.test_operator.send_operate_request(test_data)
            

        return {"mqtt": mq_reqs, "switchbot": sb_reqs, "test": test_data}

    def _fetch_all_devices(self) -> List[dict]:
        resp = httpx.get("http://localhost:4049/device/get-all")
        resp.raise_for_status()
        return resp.json()

    def _build_maps(self, devices: List[dict]) -> Tuple[Dict[str, dict], Dict[str, dict]]:
   
        sb_map = {
            d["device_id"]: d
            for d in devices
            if d.get("connector_type") == "switchbot"
        }
        mq_map = {
            d["device_id"]: d
            for d in devices
            if d.get("connector_type") == "mqtt"
        }

 
        return sb_map, mq_map

    def _prepare_requests(
        self,
        llm_devices: List[dict],
        sb_map: Dict[str, dict],
        mq_map: Dict[str, dict]
    ) -> Tuple[List[dict], List[dict]]:
        mqtt_reqs = []
        switchbot_reqs = []
        print()
        print("SB_MAP", sb_map)
        print()
        print()
        print("LLM_DEVICES", llm_devices)
        print()
        for data in llm_devices:
            dev_id = data.get("id")
            
            if not dev_id:
                print("[WARN] Device entry missing 'id'")
                continue

            if dev_id in sb_map:
                db = sb_map[dev_id]
             
                switchbot_reqs.append({
                    "connector_topic": db["connector_topic"],
                    "state": data.get("state"),
                    "intensity": data.get("intensity"),
                    "color": data.get("color"),
                })
            elif dev_id in mq_map:
                db = mq_map[dev_id]
                mqtt_reqs.append({
                    "topic": db.get("connector_topic") or db.get("topic"),
                    "state": data.get("state"),
                    "intensity": data.get("intensity"),
                    "color": data.get("color"),
                })
            else:
                print(f"[WARN] Unknown device_id: {dev_id}")

        return mqtt_reqs, switchbot_reqs

    def send_mqtt_request(self, devices: List[dict]):
        for dev in devices:
            topic = dev["topic"]
            payload = json.dumps(dev)
            self.mqtt_publisher.send_data(topic, payload)



if __name__ == "__main__":




    db = DeviceOperator() 

    device = [{
        "id"  : "test_id",
        "state" : False, 
        "intensity" : "80", 
        "color" : {"r":20, "g":20, "b":255}
    }
    ]


    db.send_operator(device)
