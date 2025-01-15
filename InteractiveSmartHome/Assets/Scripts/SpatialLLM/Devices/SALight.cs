using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.Network;
using Newtonsoft.Json;
using SpatialLLM.Type;
using Unity.VisualScripting;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;


namespace SpatialLLM.Device{
public class SALight : SADevice
{
    
        private GameObject boundingBoxVisualizer;
         
        Light light;

        public bool isDebug = false;

        private void Awake() {

            this.saDeviceType = SADeviceType.Light;
            string id = Guid.NewGuid().ToString();

            this.deviceData = new DBDeviceData(
                id,
                this.gameObject.name,
                "light",
                "This is a light device. You need this when you want to turn on or off the light. Intensity 0 is the darkest, 100 is the brightest. You can specify the color of the light by specifying the RGB value. For example, if you want to set the light to red, you can specify the RGB value as 255, 0, 0.",
                "device/" + id,
                this.transform.position
            );

            this.spatialData = new DeviceSpatialData(id , gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));


      




        }


        private void Start() {
        boundingBoxVisualizer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boundingBoxVisualizer.transform.SetParent(this.transform);
        boundingBoxVisualizer.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.3f); // 半透明
        boundingBoxVisualizer.SetActive(false);
            
        light = GetComponentInChildren<Light>();

        if(light)this.currentOperatingData.state = light.enabled;
        if(light)this.currentOperatingData.intensity = GetLightIntensity();


        if(isDebug)return;
        MRMqttController.Instance.OnConnectionCompleted += () => {
        MRMqttController.Instance.SubscribeDeviceTopic(this.deviceData.device_name, this.deviceData.mqtt_topic,  OnReceiveMsgFromServer);
        };
        }

 
         void OnReceiveMsgFromServer(string payload)
        {
              Debug.Log($"<color=Blue>[{this.gameObject.name}] Received {payload}</color>");
                OperatingDeviceData operatingDeviceData = JsonConvert.DeserializeObject<OperatingDeviceData>(payload);
                OperateDevice(operatingDeviceData);

              
        }


        public int GetLightIntensity()
        {
            return (int)(light.intensity * 10.0f);
        }
        public override void OperateDevice(OperatingDeviceData operatingDeviceData )
        {


            Debug.Log($"<color=yellow>[{this.gameObject.name}] State {operatingDeviceData.state}, Intensity: {operatingDeviceData.intensity}</color>");
             light.enabled = operatingDeviceData.state;
             currentOperatingData = operatingDeviceData;
                if(operatingDeviceData.intensity != null) light.intensity = Mathf.Clamp((float)operatingDeviceData.intensity / 10.0f, 0.0f, 10.0f);
                Debug.Log($"<color=yellow>intensity: {light.intensity} </color>");

                if(operatingDeviceData.color != null){
                    ColorData colorData = operatingDeviceData.color;
                    light.color = new Color(
                        Mathf.Clamp01(colorData.r / 255.0f),
                        Mathf.Clamp01(colorData.g / 255.0f),
                        Mathf.Clamp01(colorData.b / 255.0f)
                    );
                }
        }





      private void VisualizeBoundingBox()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // バウンディングボックスのサイズと位置を設定
            boundingBoxVisualizer.transform.position = renderer.bounds.center;
            boundingBoxVisualizer.transform.localScale = renderer.bounds.size;
            boundingBoxVisualizer.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Renderer not found. Cannot visualize bounding box.");
        }
    }


    }
}