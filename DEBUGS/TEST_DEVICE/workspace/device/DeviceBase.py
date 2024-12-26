# led_actuator.py
import json
import paho.mqtt.client as mqtt

import os

# 環境変数のロード


broker_host = "mqtt-broker" if os.getenv("NODE_ENV") == "docker" else "localhost"


class DeviceBase:
    def __init__(self):
        
        
        self.client = mqtt.Client()

        # MQTTコールバック設定
        self.client.on_connect = self.on_connect
        self.client.on_message = self.on_message

        # MQTT接続
        self.client.connect(broker_host, 1883, 60)
        self.client.loop_start()
    
    def debug_log(self, message):
        print(f"[{self.__class__.__name__}] {message}")
    

    def on_message(self,msg,value):
        if(msg.get("data_type") == "request"):
            led_status_data = {
                "action_id": msg.get("action_id"),
                "data_type": "boolean",
                "value": value,
            }
            self.client.publish(msg.get("value"), json.dumps(led_status_data))
            self.debug_log(f"Send LED status to {msg.get('value')}")    
        
       
    


    def send_data(self,input_data):
        self.debug_log("No item to send")


   


