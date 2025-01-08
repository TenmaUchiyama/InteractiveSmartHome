using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using MRFlow.Network;
using Oculus.Interaction;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;



namespace SpatialLLM.Device

{


   
    public abstract class SADevice : MonoBehaviour
    {
         protected DBDeviceData deviceData; 
         protected DeviceSpatialData spatialData;

         protected SADeviceType saDeviceType;

         protected OperatingDeviceData currentOperatingData = new OperatingDeviceData(); 




    

        public int HandleHover { get; private set; }

        public abstract void OperateDevice(OperatingDeviceData operatingDeviceData);


        public DeviceSpatialData GetDevicePositionalData()
        {
            this.spatialData.position = new Position(new Vector3(this.transform.position.x, this.transform.position.z, this.transform.position.y));
            this.spatialData.distance_from_user = Vector3.Distance(transform.position, Camera.main.transform.position);
            return this.spatialData;
        }


        public DBDeviceData GetDBDeviceData() 
        {
            return this.deviceData;
        }

        

        public bool CompareDeviceType(string device_type)
        {
            return device_type.Equals(saDeviceType.ToString()) || device_type == "";
        }


        public SADeviceType GetSADeviceType()
        {
            return this.saDeviceType;
        }


      
        public OperatingDeviceData GetCurrentOperateData()
        {
            return this.currentOperatingData;
        }

    }
}