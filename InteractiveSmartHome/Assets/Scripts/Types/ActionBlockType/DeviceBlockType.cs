using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ActionDataTypes.Device
{

    public record DeviceBlockData : ActionBlock
    {
        public string device_type {get; set;}
        public string device_data_id {get; set;}

        public DeviceBlockData(Guid id, string name, string description, string device_type, string device_data_id) : base(id, name, description, BlockActionTypeMap.GetActionType(ActionBlockType.Device))
        {
            this.device_type = device_type;
            this.device_data_id = device_data_id;
        }

    }
    public record ToggleButtonData : ActionBlock
    {
        public string buttonName {get; set;}
        public string buttonDescription {get; set;}
        public string buttonType {get; set;}
        public string buttonState {get; set;}

        public ToggleButtonData(Guid id, string name, string description, string type, string buttonName, string buttonDescription, string buttonType, string buttonState) : base(id, name, description, type)
        {
            this.buttonName = buttonName;
            this.buttonDescription = buttonDescription;
            this.buttonType = buttonType;
            this.buttonState = buttonState;
        }


    }
    
}