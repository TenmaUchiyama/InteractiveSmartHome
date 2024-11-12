# led_actuator.py
import json
import paho.mqtt.client as mqtt
from device.DeviceBase import DeviceBase

class ToggleButton(DeviceBase):
    def __init__(self):
        super().__init__()
        self.status = False
    
    
    def on_connect(self, client, userdata, flags, rc):
        self.debug_log("Connected to MQTT broker")
        self.client.subscribe("toggle-button-sensor")

    def on_message(self, client, userdata, msg):
        
        msg_data = json.loads(msg.payload.decode())

        super().on_message(msg_data,self.status)
        self.debug_log(f"Received message on {msg.topic}: {msg_data}") 

        
        # dtype = msg_data.get("data_type")
        # if(dtype == "trigger"):
        #     self.status = not self.status
        #     self.debug_log(f"LED status changed to {self.status}")
        #     self.client.publish("bridge/toggle-button-actuator", json.dumps(msg_data))
        
        # if(dtype == "boolean"):
        #     self.status = msg_data.get("value")
        #     self.debug_log(f"LED status changed to {self.status}")


    
    def send_data(self,input_data):
        inpData = input_data.split(" ")
        if len(inpData) != 2 or inpData[1].lower() != "on" or inpData[1].lower() != "off": 
    
            self.status = True if inpData[1] == "on" else False
            data = {
                "action_id": "toggle",
                "data_type": "boolean",
                "value": self.status,
            }
            self.client.publish("bridge/toggle-button-sensor", json.dumps(data))
            self.debug_log("Send toggle signal to bridge/toggle-button-actuator")
        else:
            self.debug_log("Invalid Input")
        
            
        

