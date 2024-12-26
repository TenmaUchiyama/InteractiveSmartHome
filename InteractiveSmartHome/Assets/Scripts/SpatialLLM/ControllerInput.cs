using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Type;
using UnityEngine;

public class ControllerInput : MonoBehaviour
{
  


private void Start() {
    Debug.Log("<color=yellow>Hello</color>");
}

   private void Update() {
        if(OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            Debug.Log("[ControllerInput] Pressed");
            
            SASpeechRecognizer.Instance.ActivateVoice();
        }


        if(OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
             Debug.Log("[ControllerInput] Released");
    
            SASpeechRecognizer.Instance.DeactivateVoice();
        }



       if (Input.GetKeyDown(KeyCode.T))
        {

            Debug.Log("[ControllerInput] A Pressed");
            List<SpatialLLM.Network.NetworkDataType.DeviceSpatialData> data = SpatialAwarnessProvider.Instance.DirectionFunction(DirectionUtil.GetDirection(DirectionUtil.Direction.Right), "high"); 


            Debug.Log($"<color=yellow>[ControllerInput] Data: {data.Count}</color>");

            foreach(var device in data)
            {
                Debug.Log($"<color=yellow>[ControllerInput] Device: {device.name}</color>");
            }


        
        }


    if (Input.GetKeyDown(KeyCode.Space))
        {

            Debug.Log("[ControllerInput] Space Pressed");
            SASpeechRecognizer.Instance.ToggleVoiceActivation(); 
        
        }

   }
}
