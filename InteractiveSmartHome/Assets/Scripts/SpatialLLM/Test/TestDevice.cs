using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Meta.Voice.NLayer;
using MRFlow.Network;
using Newtonsoft.Json;
using Oculus.Platform.Models;
using SpatialLLM.Type;
using UnityEditor;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;




namespace SpatialLLM.Core{




    public class TestDevice : MonoBehaviour
    {


    
        public DBDeviceData dbDeviceData;
        
        public DeviceSpatialData debugDeviceData;
        public bool debug = false;
        private bool isVisible = false;

          JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        

        private Color targetColor = Color.red;
        private Color originalColor = new Color(92/255,212/255,255/255,255/255);


        private Renderer renderer; 


        public bool IsVisible
        {
            get { return isVisible; }
        }

        private void Awake() {

            string id = Guid.NewGuid().ToString();
            this.dbDeviceData = new DBDeviceData(
                id,
                this.gameObject.name,
                "test",
                "device/" + this.gameObject.name,
                "This is a test device. You can use this device to test the system.",
                this.transform.position

            );


            debugDeviceData = new DeviceSpatialData(id , gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));
        
        }






        private void Start() {
            
            renderer = this.GetComponent<Renderer>();

            MRMqttController.Instance.OnConnectionCompleted += () => {
            MRMqttController.Instance.SubscribeDeviceTopic(this.dbDeviceData.device_name, this.dbDeviceData.product_topic,  OnReceiveMsgFromServer);
            };
        }


        public void OnReceiveMsgFromServer(string payload)
        {
                Debug.Log($"<color=yellow>[{this.gameObject.name}] Received {payload}</color>");
                this.ChangeColor();
        }


        public DeviceSpatialData GetDevicePositionalData()
        {
            debugDeviceData.position = new Position(transform.position);
            debugDeviceData.distance_from_user = Vector3.Distance(transform.position, Camera.main.transform.position);
            return debugDeviceData;
        }


        public DBDeviceData GetDBDeviceData() 
        {
            return this.dbDeviceData;
        }

    void OnBecomeVisible()
    {
        isVisible = true;
    
       if(debug)
       {
           Debug.Log("Visibleeeeeeeeeeeeeee");
       }
    }


    void OnBecomeInvisible()
    {
        isVisible = false;
    }



    public void ChangeColor() 
    {
        this.renderer.material.color = Color.red;
    }

    public void ResetColor()
    {
        this.renderer.material.color = this.originalColor;
    }
}
}