using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Network;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpatialLLM.Experiment
{
    public class LabelSystemExecutor : SystemExecutor
    {
        private string currentState = "preparation";
        private string recognizedWord = "";
        private string lastRecognizedWord = "";
        private string latestLLMOutput = "[No output]";
        private bool gripHeld = false;



        protected override void Start()
        {
            base.Start();

            if (SASpeechRecognizer.Instance)
            {
                SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
            }

            if (LabelLLMQueryRequest.Instance)
            {
                LabelLLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReceiveResponseFromLLM);
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
         StateUIHelper();  // ← これを絶対に呼ぶ！
        Debug.Log($"[LabelExecutor] 音声認識結果を更新: {recognizedWord}");
}




    protected override void OnLeftThumbstickLeftFlick()
    {
        base.OnLeftThumbstickLeftFlick();

        // recognizedWordの一番後ろの文字を消す
        if (recognizedWord.Length > 0)
        {
            recognizedWord = recognizedWord.Substring(0, recognizedWord.Length - 1);
            lastRecognizedWord = recognizedWord; // 最後の認識結果を更新
            saUIManager.SetRecognizedTxt(recognizedWord);
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

    // ─── トリガー：録音開始／終了 ──────────────────────
    if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
    {
        SASpeechRecognizer.Instance.ActivateVoice();
        Debug.Log("[LabelSystemExecutor] Trigger押下：録音開始");
    }

    if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
    {
        SASpeechRecognizer.Instance.DeactivateVoice();
        Debug.Log("[LabelSystemExecutor] Trigger離上：録音終了");
    }

    // ─── 左スティック押下（最後の文字削除） ────────────────
    if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick))
    {
        OnLeftThumbstickLeftFlick();
        StateUIHelper();
        Debug.Log("[LabelSystemExecutor] LeftThumbstick押下：最後の文字削除");
    }

    // ─── Grip 押下／離上 ────────────────────────────────
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

    // ─── キャンセル（Escape or スティック押し込み） ───────────
    if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
    {
        ResetRecognizedWord();
        currentState = "preparation";
        experimentManager.BackToShowDevice();
        StateUIHelper();
        Debug.Log("[LabelSystemExecutor] キャンセル実行");
    }
}

        private async void OperationProceed()
        {
            Debug.Log($"<color=green>LabelExecutor State: {currentState}</color>");
        StateUIHelper();
            switch (currentState)
            {
                case "preparation":
                    SADeviceRef.Instance.ClearAllDeviceOperation();
                    saUIManager.SetInstructionText("Press Trigger to Record");
                    this.timerStarted = true;
                    break;

                case "recorded":
                    saUIManager.DisplaySendingLLM(recognizedWord);
                    saUIManager.StartSendLLM();
                    string taskId = experimentManager.GetCurrentTaskId();
                    if (!LLMQueryRequest.Instance.IsRequesting)
                    {

                        if (recognizedWord == "")
                        {
                        
                            return;
                        }
                        LLMQueryRequestType llmQueryRequest = new LLMQueryRequestType()
                        {
                            task_id = taskId,
                            attempt_id = this.experimentTask.NextGuid,
                            llm_message = recognizedWord,
                        };




                        try
                        {
                            await LabelLLMQueryRequest.Instance.SendQuery(recognizedWord, taskId, this.experimentTask.NextGuid);
                        }
                        catch
                        {
                            Debug.LogError("Failed to send LLM query for label operation.");
                            saUIManager.FinishLoadingAndDisplayResponse("[LLM Error]");
                            currentState = "received";
                            this.timerStarted = false;
                            this.elapsedTime = 0f; // タイマーをリセット
                            return;
                        }
                       
                    }
                    break;

                case "received":

                List<string> operatedIds = new List<string>();
                    var devices = SADeviceRef.Instance.GetAllOperatedDevices(); // 型を確認
                    foreach (var d in devices)
                    {
                        string id = d.GetDeviceID();  // ここでエラーが出るなら GetDeviceID が曖昧な定義
                        operatedIds.Add(id);
                    }

                    // 1. 発話記録を追加
                    this.experimentTask.AddTaskAttempt(
                        lastRecognizedWord,
                        this.elapsedTime.ToString(),
                        operatedIds
                    );

                    // 2. ログ保存
                    this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());

                    saUIManager.FinishLoadingAndDisplayResponse(latestLLMOutput);
                    currentState = "checking";
                    this.timerStarted = false; 
                    this.elapsedTime = 0f; // タイマーをリセット
                    OperationProceed();
                    break;

                case "checking":
                
                    saUIManager.SetInstructionText("Grip+Y to confirm, or press Y to retry");
       
                    break;

                case "done":
                // 操作されたデバイスIDを取得
             

           

    // 2. ログ保存
    this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());
                    saUIManager.ClearRecognizedWord();
                    CompleteOperation();
                    currentState = "preparation";
                    break;
            }
        }

        private void OnReceiveResponseFromLLM(string json)
        {
            try
            {
                recognizedWord = ""; // 受信後にリセット
                var response = JsonConvert.DeserializeObject<LLMResponse>(json);
                latestLLMOutput = response.output ?? "[No output]";
                Debug.Log($"<color=cyan>LLM Output Saved: {latestLLMOutput}</color>");
                currentState = "received";
                OperationProceed();
            }
            catch (System.Exception e)
            {
                Debug.LogError("LLMレスポンスの解析に失敗: " + e.Message);
                latestLLMOutput = "[LLM Error]";
                currentState = "received";
                OperationProceed();
            }
        }

        public override void BeginOperation()
        {
            base.BeginOperation();
            currentState = "preparation";
            saUIManager.SetInstructionText("Press Trigger to Record");
        }

        public override void CompleteOperation()
        {
            base.CompleteOperation();
            foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
            {
                device.DisplayShowLabel(false);
            }
        }
    }
}
