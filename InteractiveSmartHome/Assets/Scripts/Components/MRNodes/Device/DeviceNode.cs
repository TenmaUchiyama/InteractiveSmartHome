using System;
using ActionDataTypes;
using ActionDataTypes.Device;
using MRFlow.Component;
using NodeTypes;
using UnityEngine;

namespace MRFlow.Component 
{
   
   public class DeviceNode : MRNode
{

   [SerializeField] string device_name = "Device Node";
   [SerializeField] string description = "Just a Simple Device";
   [SerializeField] NodeType nodeType;
   [SerializeField] ActionDataTypes.DeviceType deviceType;
   
   [SerializeField] string device_data_id = "";


   

    public override void InitNewNode() {
         IActionBlock deviceBlock = new DeviceBlockData(
            Guid.NewGuid(),
            this.device_name,
            this.description  ,
            BlockActionTypeMap.GetDeviceTypeString(deviceType),
            this.device_data_id
         );

         MRNodeData newNode = new MRNodeData(
                Guid.NewGuid(),
                NodeTypeMap.GetNodeTypeString(nodeType),
                deviceBlock,
                this.transform.position
         );


            this._mrNodeData = newNode;
    }



}
}