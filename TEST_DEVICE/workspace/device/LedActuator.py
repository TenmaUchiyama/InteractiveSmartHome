# led_actuator.py
import json
import paho.mqtt.client as mqtt
from device.DeviceBase import DeviceBase

class LedActuator(DeviceBase):
    def __init__(self):
        super().__init__()
        self.status = False
    
    
    def on_connect(self, client, userdata, flags, rc):
        self.debug_log("Connected to MQTT broker")
        self.topic= "led-actuator"
        self.client.subscribe(self.topic)

    def on_message(self, client, userdata, msg):
        msg_data = json.loads(msg.payload.decode())
        super().on_message(msg_data,self.status)
        self.debug_log(f"Received message on {msg.topic}: {msg_data}") 

        topic = "mrflow/" + msg_data.get("action_id")
        

        dtype = msg_data.get("data_type")
        if(dtype == "trigger"):
            self.status = not self.status
            led_status_data = {
                "action_id": msg_data.get("action_id"),
                "data_type": "boolean",
                "value": self.status,
            }
            self.client.publish(topic, json.dumps(led_status_data))
            self.debug_log(f"LED status changed to {topic} value {self.status}")


        if(dtype == "boolean"):
            self.status = msg_data.get("value")
            led_status_data = {
                "action_id": msg_data.get("action_id"),
                "data_type": "boolean",
                "value": self.status,
            }
            self.client.publish(topic, json.dumps(msg_data))
            self.debug_log(f"LED status changed to {self.status}")

        if(msg_data.get("data_type") == "init"):
            device_status = {
                    "action_id": msg_data.get("action_id"),
                    "data_type": "boolean",
                    "value": self.status,
                }


            self.debug_log("Init message received. Sending LED status to bridge value: " + json.dumps(device_status))
            self.client.publish("bridge/" + self.topic, json.dumps(device_status))


    

        

