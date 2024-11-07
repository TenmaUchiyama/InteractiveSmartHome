using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ActionDataTypes.Device
{

    public record DeviceBlockData : IActionBlock
    {
        public string device_type {get; set;}
        public string device_data_id {get; set;}


         public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string action_type { get; set;}
        

    public DeviceBlockData(Guid id, string name, string description, string device_type, string device_data_id) 
        {
            this.device_type = device_type;
            this.device_data_id = device_data_id;


            this.id = id;
            this.name = name;
            this.description = description;
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Device);
        }

    }
    public record ToggleButtonData : IActionBlock
    {
        public string buttonName {get; set;}
        public string buttonDescription {get; set;}
        public string buttonType {get; set;}
        public string buttonState {get; set;}



        public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string type { get; set;}
        public string action_type { get; set;}

        public ToggleButtonData(Guid id, string name, string description, string type, string buttonName, string buttonDescription, string buttonType, string buttonState) 
        {
            this.buttonName = buttonName;
            this.buttonDescription = buttonDescription;
            this.buttonType = buttonType;
            this.buttonState = buttonState;



            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Device);
        }


    }
    
}