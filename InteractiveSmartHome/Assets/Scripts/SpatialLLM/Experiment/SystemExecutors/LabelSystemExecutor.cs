using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using SpatialLLM.Core;
using SpatialLLM.Device;

namespace SpatialLLM.Experiment
{
    public class LabelSystemExecutor : SystemExecutor
    {
        private bool isGripHolding = false;
        private bool isOperationDone = false;

        private bool isTriggarable = false;

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
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isStarted) return;

            // --- ラベル表示処理（Gripボタン押し中） ---
            isGripHolding = OVRInput.Get(OVRInput.RawButton.LHandTrigger);

            foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
            {
                device.DisplayShowLabel(isGripHolding && !SASpeechRecognizer.Instance.IsActive);
            }

            // --- Trigger録音処理 ---
            if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
                if (isGripHolding) return;

                SASpeechRecognizer.Instance.ActivateVoice();
                Debug.Log("[LabelExecutor] Trigger押下：録音開始");
            }

            if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
            {
                if (isGripHolding) return;

        
                SASpeechRecognizer.Instance.DeactivateVoice();
                Debug.Log("[LabelExecutor] Trigger離す：録音終了");
            }

            // --- Yボタン処理 ---
            if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                if (!saUIManager.IsRecognizedWordEmplty())
                {
                    if (!isOperationDone)
                    {
                        experimentManager.DisplayCurrentOperation();
                        isOperationDone = true;
                        saUIManager.SetInstructionText("Press Y to complete");
                    }
                    else
                    {
                        CompleteOperation();
                        isOperationDone = false;
                        saUIManager.ClearRecognizedWord();
                    }
                }
            }

            // --- キャンセル（XボタンまたはESC） ---
            if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
            {
                experimentManager.BackToShowDevice();
            }
        }

        public override void BeginOperation()
        {
            base.BeginOperation();
            saUIManager.SetInstructionText("Press Trigger to Record, Grip to Show Labels");
        }

        public override void CompleteOperation()
        {
            base.CompleteOperation();

            // 終了時にすべてのデバイスのラベルを非表示にする
            foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
            {
                device.DisplayShowLabel(false);
            }
        }
    }
}
