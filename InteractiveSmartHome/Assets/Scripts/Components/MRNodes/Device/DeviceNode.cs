using System;
using System.Collections;
using System.Collections.Generic;
using ActionDataTypes;
using ActionDataTypes.Device;
using MRFlow.Component;
using NodeTypes;
using UnityEngine;

public class DeviceNode : MRNode
{

   [SerializeField] string device_name = "Device Node";
   [SerializeField] string description = "Just a Simple Device";
   
   [SerializeField] string device_data_id = "";

    private void Awake() {
         ActionBlock deviceBlock = new DeviceBlockData(
            Guid.NewGuid(),
            this.device_name,
            this.description  ,
            BlockActionTypeMap.GetActionType(ActionBlockType.Device),
            this.device_data_id
         );

         MRNodeData newNode = new MRNodeData(
                Guid.NewGuid(),
                BlockActionTypeMap.GetActionType(ActionBlockType.Device),
                deviceBlock,
                this.transform.position
         );


            this._mrNodeData = newNode;
    }



}
