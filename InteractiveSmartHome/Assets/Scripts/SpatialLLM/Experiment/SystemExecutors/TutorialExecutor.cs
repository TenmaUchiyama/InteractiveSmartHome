using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using Oculus.Interaction;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Experiment;
using SpatialLLM.Network;
using UnityEngine;

public class TutorialExecutor : SystemExecutor
{


    [SerializeField] private RayInteractor rayInteractor;
    [SerializeField] private VRCircleSelector circleSelector;
    [SerializeField] private bool isPointer = false;
    [SerializeField] private bool isLabel = false;
    private string currentState = "waiting";
    
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

    void OnDestroy()
    {
        if (SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        }
    }

    private void OnVoiceRecognized(string recognizedText)
    {
        Debug.Log($"[TutorialExecutor] 音声認識結果1: {recognizedText}");
        saUIManager.SetRecognizedTxt(recognizedText);
        Debug.Log($"[TutorialExecutor] 音声認識結果2: {saUIManager.GetRecognizedWord()}");
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


        if (isPointer)
        {
              if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                Debug.Log("[PointerSystemExecutor] Aボタン押下：Ray Interactorを有効化");
                rayInteractor.gameObject.SetActive(true);
                circleSelector.SetSelectionStarted(false);
            }
            if (OVRInput.GetUp(OVRInput.RawButton.A))
            {
                Debug.Log("[PointerSystemExecutor] Aボタン離す：Ray Interactorを無効化");

                rayInteractor.gameObject.SetActive(false);
                circleSelector.SetSelectionStarted(true);
            }
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




           if (isLabel)
        {
            

         


            bool isGripHolding = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
        
                foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
                {
                    device.DisplayShowLabel(isGripHolding);
                }
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

                if (!PromptLLMQueryRequest.Instance.IsRequesting) await PromptLLMQueryRequest.Instance.SendQuery(userInput, "tutorial");

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

                foreach(var device in SADeviceRef.Instance.GetAllDevices())
                {
                    device.Init();
                }
                currentState = "preparation";
                break;

        }

    }
}
