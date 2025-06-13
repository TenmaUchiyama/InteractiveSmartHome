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
        private string latestLLMOutput = "[No output]";



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
            saUIManager.SetRecognizedTxt(recognizedText);

            if (!saUIManager.IsRecognizedWordEmplty())
            {
                Debug.Log($"<color=green>Recognized Word: {recognizedText}</color>");
                recognizedWord = saUIManager.GetRecognizedWord();
                saUIManager.SetInstructionText("Press Y to send to Agent");
                currentState = "recorded";
            }
        }

        protected override void Update()
        {
            base.Update();
            if (!isStarted) return;

            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
                SASpeechRecognizer.Instance.ActivateVoice();
                Debug.Log("[LabelSystemExecutor] Trigger押下：録音開始");
            }

            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
            {
                SASpeechRecognizer.Instance.DeactivateVoice();
                Debug.Log("[LabelSystemExecutor] Trigger離す：録音終了");
            }

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

            if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
            {
                experimentManager.BackToShowDevice();
            }
        }

        private async void OperationProceed()
        {
            Debug.Log($"<color=green>LabelExecutor State: {currentState}</color>");

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
                        recognizedWord,
                        this.elapsedTime.ToString(),
                        operatedIds
                    );

                    // 2. ログ保存
                    this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());

                    saUIManager.FinishLoadingAndDisplayResponse(latestLLMOutput);
                    currentState = "checking";
                    this.timerStarted = false; 
                    this.elapsedTime = 0f; // タイマーをリセット
                    break;

                case "checking":
                    saUIManager.SetInstructionText("Grip+Y to confirm, or press Y to retry");
                   this.currentState = "preparation";
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
