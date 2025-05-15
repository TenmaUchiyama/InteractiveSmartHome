using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpatialLLM.Core;
using SpatialLLM.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TestVoiceActivate : MonoBehaviour
{

    [SerializeField] SAUIManager saUIManager;
    
    void Start()
        {
            if(SASpeechRecognizer.Instance)
            {
                SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
            }



            if(LLMQueryRequest.Instance)LLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReiveResponseFromLLM);
        }

        private async void OnReiveResponseFromLLM(string arg0)
{
    Debug.Log("LLM response: " + arg0);


        LLMResponse response = JsonConvert.DeserializeObject<LLMResponse>(arg0);
        if (!string.IsNullOrEmpty(response.error))
        {
            Debug.LogError("LLM Error: " + response.detail);
            return;  // エラー時は currentState を変更しない
        }

       string userInput = saUIManager.GetRecognizedWord(); 
        saUIManager.DisplaySendingLLM(userInput);
        if(!LLMQueryRequest.Instance.IsRequesting) await LLMQueryRequest.Instance.SendQuery(userInput); 
 
}

    private void OnVoiceRecognized(string recognizedText)
    {
        saUIManager.SetRecognizedTxt(recognizedText);

            if (!saUIManager.IsRecognizedWordEmplty())
            {
                saUIManager.SetInstructionText("Press Y to send to Agent");
            }
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
            {  string userInput = saUIManager.GetRecognizedWord(); 
            saUIManager.DisplaySendingLLM(userInput);
            if(!PromptLLMQueryRequest.Instance.IsRequesting) await PromptLLMQueryRequest.Instance.SendQuery(userInput); 
}
    }
}
