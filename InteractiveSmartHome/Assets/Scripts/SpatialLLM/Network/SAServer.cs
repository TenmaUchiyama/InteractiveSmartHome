using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ActionDataTypes;
using Meta.WitAi.Json;
using Newtonsoft.Json.Linq;
using SpatialLLM.Core;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using static SpatialLLM.Network.NetworkDataType;
using static SpatialLLM.Network.NetworkDataType.LLMServerDataUtil;
using static SpatialLLM.Type.DirectionUtil;

public class SAServer : HttpServer
{
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 7070;





    Dictionary<string, Func<string, string>> functionMap = new Dictionary<string, Func<string, string>>();
    




   private void Start()
    {

      

        InitServer(host, port);


        Get("/", async (context) => {

            Debug.Log("<color=red>Receive TEST</color>");
            await context.Respond(200, "Hello from Quest 3");
        });


        Post("/", async (context) => {
          
        JObject data = await context.ReadBodyAsJsonAsync<JObject>(); 

        string function = data["function"]?.ToString();

        Debug.Log($"<color=yellow>[SAServer] Function: {function}</color>");
        switch(function)
        {
            case "all-device":
                
                string allOrder = data["order"]?.ToString();
                string allRange = data["range"]?.ToString();
                Debug.Log($"<color=yellow>[SAServer] Order: {allOrder}</color>");
                Debug.Log($"<color=yellow>[SAServer] Range: {allRange}</color>");
                List<DeviceSpatialData> allResult = SpatialAwarnessProvider.Instance.AllDevice(allOrder,allRange);
                string sendingData = this.SendingDeviceData(allResult);
                await context.Respond(200, sendingData);
            break; 
            case "direction":
               
                string dir = data["dir"]?.ToString();
                string dirOrder = data["order"]?.ToString();
                string dirRange = data["range"]?.ToString();
                
                Debug.Log($"<color=yellow>[SAServer] Direction: {dir}</color>");   
                Debug.Log($"<color=yellow>[SAServer] Order: {dirOrder}</color>"); 
                Debug.Log($"<color=yellow>[SAServer] Range: {dirRange}</color>");
                 List<DeviceSpatialData> dirResult = SpatialAwarnessProvider.Instance.DirectionFunction(dir, dirOrder,dirRange); 



                string sendingDirData = this.SendingDeviceData(dirResult);

                Debug.Log($"<color=green>{sendingDirData}</color>");
                await context.Respond(200, sendingDirData);
            break; 
            case "sight":

                 string isInFov = data["isInFov"]?.ToString();
            
                string sightOrder = data["order"]?.ToString();
                string rangeOrder = data["range"]?.ToString();
                Debug.Log($"<color=yellow>[SAServer] Order: {sightOrder}</color>");
                Debug.Log($"<color=yellow>[SAServer] Range: {rangeOrder}</color>");
                 List<DeviceSpatialData> sightResult = SpatialAwarnessProvider.Instance.SightFunction(isInFov , sightOrder, rangeOrder);
                 Debug.Log($"<color=blue>{sightResult.Count}</color>");
                string sendingSightData = this.SendingDeviceData(sightResult);
                Debug.Log($"<color=green>{sendingSightData}</color>");
                await context.Respond(200, sendingSightData);
            break; 
            case "reset":
                TestDeviceManager.Instance.ResetAllColor();
            break; 

            default:
            break; 
        }


  
        });



    
    }



private string  SendingDeviceData(List<DeviceSpatialData> devicePositionalDatas)
{
    var sending = new
    {
        status = "success",
        devices = devicePositionalDatas
    };
    
    var jsonData = JsonConvert.SerializeObject(sending);
    return jsonData;
}


}



