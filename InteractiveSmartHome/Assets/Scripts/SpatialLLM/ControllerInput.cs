using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;

public class ControllerInput : MonoBehaviour
{
  

private bool IsActive = false;
   private void Update() {
        if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {
            Debug.Log("[ControllerInput] Pressed");
            if(LLMQueryRequest.Instance.IsRequesting) return;
            SASpeechRecognizer.Instance.ActivateVoice();
            IsActive = true;
        }


        if(OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
             Debug.Log("[ControllerInput] Released");

            if(!IsActive) return;
            SASpeechRecognizer.Instance.DeactivateVoice();
            IsActive = false;
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
