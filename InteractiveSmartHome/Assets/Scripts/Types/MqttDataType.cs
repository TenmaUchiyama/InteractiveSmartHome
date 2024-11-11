using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MRFlow.Types
{


    public record MqttDataType 
    {
        public string action_id;
        public string data_type; 
        public string value; 
    }



}