using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.Network;
using Newtonsoft.Json;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;


namespace SpatialLLM.Device{
public class SALight : SADevice
{
    
    
         
        Light light;

        private void Awake() {

            this.saDeviceType = SADeviceType.Light;
            string id = Guid.NewGuid().ToString();

            this.deviceData = new DBDeviceData(
                id,
                this.gameObject.name,
                "light",
                "device/" + id,
                this.transform.position
            );

            this.spatialData = new DeviceSpatialData(id , gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));
            Debug.Log($"[SALight]{this.spatialData}");

        }


        private void Start() {
             light = GetComponentInChildren<Light>();

             MRMqttController.Instance.OnConnectionCompleted += () => {
        MRMqttController.Instance.SubscribeDeviceTopic(this.deviceData.device_name, this.deviceData.mqtt_topic,  OnReceiveMsgFromServer);
        };
        }

 
         void OnReceiveMsgFromServer(string payload)
        {
              Debug.Log($"<color=yellow>[{this.gameObject.name}] Received {payload}</color>");
                OperatingDeviceData operatingDeviceData = JsonConvert.DeserializeObject<OperatingDeviceData>(payload);


                light.enabled = operatingDeviceData.state;
                if(operatingDeviceData.intensity != null) light.intensity = Mathf.Clamp((float)operatingDeviceData.intensity / 10.0f, 0.0f, 10.0f);
                

                if(operatingDeviceData.color != null){
                    ColorData colorData = operatingDeviceData.color;
                    light.color = new Color(
                        Mathf.Clamp01(colorData.r / 255.0f),
                        Mathf.Clamp01(colorData.g / 255.0f),
                        Mathf.Clamp01(colorData.b / 255.0f)
                    );
                }
        }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}