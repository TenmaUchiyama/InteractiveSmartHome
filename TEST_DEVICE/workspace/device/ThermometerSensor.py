# led_actuator.py
import json
import paho.mqtt.client as mqtt
from device.DeviceBase import DeviceBase
import time



class ThermometerSensor(DeviceBase):
    def __init__(self):
        super().__init__()
        self.temperature = 10
    
    
    def on_connect(self, client, userdata, flags, rc):
        self.debug_log("Connected to MQTT broker")
        self.client.subscribe("toggle-button-sensor")

    def on_message(self, client, userdata, msg):
        
        msg_data = json.loads(msg.payload.decode())

        if(msg_data.get("data_type") == "request"):
            thermo_status_data = {
                "action_id": msg_data.get("action_id"),
                "data_type": "number",
                "value": self.value,
            }

            self.client.publish(msg.get("value"), json.dumps(thermo_status_data))
        self.debug_log(f"Received message on {msg.topic}: {msg_data}") 

    
    def send_data(self,input_data):
        inpData = input_data.split(" ")
        if len(inpData) != 2 and self.try_parse_int(inpData[1]): 
            
            data = {
                "action_id": "toggle",
                "data_type": "number",
                "value": self.temperature,
            }

            sendingTopic = "bridge/thermo-sensor"
            self.client.publish(sendingTopic, json.dumps(data))
            self.debug_log(f"Send toggle signal to {sendingTopic}")
        else:
            self.debug_log("Invalid Input")

    def try_parse_int(value):
        try:
            return int(value)
        except ValueError:
            return None
        
            
        

