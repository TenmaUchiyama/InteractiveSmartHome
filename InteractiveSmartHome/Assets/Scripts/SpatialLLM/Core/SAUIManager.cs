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
<<<<<<< HEAD
    [JsonProperty("llm_response")]
    public string Response { get; set; }
=======
    public string output { get; set; }
>>>>>>> stack
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
        if(LLMQueryRequest.Instance)LLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReceiveResponseFromLLM);
    }

  
    private void OnDestroy() {
        // if(SASpeechRecognizer.Instance)SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        if(LLMQueryRequest.Instance)LLMQueryRequest.Instance.OnReceiveResponseFromLLM.RemoveListener(OnReceiveResponseFromLLM);
    }


      private void OnReceiveResponseFromLLM(string arg0)
    {

       spinner.SetActive(false);
       agentResponseObject.SetActive(true); 

      try
        {
            // JSONを型付きクラスにデシリアライズ
            LLMResponse response = JsonConvert.DeserializeObject<LLMResponse>(arg0);

<<<<<<< HEAD
            if (response != null && !string.IsNullOrEmpty(response.Response))
            {
                Debug.Log($"<color=yellow>[SAUIManager] {response.Response}</color>");

                if (agentResponseText != null)
                {
                    agentResponseText.text = response.Response;
=======
     
                Debug.Log($"<color=yellow>[SAUIManager] {response.output}</color>");

                if (agentResponseText != null)
                {
                    agentResponseText.text = response.output;
>>>>>>> stack
                }
                else
                {
                    Debug.LogWarning("agentResponseText is not assigned in the inspector.");
                }
<<<<<<< HEAD
            }
            else
            {
                Debug.LogError("llm_response is null or empty.");
                if (agentResponseText != null)
                {
                    agentResponseText.text = "Error: Response is empty.";
                }
            }
=======
        
>>>>>>> stack
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process JSON: {ex.Message}");
        }


<<<<<<< HEAD
       Invoke(nameof(ClearUI), 5f);
    }


    private void OnVoiceRecognized(string recognizedText)
=======
  
    }


    public void ClearDisplay ()
    {
        spinner.SetActive(false);
        agentResponseObject.SetActive(true); 
  

    }

    public void DisplaySendingLLM(string recognizedText)
>>>>>>> stack
    {
       if(spinner)spinner.SetActive(true);    
       userCommandObject.SetActive(true);
       userCommandText.text = recognizedText;
<<<<<<< HEAD
=======
       
>>>>>>> stack
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

<<<<<<< HEAD
=======
    public string GetRecognizedWord()
    {
        return userCommandText.text;
    }

>>>>>>> stack
    public void ClearRecognizedWord()
    {
        userCommandText.text = ""; 
        userCommandObject.SetActive(false);
<<<<<<< HEAD
=======
        agentResponseObject.SetActive(false);
>>>>>>> stack
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
