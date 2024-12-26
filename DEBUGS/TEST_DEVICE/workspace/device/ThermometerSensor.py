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
        self.topic= "thermo-sensor"
        self.client.subscribe(self.topic)

    def on_message(self, client, userdata, msg):
        
        msg_data = json.loads(msg.payload.decode())

        if(msg_data.get("data_type") == "request"):
            thermo_status_data = {
                "action_id": msg_data.get("action_id"),
                "data_type": "number",
                "value": self.temperature,
            }

            self.client.publish(msg_data.get("value"), json.dumps(thermo_status_data))
        
        if(msg_data.get("data_type") == "init"):
            device_status = {
                    "action_id": msg_data.get("action_id"),
                    "data_type": "number",
                    "value": self.temperature,
                }


            self.debug_log("Init message received. Sending LED status to bridge value: " + json.dumps(device_status))
            self.client.publish("bridge/" + self.topic, json.dumps(device_status))
        self.debug_log(f"Received message on {self.topic}: {msg_data}") 

    
    def send_data(self,input_data):
        inpData = input_data.split(" ")
        value = self.try_parse_float(inpData[1])
        if(value is None or len(inpData) != 2):
            self.debug_log("Invalid Input")
            return

            
        
        self.temperature = float(inpData[1])
        data = {
            "action_id": "toggle",
            "data_type": "number",
            "value": self.temperature,
        }

        sendingTopic = "bridge/" + self.topic
        self.client.publish(sendingTopic, json.dumps(data))

        self.debug_log(f"Send toggle signal to {sendingTopic}")
        

    def try_parse_float(self,value):
        try:
            return float(value)
        except ValueError:
            return None
        
            
        

