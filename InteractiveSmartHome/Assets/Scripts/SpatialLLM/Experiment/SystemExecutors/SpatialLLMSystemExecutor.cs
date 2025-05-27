using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using SpatialLLM.Core;
using SpatialLLM.Experiment;
using SpatialLLM.Network;
using UnityEngine;

[System.Serializable]
public class LLMResponse
{
    public string output;   
    public string error;   
    public string detail;   
}


public class SpatialLLMSystemExecutor : SystemExecutor
{

    private bool isGripHolding = false;
    private bool isOperationDone = false;

    private bool isTriggarable = false;


    private string currentState = "waiting";
    [SerializeField] private SAUIManager saUIManager;

    private string agentMsg = "";
    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
        }

        if (PromptLLMQueryRequest.Instance) PromptLLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReiveResponseFromLLM);
    }

    private void OnReiveResponseFromLLM(string arg0)
    {

        try
        {
            LLMResponse response = JsonConvert.DeserializeObject<LLMResponse>(arg0);
            saUIManager.FinishLoadingAndDisplayResponse(response.output);
            currentState = "received";
            YOperation(); // 成功した場合のみ処理を継続
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to parse LLM response: " + ex.Message);
        }
    }

    void Onestroy()
    {
        if (SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        }
    }

    private void OnVoiceRecognized(string recognizedText)
    {
        saUIManager.SetRecognizedTxt(recognizedText);

        if (!saUIManager.IsRecognizedWordEmplty())
        {
            saUIManager.SetInstructionText("Press Y to send to Agent");
            currentState = "recorded";
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



        if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y))
        {
            YOperation();
        }

        // --- キャンセル（XボタンまたはESC） ---
        if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
        {
            experimentManager.BackToShowDevice();
        }

    }



    private async void YOperation()
    {

        Debug.Log($"<color=yellow>YOperation: {currentState}</color>");
        switch (currentState)
        {
            case "preparation":

                break;
            case "recorded":
                string userInput = saUIManager.GetRecognizedWord();
                saUIManager.DisplaySendingLLM(userInput);
                saUIManager.StartSendLLM();

                if (!PromptLLMQueryRequest.Instance.IsRequesting) await PromptLLMQueryRequest.Instance.SendQuery(userInput, experimentManager.GetCurrentTaskId());

                break;
            case "received":

                saUIManager.SetInstructionText("Press Y to proceed");
                currentState = "checking";
                break;
            case "checking":
                saUIManager.SetInstructionText("Press Y to next task");
                currentState = "done";
                break;
            case "done":

                saUIManager.ClearRecognizedWord();
                CompleteOperation();
                currentState = "preparation";
                break;

        }

    }

}
