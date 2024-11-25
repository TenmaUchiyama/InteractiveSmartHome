import paho.mqtt.client as mqtt
from device.LedActuator import LedActuator
from device.ToggleButton import ToggleButton
from device.ThermometerSensor import ThermometerSensor


if __name__ == "__main__": 

    devices = {
        # "led" : LedActuator() ,
        "toggle" : ToggleButton(), 
        "thermo" : ThermometerSensor()
    }

    


    while True:
       
        inp = input()
        device= inp.split(" ")[0]

        for name in devices.keys():
            if device == name: 
                devices[name].send_data(inp)

        if inp == "exit":
            break





