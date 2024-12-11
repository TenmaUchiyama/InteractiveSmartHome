using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using UnityEngine;

public class ControllerInput : MonoBehaviour
{
   [SerializeField] Renderer renderer; 


private void Start() {
    Debug.Log("<color=yellow>Hello</color>");
    renderer.material.color  = Color.white;
}

   private void Update() {
        if(OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            Debug.Log("[ControllerInput] Pressed");
            renderer.material.color = Color.red; 
            SASpeechRecognizer.Instance.ActivateVoice();
        }


        if(OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
             Debug.Log("[ControllerInput] Released");
            renderer.material.color = Color.white; 
            SASpeechRecognizer.Instance.DeactivateVoice();
        }



    if (Input.GetKeyDown(KeyCode.Space))
        {
            SASpeechRecognizer.Instance.ToggleVoiceActivation(); 
        
        }

   }
}
