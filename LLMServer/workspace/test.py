from agents.device_operator_agent.operator_tool import operateDevice 









device = [{
    "id"  : "light1",
    "state" : False, 
    "intensity" : "80", 
    "color" : {"r":0, "g":0, "b":255}
},

{    "id"  : "light2",
    "state" : False, 
    "intensity" : "80", 
    "color" : {"r":255, "g":0, "b":0}
},
{    "id"  : "light3",
    "state" : False, 
    "intensity" : "80", 
    "color" : {"r":0, "g":255, "b":0}
},

]



async def main():
    

    res = await operateDevice.ainvoke({
        "devices": device
    }) 

    print(res)


if __name__ == "__main__":
    main()
    
    





