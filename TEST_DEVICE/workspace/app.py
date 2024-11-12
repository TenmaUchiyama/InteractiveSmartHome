import paho.mqtt.client as mqtt
from device.LedActuator import LedActuator
from device.ToggleButton import ToggleButton


if __name__ == "__main__": 
    led = LedActuator()
    toggle = ToggleButton()


    while True:
       
        inp = input()
        device= inp.split(" ")[0]


        if device == "toggle":
            toggle.send_data(inp)


        if inp == "exit":
            break





