from agents.device_operator_agent.operator_tool import operateDevice 
import httpx
import asyncio









device = [{
    "id"  : "7a5851cf-18eb-4279-a218-c11b97306ba8",
    "state" : False, 
    "intensity" : "80", 
    "color" : {"r":0, "g":0, "b":255}
},

{    "id"  : "dd206fc7-1b6e-4bfd-91e7-211932f7ba2e",
    "state" : False, 
    "intensity" : "80", 
    "color" : {"r":255, "g":0, "b":0}
},
{    "id"  : "dd8056fc-71a0-4445-96e9-24e00effc226",
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

    # res = httpx.get("http://localhost:4049/device/get-all")
    # print(res.json())


if __name__ == "__main__":
    asyncio.run(main())
    
    





