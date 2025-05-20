using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using SpatialLLM.Core;
using SpatialLLM.Network;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class SAUIManager : MonoBehaviour
{

    public class LLMResponse
{
    public string output { get; set; }
}

    [SerializeField] GameObject spinner;
    [SerializeField] GameObject userCommandObject;
    [SerializeField] TextMeshProUGUI userCommandText; 
    [SerializeField] TextMeshProUGUI instructionText;
    [SerializeField] TextMeshProUGUI deviceCountText;


     [SerializeField] GameObject  agentResponseObject;
    [SerializeField] TextMeshProUGUI agentResponseText;
    void Start()
    {
    

        // if(SASpeechRecognizer.Instance)SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
        if(PromptLLMQueryRequest.Instance) PromptLLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReceiveResponseFromLLM);
    }

  
    private void OnDestroy() {
        // if(SASpeechRecognizer.Instance)SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        if(PromptLLMQueryRequest.Instance) PromptLLMQueryRequest.Instance.OnReceiveResponseFromLLM.RemoveListener(OnReceiveResponseFromLLM);
    }


      private void OnReceiveResponseFromLLM(string arg0)
    {

       spinner.SetActive(false);
    //    agentResponseObject.SetActive(true); 

      try
        {
            // JSONを型付きクラスにデシリアライズ
            LLMResponse response = JsonConvert.DeserializeObject<LLMResponse>(arg0);

     
                Debug.Log($"<color=yellow>[SAUIManager] {response.output}</color>");

                if (agentResponseText != null)
                {
                    agentResponseText.text = response.output;
                }
                else
                {
                    Debug.LogWarning("agentResponseText is not assigned in the inspector.");
                }
        
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process JSON: {ex.Message}");
        }


  
    }


    public void FinishLoadingAndDisplayResponse (string agentOutput)
    {
        spinner.SetActive(false);
        agentResponseObject.SetActive(true); 
        agentResponseText.text = agentOutput;

    }


    public void StartSendLLM()
    {
        this.ClearUserCommand();
        if(spinner) spinner.SetActive(true); 
    }

    public void DisplaySendingLLM(string recognizedText)
    {
        
          userCommandText.text = recognizedText;
          userCommandObject.SetActive(true); 
    }

    


    public void ClearUserCommand()
    {
        userCommandObject.SetActive(false);
       userCommandText.text ="";
    }

    


    public string GetRecognizedWord() 
    {
        return userCommandText.text;
    }


    // private void Update() {
    //      if(OVRInput.GetDown(OVRInput.RawButton.Y))
    //     {
    //       ClearUI();
    //     }
    // }


    public void SetRecognizedTxt(string recognizedText)
    {
       userCommandObject.SetActive(true);
       userCommandText.text = recognizedText;
    }


    // public void ClearUI() 
    // {
    //     userCommandText.text = ""; 
    //     userCommandObject.SetActive(false); 

    //     agentResponseText.text = "";
    //     agentResponseObject.SetActive(false);


    //     spinner.SetActive(false);

    // }


    public void ClearInstruction() 
    {
        instructionText.text = ""; 
    }


    public void SetInstructionText(string instruction) 
    {
        instructionText.text = instruction;
    }

    public bool IsRecognizedWordEmplty() 
    {
        return userCommandText.text == "";
    }

    public void ClearRecognizedWord()
    {
        userCommandText.text = ""; 
        userCommandObject.SetActive(false);
        agentResponseObject.SetActive(false);
    }


    public void ClearDeviceCount() 
    {
        deviceCountText.text = ""; 
    }


    public void SetDeviceCountText(string deviceCount)
    {
        deviceCountText.text = "Num of Devices: " + deviceCount;
    }


    public void ClearUI() 
    {
        this.ClearDeviceCount();
        this.ClearInstruction();
        this.ClearRecognizedWord();
    }


    
}
