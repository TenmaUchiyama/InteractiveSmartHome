using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SpatialLLM.Core;
using SpatialLLM.Experiment;
using UnityEngine;

public class TestExecutor : SystemExecutor
{
    private bool isStarted = false;

    

    // Update is called once per frame
    void Update()
    {
        if(!isStarted) return;

        if(Input.GetKeyDown(KeyCode.I))
        {
            this.CompleteOperation();
        }




         if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
           

                SASpeechRecognizer.Instance.ActivateVoice();
                Debug.Log("[LabelExecutor] Trigger押下：録音開始");
            }

            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
            {
               
        
                SASpeechRecognizer.Instance.DeactivateVoice();
                Debug.Log("[LabelExecutor] Trigger離す：録音終了");
            }

    }




   
}
