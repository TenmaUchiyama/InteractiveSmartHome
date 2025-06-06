using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Oculus.Interaction;
using SpatialLLM.Core;
using SpatialLLM.Device;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialLLM.Experiment
{
    public class PointerSystemExecutor : SystemExecutor
    {   private bool isGripHolding = false;
        private string currentState = "preparation";
        [SerializeField] private RayInteractor rayInteractor; 
        [SerializeField] private VRCircleSelector circleSelector;
        [SerializeField] private WordLogger wordLogger;
        string recognizedWord = "";




        protected override void Start()
        {
            base.Start();

            if (SASpeechRecognizer.Instance)
            {
                SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
            }



             rayInteractor.gameObject.SetActive(false);

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

        

            // --- Trigger録音処理 ---
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) && !isGripHolding )
            {
                SASpeechRecognizer.Instance.ActivateVoice();
                Debug.Log("[LabelSystemExecutor] Trigger押下：録音開始");
            }

            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) && !isGripHolding)
            {
                SASpeechRecognizer.Instance.DeactivateVoice();
                Debug.Log("[LabelSystemExecutor] Trigger離す：録音終了");
            }

            // --- Yボタン処理 ---
            if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y))
            {
                YOperation();
            }


            if (Input.GetKeyDown(KeyCode.V))
            {
                OnVoiceRecognized("テスト");
            }



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
            this.isDeviceOperated = false;
            this.deviceOperatable = true;
            await UniTask.WaitUntil(() => this.isDeviceOperated); 
            this.deviceOperatable = false;
            this.isDeviceOperated = false;
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
            saUIManager.SetInstructionText("Press Trigger to Record");
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