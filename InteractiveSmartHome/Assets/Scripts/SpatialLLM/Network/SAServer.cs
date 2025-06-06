using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Network;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;

/// <summary>
/// この関数は、AgentサーバーのFunctionCallingを受け取り、それに対応する処理を行う。
/// 
/// </summary>
/// <value></value>


namespace SpatialLLM.Network{


public record GetDeviceAroundFurnitureReq
{
    public string id { get; set; }
    public string order { get; set; }
    public float? range { get; set; }
}

public record FOVFurnitureRequest
{
    public bool isInFov { get; set; }
    public string order { get; set; }
    public float? range { get; set; }

    public string furnitureType { get; set; }
}

public record DirFurnitureRequest
{
    public string direction { get; set; }
    public string order { get; set; }
    public float? range { get; set; }

    public string furnitureType { get; set; }
}


public record FurnitureRequest
{
    public string furnitureType {get; set;}
    public float? range { get; set; }
}
public record FOVRequest
{
    
    public bool isInFov { get; set; }
    public string order { get; set; }
    public float? range { get; set; }
}

public record DirectionRequest
{
    public string direction { get; set; }
    public string order { get; set; }
    public float? range { get; set; }
}

public record AllRequest
{
    public string order { get; set; }
    public float? range { get; set; }
}

public record DeviceControlData
{
    public string id { get; set; }
    public bool state { get; set; }
    public int intensity { get; set; }
    public Dictionary<string, int> color { get; set; }
}

public class SAServer : HttpServer
{
    
   [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 7070;

    void Start()
    {
        Debug.Log("SAServer Initializing");
        InitServer(host, port);

        Get("/", async (context) => {

            Debug.Log("<color=red>Receive TEST</color>");
            await context.Respond(200, "Hello from Quest 3");
        });



        Get("/device/all-device", async (context) => {

            List<DeviceSpatialData> deviceSpatialDatas = SADeviceRef.Instance.GetAllDevices().Select(d => d.GetDevicePositionalData()).ToList();    
            string sendingData = JsonConvert.SerializeObject(deviceSpatialDatas);
            await context.Respond(200, sendingData);
        });


        Post("/device/fov", async (context) => {
            try {
                FOVRequest data = await context.ReadBodyAsJsonAsync<FOVRequest>();
                Debug.Log("Received Fov"); 
                Debug.Log(SpatialAwarnessProvider.Instance);
                List<DeviceSpatialData> fovResult = SpatialAwarnessProvider.Instance.GetDevicesInFov("Light", data);
               
                string sendingFovData = this.SendingDeviceData(fovResult);
                Debug.Log($"<color=yellow>[SAServer] {sendingFovData}</color>");
                await context.Respond(200, sendingFovData);
            } catch (Exception ex) {
                Debug.LogError($"Error processing /fov request: {ex.Message}");
                await context.Respond(500, "Internal Server Error");
            }
        });

        Post("/device/all", async (context) => {
            try {
                AllRequest data = await context.ReadBodyAsJsonAsync<AllRequest>();
                List<DeviceSpatialData> allResult = SpatialAwarnessProvider.Instance.GetAllDevices("Light",data  );
                string sendingAllData = this.SendingDeviceData(allResult);
                Debug.Log($"<color=yellow>[SAServer] order : {data.order}, range: {data.range}</color>");
                await context.Respond(200, sendingAllData);
            } catch (Exception ex) {
                Debug.LogError($"Error processing /all request: {ex.Message}");
                await context.Respond(500, "Internal Server Error");
            }
        });

        Post("/device/direction", async (context) => {
            try {
                DirectionRequest data = await context.ReadBodyAsJsonAsync<DirectionRequest>();
                List<DeviceSpatialData> directionResult = SpatialAwarnessProvider.Instance.GetDevicesInDirection("Light", data);
                Debug.Log($"Count: {directionResult.Count}");
                string sendingDirectionData = this.SendingDeviceData(directionResult);

                Debug.Log($"<color=yellow>[SAServer] direction: {data.direction}, order : {data.order}, range: {data.range}</color>");
                await context.Respond(200, sendingDirectionData);
            } catch (Exception ex) {
                Debug.LogError($"Error processing /direction request: {ex.Message}");
                await context.Respond(500, "Internal Server Error");
            }
        });

        Post("/device/operate", async (context) => {
            try {
                

                Debug.Log("<color=yellow>Received DeviceControlData</color>");
                List<OperatingDeviceData> operatingDeviceDatas  = await context.ReadBodyAsJsonAsync<List<OperatingDeviceData>>(); 

                foreach(OperatingDeviceData data in operatingDeviceDatas)
                {
                    Debug.Log($"<color=yellow>Operating Device: {data.id}, state: {data.state}, intensity: {data.intensity}, color: {data.color}</color>");
                    if (data == null) continue; 
                    SADevice saDevice = SADeviceRef.Instance.GetDeviceById(data.id); 
                    Debug.Log($"<color=yellow>SADevice: {saDevice}</color>");
                    if (saDevice) saDevice.OperateDevice(data);
                }


                var sending = new {
                    status = "success", 
                    message = "Operate Data Successfully"
                };
                var jsonData = JsonConvert.SerializeObject(sending);
                await context.Respond(200, jsonData);
            } catch (Exception ex) {
                Debug.LogError($"Error processing /operate request: {ex.Message}");
                await context.Respond(500, "Internal Server Error");
            }
        });

        Post("/furniture/get", async (context) => {
            try {
                FurnitureRequest data = await context.ReadBodyAsJsonAsync<FurnitureRequest>();
                Debug.Log("Received Furniture"); 
                Debug.Log(data);

                List<DeviceSpatialData> directionResult = SpatialAwarnessProvider.Instance.GetDeviceByFurnitureType(data.furnitureType, data.range ?? 0f);
                string sendingDirectionData = this.SendingDeviceData(directionResult);
                await context.Respond(200, sendingDirectionData);
            } catch (Exception ex) {
                Debug.LogError($"Error processing /furniture/get request: {ex.Message}");
                await context.Respond(500, "Internal Server Error");
            } 
        });

        // Post("/furniture/fov", async (context) => {
        //     try{

        //         FOVFurnitureRequest data = await context.ReadBodyAsJsonAsync<FOVFurnitureRequest>();
        //         Debug.Log("Received Fov"); 
        //         Debug.Log(data);
        //         // List<FurnitureData> fovResult = SpatialAwarnessProvider.Instance.GetFurnitureInFov(data);
        //         string sendingFovData = this.SendingFurnitureData(fovResult);
        //         Debug.Log($"<color=yellow>[SAServer] isInFov: {data.isInFov}, order : {data.order}, range: {data.range}</color>");

        //         await context.Respond(200, sendingFovData);
        //     }catch(Exception e)
        //     {
        //         Debug.LogError($"Error processing /furniture/fov request: {e.Message}");
        //     };
        // }); 


        // Post("/furniture/direction", async (context) => {
        //     try{

        //         DirFurnitureRequest data = await context.ReadBodyAsJsonAsync<DirFurnitureRequest>();
        //         Debug.Log("Received Direction"); 
        //         Debug.Log(data);
                
        //         List<FurnitureData> fovResult = SpatialAwarnessProvider.Instance.GetFurnitureInDirection(data);
        //         string sendingFovData = this.SendingFurnitureData(fovResult);
        //         Debug.Log($"<color=yellow>[SAServer] direction: {data.direction}, order : {data.order}, range: {data.range}</color>");

        //         await context.Respond(200, sendingFovData);
        //     }catch(Exception e)
        //     {
        //         Debug.LogError($"Error processing /furniture/fov request: {e.Message}");
        //     };
        // }); 



        Post("/furniture/find_device_with_furniture", async (context) => {
            try{

                GetDeviceAroundFurnitureReq furnitureData = await context.ReadBodyAsJsonAsync<GetDeviceAroundFurnitureReq>();
                Debug.Log($"<color=yellow>sending: {furnitureData} </color>"); 
                List<DeviceSpatialData> devicePositionalDatas = SpatialAwarnessProvider.Instance.GetDevicesAroundFurniture(furnitureData.id, furnitureData.order, furnitureData.range ?? 0f);
                Debug.Log($"<color=yellow>[SAServer] {devicePositionalDatas}</color>");

                string sendingDeviceData = this.SendingDeviceData(devicePositionalDatas);
                Debug.Log($"<color=yellow>[SAServer] Find Device with Furniture: {sendingDeviceData}</color>");

                await context.Respond(200, sendingDeviceData);
            }catch(Exception e)
            {
                Debug.LogError($"Error processing /furniture/fov request: {e.Message}");
            };
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

    private string  SendingDeviceData(List<DeviceSpatialDataForFurniture> devicePositionalDatas)
    {
        var sending = new
        {
            status = "success",
            devices = devicePositionalDatas
        };
        
        var jsonData = JsonConvert.SerializeObject(sending);
        return jsonData;
    }

    private string SendingFurnitureData(List<FurnitureData> furnitureDatas)
    {


        List<string> furnitureDataJson = furnitureDatas.Select(furnitureData => furnitureData.ToJson()).ToList();

        var sending = new
        {
            status = "success",
            furnitures = furnitureDataJson
        };
        var jsonData = JsonConvert.SerializeObject(sending);
        return jsonData;
        }
}
}