using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ActionDataTypes;
using Meta.WitAi.Json;
using SpatialLLM.Core;
using SpatialLLM.Network;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using static DirectionUtil;
using static SpatialLLM.Network.NetworkDataType;
using static SpatialLLM.Network.NetworkDataType.LLMServerDataUtil;

public class SAServer : HttpServer
{
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 7070;
    void Start()
    {
        InitServer(host, port);





        Post("/", async (context) => {
            // リクエストボディを読み込む
        FunctionMsgType data = await context.ReadBodyAsJsonAsync<FunctionMsgType>(); 

        if(data.function == "direction")
        {
            string direction = data.args[0];
            Direction dir = DirectionUtil.GetDirection(direction);
            List<Device> devices = SpatialAwarnessProvider.Instance.GetDeviceInDirection(dir); 
            List<DevicePositionalData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();
            devicePositionData.Sort((a, b) => a.distance_from_user.CompareTo(b.distance_from_user));
            
            var sending = new {
                status = "success",
                data = devicePositionData
            };

            var jsonData = JsonConvert.SerializeObject(sending);


            // JSON形式でレスポンスを返す   
            await context.Respond(200, jsonData);
        }


        if(data.function == "sight")
        {
            List<Device> devices = SpatialAwarnessProvider.Instance.GetDevicesInSight(data.args[0].ToLower() == "true"); 
            List<DevicePositionalData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();
            devicePositionData.Sort((a, b) => a.distance_from_user.CompareTo(b.distance_from_user));
            
            var sending = new {
                status = "success",
                data = devicePositionData
            };

            var jsonData = JsonConvert.SerializeObject(sending);
        }
        
        });

    }



   
}



