using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using MRFlow.Network;
using Oculus.Interaction;
using SpatialLLM.Network;
using SpatialLLM.Type;
using TMPro;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;



namespace SpatialLLM.Device

{


   
    public abstract class SADevice : MonoBehaviour
    {

         [SerializeField] ShowLabel showLabel;
         protected DBDeviceData deviceData; 
         protected DeviceSpatialData spatialData;

         protected SADeviceType saDeviceType;

         protected OperatingDeviceData currentOperatingData = new OperatingDeviceData(); 

         protected bool isDeviceSelected = false;

        protected bool isDeviceOn = false; 

        public bool IsDeviceOn => isDeviceOn;
 
    

        public int HandleHover { get; private set; }




        public abstract void OperateDevice(OperatingDeviceData operatingDeviceData);

        public abstract void TurnOnWithColor(Color color);

        public abstract void TurnOff();

        public abstract void Init();


        

        public DeviceSpatialData GetDevicePositionalData()
        {
            this.spatialData.position = new Position(new Vector3(this.transform.position.x, this.transform.position.z, this.transform.position.y));
            this.spatialData.distance_from_user = Vector3.Distance(transform.position, Camera.main.transform.position);
            return this.spatialData;
        }



        public string GetDeviceID() 
        {
            return this.deviceData.device_id;
        }


        public DBDeviceData GetDBDeviceData() 
        {
            return this.deviceData;
        }


        public bool IsDeviceSelected()
        {
            return this.isDeviceSelected;
        }

        public void SetIsSelected(bool isSelected)
        {
            this.isDeviceSelected = isSelected;
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