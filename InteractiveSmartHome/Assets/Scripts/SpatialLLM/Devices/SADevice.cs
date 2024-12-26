using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using MRFlow.Network;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;



namespace SpatialLLM.Device

{
    public class SADevice : MonoBehaviour
    {
         DBDeviceData deviceData; 
         DeviceSpatialData spatialData;

         [SerializeField] Light light;
    
    


        private void Awake() {
            string id = Guid.NewGuid().ToString();

            this.deviceData = new DBDeviceData(
                id,
                this.gameObject.name,
                "light",
                "device/" + id,
                this.transform.position
            );

            this.spatialData = new DeviceSpatialData(id , gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));

        }




        private void Start() {
        light = GetComponentInChildren<Light>();
   
        MRMqttController.Instance.OnConnectionCompleted += () => {
        MRMqttController.Instance.SubscribeDeviceTopic(this.deviceData.device_name, this.deviceData.mqtt_topic,  OnReceiveMsgFromServer);
        };
        }

        public void OnReceiveMsgFromServer(string payload)
        {
                Debug.Log($"<color=yellow>[{this.gameObject.name}] Received {payload}</color>");
                OperatingDeviceData operatingDeviceData = JsonConvert.DeserializeObject<OperatingDeviceData>(payload);


                light.enabled = operatingDeviceData.state;
                if(operatingDeviceData.intensity != null) light.intensity = (float)(operatingDeviceData.intensity / 10);
                

                if(operatingDeviceData.color != null){
                    ColorData colorData = operatingDeviceData.color;
                    light.color = new Color(colorData.r, colorData.g, colorData.b);
                }
        }


        public DeviceSpatialData GetDevicePositionalData()
        {
            this.spatialData.position = new Position(transform.position);
            this.spatialData.distance_from_user = Vector3.Distance(transform.position, Camera.main.transform.position);
            return this.spatialData;
        }


        public DBDeviceData GetDBDeviceData() 
        {
            return this.deviceData;
        }

        


    }
}