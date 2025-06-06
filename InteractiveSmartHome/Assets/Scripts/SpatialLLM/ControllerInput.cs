using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Experiment;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;

public class ControllerInput : Singleton<MonoBehaviour>
{
  
    [SerializeField] private SystemExecutor systemExecutor;
    [SerializeField] private ExperimentManager experimentManager;
    private bool IsActive = false;
    private bool isControllable;



    private void Start() {
        if(systemExecutor) {
            systemExecutor.onBeginOperation.AddListener(()=>{isControllable = true;});
            systemExecutor.onCompleteOperation.AddListener(()=>{isControllable = false;});
            }
    }





   private void Update() {



        if (Input.GetKeyDown(KeyCode.J))
        { 
            SASpeechRecognizer.Instance.ActivateVoice();
            
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            SASpeechRecognizer.Instance.DeactivateVoice();
        }

        if (!isControllable) return;
        


        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {


            Debug.Log("[ControllerInput] Pressed");

            if (LLMQueryRequest.Instance.IsRequesting) return;
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




  

   }
}
