using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Json;
using SpatialLLM.Core;
using SpatialLLM.Experiment;
using SpatialLLM.Network;
using UnityEngine;



public class EvaluatorExecutor : SystemExecutor
{
    private bool isGripHolding = false;
    private bool isOperationDone = false;
    private bool isTriggarable = false;

    private string recognizedWord = "";
    private string lastRecognizedWord = "";

    private string currentState = "waiting";
    private string latestLLMOutput = "[No output]";
    private string agentMsg = "";
         private bool gripHeld = false;


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
            OperationProceed(); 
    }
    }

    void OnDestroy()
    {
        if (SASpeechRecognizer.Instance)
        {
            SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        }
    }
    


       private void StateUIHelper()
{
    // まず全ラベルをクリア
    uiButtonHelper.CloseAllLabels();

    switch (currentState)
    {
        case "preparation":
            // Trigger を押して録音開始
            uiButtonHelper.SetLabel(UIButtonLabelType.LeftTrigger, "音声録音", Color.white, Color.gray);
           uiButtonHelper.SetLabel(UIButtonLabelType.LeftThumbstick, "もう一度確認", Color.black, Color.yellow);
            break;

        case "recorded":
                // 録音完了 → 送信・キャンセル
                  uiButtonHelper.SetLabel(UIButtonLabelType.LeftTrigger, "音声録音", Color.white, Color.gray);
            uiButtonHelper.SetLabel(UIButtonLabelType.Y, "送信", Color.black, Color.green);
                uiButtonHelper.SetLabel(UIButtonLabelType.X, "取り消し", Color.black, Color.red);
                uiButtonHelper.SetLabel(UIButtonLabelType.LeftThumbstick, "もう一度確認", Color.black, Color.yellow);
            break;

        case "received":
            // LLMレスポンス待ち中は特に何も表示しない or ローディングUI
            break;

        case "checking":
            uiButtonHelper.SetLabel(UIButtonLabelType.LeftGrip, "押して操作", Color.black, Color.gray);
            if (gripHeld)
            {
                uiButtonHelper.SetLabel(UIButtonLabelType.Y, "確定", Color.black, Color.green);
                uiButtonHelper.SetLabel(UIButtonLabelType.X, "やり直し", Color.black, Color.red);
                saUIManager.SetInstructionText("Grip+Y to confirm, Grip+X to retry");
            }
            else
            {
                uiButtonHelper.CloseLabel(UIButtonLabelType.Y);
                uiButtonHelper.CloseLabel(UIButtonLabelType.X);
                saUIManager.SetInstructionText("Gripを押しながらYかXを使ってください");
            }
            break;

        case "done":
        default:
            // 完了時は全ラベルを閉じる
            uiButtonHelper.CloseAllLabels();
            break;
    }
}


    private void OnVoiceRecognized(string recognizedText)
    {

        saUIManager.SetInstructionText("Press Y to send to Agent");

        // 上書きから追記に変更 ↓↓↓
        recognizedWord += recognizedText + " "; // スペースで区切る
        lastRecognizedWord = recognizedText; // 最後の認識結果を保存
        saUIManager.SetRecognizedTxt(recognizedWord);
        currentState = "recorded";
        StateUIHelper();
        Debug.Log($"[LabelExecutor] 音声認識結果を更新: {recognizedWord}");
    }

    protected override void OnLeftThumbstickLeftFlick()
    {
        base.OnLeftThumbstickLeftFlick();

        // recognizedWordの一番後ろの文字を消す
        if (recognizedWord.Length > 0)
        {
            recognizedWord = recognizedWord.Substring(0, recognizedWord.Length - 1);
            lastRecognizedWord = recognizedWord; // 最後の認識結果も更新
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
        

          if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
    {
        gripHeld = true;
        StateUIHelper();
        Debug.Log("[LabelSystemExecutor] Grip押下");
    }

    if (OVRInput.GetUp(OVRInput.RawButton.LHandTrigger))
    {
        gripHeld = false;
        StateUIHelper();
        Debug.Log("[LabelSystemExecutor] Grip離上");
    }

    // ─── Yボタン（送信／確定） ──────────────────────────
        if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y))
        {
            if (currentState == "checking")
            {
                if (gripHeld)
                {
                    currentState = "done";
                    OperationProceed();
                }
                else
                {
                    Debug.Log("[LabelSystemExecutor] GripなしでY押下：アクションなし");
                }
            }
            else
            {
                OperationProceed();
            }
        }

    // ─── Xボタン（リセット／やり直し） ───────────────────
     if (OVRInput.GetDown(OVRInput.RawButton.X))
        {


                if (currentState != "checking")
                {
                    //　X押されたとき、認識していた音声データをリセット
                    ResetRecognizedWord();
                }
                else
                {

                    if (gripHeld)
                    {
                          ResetRecognizedWord();             // 音声入力リセット
                        currentState = "preparation";      // 戻る！
                        OperationProceed();
                    }
                    else
                    {
                        Debug.Log("Y pressed without Grip - no action");
                    }
                }
           
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

        StateUIHelper(); // UIラベルの更新
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
                    if (recognizedWord == "")
                    {
                        saUIManager.SetInstructionText("No input recognized. Please try again.");
                        return; // 入力がない場合は何もしない
                    }
                    var sendinData = new
                    {
                        llm_message = recognizedWord, 
                        task_id = experimentManager.GetCurrentTaskId().ToString(),
                        attempt_id = this.experimentTask.NextGuid
                    };
                    string json = JsonConvert.SerializeObject(sendinData);
                    await LLMQueryRequest.Instance.SendQuery("llm_agent_multi", json);
                }
                break;

            case "received":
                List<string> outputids = SADeviceRef.Instance.GetAllOperatedDevices().Select(d=> d.GetDeviceID()).ToList();

                this.experimentTask.AddTaskAttempt(
                    lastRecognizedWord, // 最後の認識結果を使用
                    this.elapsedTime.ToString(),
                    outputids
                );
                currentState = "checking";
                 this.timerStarted = false; 
                this.elapsedTime = 0f; // タイマーをリセット
                OperationProceed();
                break;

            case "checking":
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
