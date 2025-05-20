using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpatialLLM.Core;
using SpatialLLM.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TestSystemExecutor : MonoBehaviour
{

    [SerializeField] SAUIManager saUIManager;

    bool isCommandVisible= false;
    bool isAgentAnswerVisible = false;
    string recognizedText = "";
    
    void Start()
        {
            if(SASpeechRecognizer.Instance)SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
       


            if(PromptLLMQueryRequest.Instance)PromptLLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReiveResponseFromLLM);
        }

        private async void OnReiveResponseFromLLM(string arg0)
{
        Debug.Log("LLM response: " + arg0);
        
        LLMResponse llmResponse = JsonConvert.DeserializeObject<LLMResponse>(arg0);
        saUIManager.FinishLoadingAndDisplayResponse(llmResponse.output);
        isAgentAnswerVisible = true;
        
  
 
}

    private void OnVoiceRecognized(string recognizedText)
    {
        Debug.Log("[TEST SYSTEM EXECUTOR] RECOGNIZED");
        saUIManager.DisplaySendingLLM(recognizedText);
        isCommandVisible = true;
        this.recognizedText = recognizedText.Trim();
    }


    
    async void Update()
    {
             if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
           

                Debug.Log("[LabelExecutor] Trigger押下：録音開始");
                SASpeechRecognizer.Instance.ActivateVoice();
            }

            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
            {
               
        
                Debug.Log("[LabelExecutor] Trigger離す：録音終了");
                SASpeechRecognizer.Instance.DeactivateVoice();
            }


             if (OVRInput.GetUp(OVRInput.RawButton.Y))
             {
                if(isCommandVisible)
                {
                    saUIManager.StartSendLLM();
                    await PromptLLMQueryRequest.Instance.SendQuery(recognizedText);
                    isCommandVisible = false;

                }

                if(isAgentAnswerVisible)
                {
                    saUIManager.ClearUI();
                    isAgentAnswerVisible = false;
                }
             }


             if (OVRInput.GetDown(OVRInput.RawButton.Start))
            {
                saUIManager.ClearUI(); 
                isAgentAnswerVisible=false; 
                isCommandVisible = false; 
                recognizedText="";

            }


    }
}
