using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using MRFlow.Network;
using SpatialLLM.Device;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;

public class SASwitchbot : SADevice
{

    [SerializeField] SpatialAnchorLoaderBuildingBlock anchorLoader;
    [SerializeField] string fixedId = "";
    [SerializeField] string name = ""; 
    [SerializeField] string topic = ""; 


    [SerializeField] ActionServerConnector actionServerConnector;



    // void OnEnable()
    // {
    //     this.spatialData = new DeviceSpatialData("aaaaa", "aaaaaa", transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));
    // }

    public override void Init()
    {
       
    }


    public void InitAnchorGen() 
    {

        Debug.Log("ENABLED");
        DrawOnHover drawOnHover = GetComponent<DrawOnHover>(); 

       if(drawOnHover) drawOnHover.VisualizeTargetDevice(Color.blue); 

    }


    public async void CreateDeviceData(string anchor_id)
    {   

        this.spatialData = new DeviceSpatialData(fixedId =="" ? Guid.NewGuid().ToString() : fixedId , "test", transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));

        
        DBDeviceData dbData = new DBDeviceData(
                device_id : fixedId =="" ? Guid.NewGuid().ToString() : fixedId ,
                device_type: "light",
                device_name: name,
                anchor_id : anchor_id, 
                connector_type: "switchbot",
                connector_topic :topic ,
                description : ""
            ); 
         this.SetDBDeviceData(
            dbData
        );




        // await actionServerConnector.AddDevices(new List<DBDeviceData>{dbData});
    }


    public void GenerateDBDeviceData(string anchor_id)
    {
        this.SetDBDeviceData(
            new DBDeviceData(
                device_id : Guid.NewGuid().ToString(),
                device_type: "light",
                device_name: "test switchbot device",
                anchor_id : anchor_id, 
                connector_type: "switchbot",
                connector_topic : "",
                description : ""
            )
        );
    }

    

    public void LoadSwitchBot(DBDeviceData dBDeviceData)
    {
        base.deviceData = dBDeviceData;
    }

    public override void OperateDevice(NetworkDataType.OperatingDeviceData operatingDeviceData)
    {
        throw new System.NotImplementedException();
    }

    public override void TurnOff()
    {
        throw new System.NotImplementedException();
    }

    public override void TurnOnWithColor(Color color)
    {
        throw new System.NotImplementedException();
    }
}
