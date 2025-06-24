using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Network;
using System;
using Oculus.Platform;
using static SpatialLLM.Network.NetworkDataType;
using Oculus.Interaction;
using System.Reflection;

namespace SpatialLLM.Experiment
{
    public class PointerSystemExecutor : SystemExecutor
    {
        [SerializeField] private RayInteractor rayInteractor;
        [SerializeField] private VRCircleSelector circleSelector;
        [SerializeField] private bool IS_SPATIAL_POINTING = false; // Spatial Pointingを有効にするかどうか
        [SerializeField] private Camera userCamera;
        private string currentState = "pointing";
        private string latestLLMOutput = "[No output]";
        private string recognizedWord = "";


        
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



        private void UpdatePointingLabel(bool isAPressed)
{
    if (isAPressed)
    {
        // A押してる間：Trigger = 単一選択
        uibuttonHelper.SetLabel(
            UIButtonLabelType.RightTrigger,
            "単一選択",
            Color.black,
            Color.green
        );
        
        // Aのラベルは不要・または非表示にしてもよい
        uibuttonHelper.SetLabelVisible(UIButtonLabelType.A, false);
    }
    else
    {
        // A押してないとき：A = レーザーにする、Trigger = 範囲選択にする
        uibuttonHelper.SetLabel(
            UIButtonLabelType.A,
            "レーザーにする",
            Color.black,
            Color.gray
        );
        
        uibuttonHelper.SetLabel(
            UIButtonLabelType.RightTrigger,
            "範囲選択",
            Color.black,
            Color.blue
        );

        uibuttonHelper.SetLabelVisible(UIButtonLabelType.A, true);
    }
}


        protected override void Start()
        {
            base.Start();

            this.onBeginOperation.AddListener(()=>
            { 
                // UpdatePointingLabel(false);
            });

            if (PointingQueryRequest.Instance)
            {
                PointingQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener(OnReceiveResponseFromLLM);
            }

            if (SASpeechRecognizer.Instance)
            {
                SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
            }


            rayInteractor.gameObject.SetActive(false);
            circleSelector.SetSelectionStarted(true);
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
            // Aボタンの押下状態に応じてラベルを更新


            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {

                Debug.Log("[PointerSystemExecutor] Aボタン押下：Ray Interactorを有効化");
                rayInteractor.gameObject.SetActive(true);
                circleSelector.SetSelectionStarted(false);
                //  UpdatePointingLabel(true);
            }
            if (OVRInput.GetUp(OVRInput.RawButton.A))
            {
                Debug.Log("[PointerSystemExecutor] Aボタン離す：Ray Interactorを無効化");

                rayInteractor.gameObject.SetActive(false);
                circleSelector.SetSelectionStarted(true);
                // UpdatePointingLabel(false);

            }

            if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y))
            {
                bool isGripped = OVRInput.Get(OVRInput.RawButton.LHandTrigger) || Input.GetKey(KeyCode.G);

                if (isGripped)
                    currentState = "done";


                OperationProceed();
            }

              if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            ResetRecognizedWord();
            currentState = "pointing";
            OperationProceed();
        }

        // --- キャンセル（XボタンまたはESC） ---
        if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick))
        {
            ResetRecognizedWord();
            currentState = "pointing";
            experimentManager.BackToShowDevice();
        }
        }

        private async void OperationProceed()
        {
            Debug.Log($"<color=green>PointerExecutor State: {currentState}</color>");


            switch (currentState)
            {

                // 初期状態
                // ここで音声認識を開始する
                case "pointing":
                    // UpdatePointingLabel(false);

                    SADeviceRef.Instance.ClearAllDeviceOperation();
                    saUIManager.SetInstructionText("Point and Press Y to send");
                    elapsedTime = 0f;
                    timerStarted = true;
                    break;

                case "recorded":
                    saUIManager.DisplaySendingLLM("[Pointed Devices]");
                    saUIManager.StartSendLLM();

                    string taskId = experimentManager.GetCurrentTaskId();
                    var pointed = SADeviceRef.Instance.GetAllSelectedDevices();

                    if (!LLMQueryRequest.Instance.IsRequesting)
                    {

                        if (IS_SPATIAL_POINTING)
                        {
                            var pointedDevices = new List<DeviceSpatialData>();
                            foreach (var d in pointed)
                            {
                                pointedDevices.Add(d.GetDevicePositionalRelativeToUser(userCamera.transform));
                            }

                            await PointingQueryRequest.Instance.SendQuery(recognizedWord, taskId, this.experimentTask.NextGuid, pointedDevices);
                        }
                        else
                        {
                            var pointedIds = new List<DeviceLabelData>();

                            foreach (var d in pointed)
                            {
                                pointedIds.Add(new DeviceLabelData() { id = d.GetDeviceID(), name = d.GetDBDeviceData().device_name });
                                // ちゃんと送信するデバイスが選択されているか確認
                                if (d.GetDeviceID() == "")
                                {
                                    Debug.LogWarning("Selected device has no ID, skipping this device.");
                                    continue;
                                }
                                else
                                {
                                    Debug.Log($"Selected device ID: {d.GetDeviceID()}");
                                }
                            }
                            //　実際に送信するデータの中を見てみる
                            Debug.Log($"Sending {pointedIds.Count} devices to LLM: {JsonConvert.SerializeObject(pointedIds)}");
                            await PointingQueryRequest.Instance.SendQuery(recognizedWord, taskId, this.experimentTask.NextGuid, pointedIds);
                        }

                    }
                    else
                    {
                        Debug.LogWarning("LLM Query is already in progress, skipping this request.");
                    }



                    break;

                case "received":
                    List<string> operatedIds = new List<string>();
                    foreach (var d in SADeviceRef.Instance.GetAllOperatedDevices())
                    {
                        operatedIds.Add(d.GetDeviceID());
                    }

                    this.experimentTask.AddTaskAttempt(
                       recognizedWord,
                       this.elapsedTime.ToString(),
                       operatedIds
                   );


                    this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());

                    saUIManager.FinishLoadingAndDisplayResponse(latestLLMOutput);
                    currentState = "checking";
                    timerStarted = false;
                    elapsedTime = 0f;
                    break;

                case "checking":
                    saUIManager.SetInstructionText("Grip+Y to confirm, or press Y to retry");
                    currentState = "pointing";
                    break;

                case "done":
                    this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());
                    saUIManager.ClearRecognizedWord();
                    CompleteOperation();
                    currentState = "pointing";
                    break;
            }
        }

        private void OnReceiveResponseFromLLM(string json)
        {
            try
            {

                recognizedWord = ""; 
                var response = JsonConvert.DeserializeObject<LLMResponse>(json);
                latestLLMOutput = response.output ?? "[No output]";
                Debug.Log($"<color=cyan>LLM Output Saved: {latestLLMOutput}</color>");
                currentState = "received";

                SADeviceRef.Instance.UnSelectAllDevices();
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

      
    }
}
