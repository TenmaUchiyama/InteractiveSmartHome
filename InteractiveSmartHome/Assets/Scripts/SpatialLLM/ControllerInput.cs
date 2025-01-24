using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Experiment;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;

public class ControllerInput : MonoBehaviour
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
        if(!isControllable) return;

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

            Debug.Log("[ControllerInput] T Pressed");
            List<SpatialLLM.Network.NetworkDataType.DeviceSpatialData> data = SpatialAwarnessProvider.Instance.DirectionFunction(DirectionUtil.GetDirection(DirectionUtil.Direction.Right), "high"); 


            Debug.Log($"<color=yellow>[ControllerInput] Data: {data.Count}</color>");

            foreach(var device in data)
            {
                Debug.Log($"<color=yellow>[ControllerInput] Device: {device.name}</color>");
            }


        
        }


        if(OVRInput.GetDown(OVRInput.RawButton.X))
        {
            systemExecutor.CompleteOperation();
        }

        if(Input.GetKeyDown(KeyCode.O))
        {
            systemExecutor.CompleteOperation();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACEKEY PRESSED"); 
             LLMQueryRequest.Instance.SendQueryForDebug("");
        }

    // if (Input.GetKeyDown(KeyCode.Space))
    //     {

    //         Debug.Log("[ControllerInput] Space Pressed");
    //         SASpeechRecognizer.Instance.ToggleVoiceActivation(); 
        
    //     }

   }
}
