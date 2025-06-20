using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private string recognizedWord = "";

    private string currentState = "waiting";
    private string latestLLMOutput = "[No output]";
    private string agentMsg = "";

    protected virtual void Start()
    {
        
        this.onBeginOperation.AddListener(() =>
        {
            currentState = "preparation"; 
            OperationProceed(); // 初期状態でYボタン操作を実行
        });
        if (SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
        }

        if (LLMQueryRequest.Instance)
            LLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReiveResponseFromLLM);
    }


 

    private void OnReiveResponseFromLLM(string arg0)
    {
        try
        {
            recognizedWord = ""; // 音声認識結果をクリア
            LLMResponse response = JsonConvert.DeserializeObject<LLMResponse>(arg0);
            latestLLMOutput = response.output ?? "[No output]";
            saUIManager.FinishLoadingAndDisplayResponse(response.output);
            currentState = "received";
            OperationProceed(); // 成功した場合のみ処理を継続
        }
        catch (System.Exception ex)
        {
            latestLLMOutput = "[LLM Error]";
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

        saUIManager.SetInstructionText("Press Y to send to Agent");

        // 上書きから追記に変更 ↓↓↓
        recognizedWord += recognizedText + " "; // スペースで区切る
        saUIManager.SetRecognizedTxt(recognizedWord);
        currentState = "recorded";
        Debug.Log($"[LabelExecutor] 音声認識結果を更新: {recognizedWord}");
}

    protected override void OnLeftThumbstickLeftFlick()
    {
        base.OnLeftThumbstickLeftFlick();

        // recognizedWordの一番後ろの文字を消す
        if (recognizedWord.Length > 0)
        {
            recognizedWord = recognizedWord.Substring(0, recognizedWord.Length - 1);
            saUIManager.SetRecognizedTxt(recognizedWord);
        }
    }
    
       private void ResetRecognizedWord()
    {
        recognizedWord = "";
        saUIManager.ClearRecognizedWord(); // もし存在しなければ SetRecognizedTxt("") などで代用
        saUIManager.SetInstructionText("Press Y to start recording");
    }

    protected override void Update()
    {



        base.Update();



        if (!isStarted) return;

        // 音声入力制御
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

        // --- Yボタン + Bボタン同時押し時のみ進める ---
        bool isYPressed = OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y);
        bool isGripped = OVRInput.Get(OVRInput.RawButton.LHandTrigger) || Input.GetKey(KeyCode.G);


        if (isYPressed)
        {
            if (isGripped)
            {
                currentState = "done";
            }

            OperationProceed();


        }

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            ResetRecognizedWord();
         
        }

        // --- キャンセル（XボタンまたはESC） ---
        if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
        {
            ResetRecognizedWord();
            currentState = "preparation";
            experimentManager.BackToShowDevice();
        }
    }

    private async void OperationProceed()
    {
        Debug.Log($"<color=yellow>OperationStates: {currentState}</color>");

        switch (currentState)
        {
            case "preparation":
                SADeviceRef.Instance.ClearAllDeviceOperation();
                saUIManager.SetInstructionText("Press Y to start recording");

                this.timerStarted = true;
                break;
    
            case "recorded":

                
              
                saUIManager.DisplaySendingLLM(recognizedWord);
                saUIManager.StartSendLLM();
                if (!LLMQueryRequest.Instance.IsRequesting)
                {
                    var sendinData = new
                    {
                        llm_message = recognizedWord, 
                        task_id = experimentManager.GetCurrentTaskId().ToString(),
                        attempt_id = this.experimentTask.NextGuid
                    };
                    string json = JsonConvert.SerializeObject(sendinData);
                    await LLMQueryRequest.Instance.SendQuery("llm_agent", json);
                }
                break;

            case "received":
                List<string> outputids = SADeviceRef.Instance.GetAllOperatedDevices().Select(d=> d.GetDeviceID()).ToList();

                this.experimentTask.AddTaskAttempt(
                    recognizedWord, 
                    this.elapsedTime.ToString(),
                    outputids
                );
                currentState = "checking";
                 this.timerStarted = false; 
                this.elapsedTime = 0f; // タイマーをリセット
                break;

            case "checking":
                currentState = "preparation";
                saUIManager.SetInstructionText("Grip+Y to confirm, or press Y to start over");
                break;

            case "done":
                this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());

                saUIManager.ClearRecognizedWord();
                CompleteOperation();
                currentState = "preparation";

                break;
        }
    }
}
