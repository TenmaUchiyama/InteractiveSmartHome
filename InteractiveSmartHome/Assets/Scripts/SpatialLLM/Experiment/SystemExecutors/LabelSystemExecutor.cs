using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using SpatialLLM.Core;
using SpatialLLM.Device;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace SpatialLLM.Experiment
{
    public class LabelSystemExecutor : SystemExecutor
    {
        private bool isGripHolding = false;
        private string currentState = "preparation";
        [SerializeField] private WordLogger wordLogger;
        string recognizedWord = "";

        protected override void Start()
        {
            base.Start();

            if (SASpeechRecognizer.Instance)
            {
                SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
            }

           
        }

        private void OnVoiceRecognized(string recognizedText)
        {
            saUIManager.SetRecognizedTxt(recognizedText);

            if (!saUIManager.IsRecognizedWordEmplty())
            {
                saUIManager.SetInstructionText("Press Y to confirm");
                recognizedWord = saUIManager.GetRecognizedWord();

                currentState = "recorded";
            }
        }

        protected override void Update()
        {
            base.Update();
            if (!isStarted) return;



            bool isRecording = false;

            // --- Trigger録音処理 ---
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) && !isGripHolding)
            {
                SASpeechRecognizer.Instance.ActivateVoice();
                Debug.Log("[LabelSystemExecutor] Trigger押下：録音開始");
                isRecording = true;
            }

            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) && !isGripHolding)
            {
                SASpeechRecognizer.Instance.DeactivateVoice();
                Debug.Log("[LabelSystemExecutor] Trigger離す：録音終了");
            }


            isGripHolding = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
            if (!isRecording)
            {
                foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
                {
                    device.DisplayShowLabel(isGripHolding);
                }
            }
            // --- Yボタン処理 ---
            if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y))
            {
                YOperation();
            }

            // --- キャンセル処理 ---
            if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
            {
                experimentManager.BackToShowDevice();
            }
        }
private async void YOperation()
{
    Debug.Log($"<color=green>YOperation: {currentState}</color>");

    switch (currentState)
    {
        case "preparation":
            // 準備中。録音前なので何もしない。
            break;

        case "recorded":
            // 音声認識が完了した直後
             saUIManager.StartSendLLM();
            await UniTask.Delay(System.TimeSpan.FromSeconds(2));
                    string outputText = "";
            experimentManager.GetCurrentArrangeData().devices.ForEach(device =>
            {
                outputText += $"{device.device.name}";
            });
            outputText += "を操作しました";
            saUIManager.FinishLoadingAndDisplayResponse(outputText);
            currentState = "checking";
                    YOperation();
            break;


        case "checking":
            // 処理結果（操作内容など）を表示
            experimentManager.DisplayCurrentOperation();

            saUIManager.SetInstructionText("Press Y to complete");
             wordLogger.AddRecognizedEntry(experimentManager.GetCurrentTaskId(), recognizedWord);
            currentState = "done";
            break;

        case "done":
                    // 完了。リセット。
           
            CompleteOperation();
            saUIManager.ClearRecognizedWord();
            currentState = "preparation";
            break;
    }
}

        public override void BeginOperation()
        {
            base.BeginOperation();
            currentState = "preparation";
            saUIManager.SetInstructionText("Press Trigger to Record, Grip to Show Labels");
        }

        public override void CompleteOperation()
        {
            base.CompleteOperation();

            // ラベル非表示
            foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
            {
                device.DisplayShowLabel(false);
            }
        }
    }
}


