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

        [SerializeField] private float intensityScale = 10.0f;
    
         
        Light light;

        DrawOnHover drawOnHover;

        public bool isDebug = false;


        
        private void Awake() {

            this.saDeviceType = SADeviceType.Light;
            string id = Guid.NewGuid().ToString();

            this.deviceData = new DBDeviceData(
                id,
                this.gameObject.name,
                SADeviceType.Light.ToString(),
                "light",
                "device/" + id,
                this.transform.position
            );

            Debug.Log($"<color=green>Device data initialized for {this.gameObject.name}. ID: {this.deviceData.device_id}</color>");

            this.spatialData = new DeviceSpatialData(id , gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));




        }


        private void Start() {
        drawOnHover = GetComponent<DrawOnHover>();
       
            
        light = GetComponentInChildren<Light>();

       UpdateCurrentData();


        if(isDebug)return;
        MRMqttController.Instance.OnConnectionCompleted += () => {
        MRMqttController.Instance.SubscribeDeviceTopic(this.deviceData.device_name, this.deviceData.mqtt_topic,  OnReceiveMsgFromServer);
        };
        }


       public override void Init() 
    {

        OperatingDeviceData initData = new OperatingDeviceData {
            state = false,
        };
        OperateDevice(initData);
        UpdateCurrentData();

        this.drawOnHover.ClearDrawing();
    }

        private void UpdateCurrentData()
        {
        if(light)this.currentOperatingData.state = light.enabled;   
        if(light)this.currentOperatingData.intensity = GetLightIntensity();
        }
         void OnReceiveMsgFromServer(string payload)
        {
              Debug.Log($"<color=Blue>[{this.gameObject.name}] Received {payload}</color>");
                OperatingDeviceData operatingDeviceData = JsonConvert.DeserializeObject<OperatingDeviceData>(payload);
                OperateDevice(operatingDeviceData);

              
        }


        public int GetLightIntensity()
        {
            return (int)(light.intensity * intensityScale);
        }
        public override void OperateDevice(OperatingDeviceData operatingDeviceData )
        {


            Debug.Log($"<color=blue>[{this.gameObject.name}] State {operatingDeviceData.state}, Intensity: {operatingDeviceData.intensity}</color>");
             light.enabled = operatingDeviceData.state;
            Debug.Log($"<color=red>[{light.enabled}</color>");
             isDeviceOn = operatingDeviceData.state;
             currentOperatingData = operatingDeviceData;
                if(operatingDeviceData.intensity != null) light.intensity = Mathf.Clamp((float)operatingDeviceData.intensity / 3.0f, 0.0f, 3.0f);

                if(operatingDeviceData.color != null){
                    ColorData colorData = operatingDeviceData.color;
                    light.color = new Color(
                        Mathf.Clamp01(colorData.r / 255.0f),
                        Mathf.Clamp01(colorData.g / 255.0f),
                        Mathf.Clamp01(colorData.b / 255.0f)
                    );
                }else{
                    light.color = Color.white;

                }


                this.drawOnHover.VisualizeTargetDevice(Color.blue);
        }

        public override void TurnOnWithColor(Color color)
        {
            light.enabled = true; 
            light.intensity = intensityScale; 
            light.color = color;
            isDeviceOn = true;
        }

        public override void TurnOff()
        {
            light.enabled = false;
            light.intensity = 0;
            light.color = Color.white;
            isDeviceOn = false;
            this.drawOnHover.ClearDrawing();
        }
    }
}