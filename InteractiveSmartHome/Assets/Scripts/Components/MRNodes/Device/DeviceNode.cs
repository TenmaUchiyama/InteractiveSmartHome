using System;
using ActionDataTypes;
using ActionDataTypes.Device;
using MRFlow.Types;
using NodeTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MRFlow.Component 
{
   
   public class DeviceNode : MRNode
{
   
   [SerializeField] TextMeshProUGUI deviceStatusText;
   [SerializeField] Image nodeBackgroundImage;

   
   [SerializeField] string device_name = "Device Node";
   [SerializeField] string description = "Just a Simple Device";
   [SerializeField] ActionDataTypes.DeviceType deviceType;

   
   [SerializeField] string device_data_id = "";

   [SerializeField] bool isDebug = false;

    override protected void Start() {
    }
   

    public override void InitNewNode() {
         IActionBlock deviceBlock = new DeviceBlockData(
            Guid.NewGuid(),
            this.device_name,
            this.description  ,
            BlockActionTypeMap.GetDeviceTypeString(deviceType),
            this.device_data_id
         );

        this.nodeType = NodeType.Sensor_Thermometer;
         MRNodeData mrNodeData= new MRNodeData(
                Guid.NewGuid(),
                NodeTypeMap.GetNodeTypeString(this.nodeType),
                deviceBlock,
                this.transform.position
         );


        this.SetMRNodeData(mrNodeData);
    }



        public override void SetMRNodeData(MRNodeData mRNodeData)
        {
      
            base.SetMRNodeData(mRNodeData);


        }


        protected override void OnReceiveMsgFromServer(string payload)
        {
            MqttDataType mqttData = JsonUtility.FromJson<MqttDataType>(payload);
            if(mqttData.data_type == "boolean")
            {
                  Debug.Log($"<color=green>[{this.device_name}]DeviceNode: {mqttData.value}</color>");
                  Debug.Log($"<color=yellow>[{this.device_name}]It Should Be: {mqttData.value.ToLower() == "true" }</color>");
                  this.deviceStatusText.text = mqttData.value.ToLower() == "true" ? "ON" : "OFF";
                  this.deviceStatusText.color = mqttData.value.ToLower() == "true" ? Color.green : Color.black;
            }

            if(mqttData.data_type == "string")
            {
                  Debug.Log($"<color=green>Data Type: {mqttData.data_type}</color>");
                  Debug.Log($"<color=green>DeviceNode: {mqttData.value}</color>");
            }

            if(mqttData.data_type == "number")
            {
                  Debug.Log($"<color=green>Data Type: {mqttData.data_type}</color>");
                  Debug.Log($"<color=green>DeviceNode: {mqttData.value}</color>");
                
                  if(float.TryParse(mqttData.value, out float value))
                  {
                        deviceStatusText.text = value.ToString();
                  }
            }
        }   




    }
}