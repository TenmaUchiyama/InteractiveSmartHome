using System;
using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Experiment;
using UnityEngine;

public class WofOSystemExecutor : SystemExecutor
{


       private bool isGripHolding = false;
        private bool isOperationDone = false;

        private bool isTriggarable = false;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        if(SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
        }
    }

    void Onestroy()
    {
        if(SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        }
    }

    private void OnVoiceRecognized(string recognizedText)
    {
        saUIManager.SetRecognizedTxt(recognizedText);

            if (!saUIManager.IsRecognizedWordEmplty())
            {
                saUIManager.SetInstructionText("Press Y to confirm");
            }
    }

    // Update is called once per frame
    protected override void Update()
    {
          base.Update();

            if (!isStarted) return;



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



             if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                if (!saUIManager.IsRecognizedWordEmplty())
                {
                    if (!isOperationDone)
                    {
                        experimentManager.DisplayCurrentOperation();
                        isOperationDone = true;
                        saUIManager.SetInstructionText("Press Y to complete");
                    }
                    else
                    {
                        CompleteOperation();
                        isOperationDone = false;
                        saUIManager.ClearRecognizedWord();
                    }
                }
            }

            // --- キャンセル（XボタンまたはESC） ---
            if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
            {
                experimentManager.BackToShowDevice();
            }

    }
}
