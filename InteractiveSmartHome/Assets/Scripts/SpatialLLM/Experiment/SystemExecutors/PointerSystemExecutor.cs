
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
using Unity.VisualScripting;

namespace SpatialLLM.Experiment
{
    public class PointerSystemExecutor : SystemExecutor
    {
        [SerializeField] private RayInteractor rayInteractor;
        [SerializeField] private VRCircleSelector circleSelector;
        [SerializeField] private bool IS_SPATIAL_POINTING = false; // Spatial Pointingを有効にするかどうか
        [SerializeField] private bool isEvaluation = false;
        [SerializeField] private Camera userCamera;
        private string currentState = "pointing";
        private string latestLLMOutput = "[No output]";
        private string recognizedWord = "";
        private string lastRecognizedWord = "";
private bool gripHeld = false;

        
    protected override void OnLeftThumbstickLeftFlick()
    {
        base.OnLeftThumbstickLeftFlick();

        // recognizedWordの一番後ろの文字を消す
        if (recognizedWord.Length > 0)
        {
            recognizedWord = recognizedWord.Substring(0, recognizedWord.Length - 1);
            lastRecognizedWord = recognizedWord; // 最後の認識単語を更新
            saUIManager.SetRecognizedTxt(recognizedWord);
        }
    }
    
       private void ResetRecognizedWord()
    {
        recognizedWord = "";
        saUIManager.ClearRecognizedWord(); // もし存在しなければ SetRecognizedTxt("") などで代用
        saUIManager.SetInstructionText("Press Y to start recording");
    }


private void StateUIHelper()
{
        uiButtonHelper.CloseAllLabels();
    // 状態に応じてUI更新
            switch (currentState)
            {



                case "pointing":
                    // 右手操作UI
                    PointingHelper(true, false); // Aボタン未押下（切替表示）
                                                 // 左手トリガーに音声認識表示
                    uiButtonHelper.SetLabel(UIButtonLabelType.LeftTrigger, "音声録音", Color.black, Color.gray);
                    
                    uiButtonHelper.SetLabel(UIButtonLabelType.LeftThumbstick, "もう一度確認", Color.black, Color.yellow);
                    break;

                case "recorded":
                    PointingHelper(false, false); // Pointing系は非表示
                                                  // 音声送信・取り消し・戻るの表示（左手用）
                    uiButtonHelper.SetLabel(UIButtonLabelType.LeftTrigger, "音声録音", Color.black, Color.gray);
                    uiButtonHelper.SetLabel(UIButtonLabelType.Y, "送信", Color.black, Color.green);
                    uiButtonHelper.SetLabel(UIButtonLabelType.X, "取り消し", Color.black, Color.red);
                    uiButtonHelper.SetLabel(UIButtonLabelType.LeftThumbstick, "もう一度確認", Color.black, Color.yellow);
                    break;

                case "received":
                    //     // 一時的に「確認待ち」状態、何も表示しないで良いかもしれないが必要なら記述可能
                    // uiButtonHelper.CloseAllLabels();
                    // saUIManager.SetInstructionText("Press Y to confirm");
                    // uiButtonHelper.SetLabel(UIButtonLabelType.Y, "確認", Color.black, Color.green);
                    break;

                case "checking":
                    uiButtonHelper.SetLabel(UIButtonLabelType.LeftGrip, "押して操作", Color.black, Color.gray);
                    if (gripHeld)
                    {
                        uiButtonHelper.SetLabel(UIButtonLabelType.Y, "次へ進む", Color.black, Color.green);
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
                    uiButtonHelper.CloseAllLabels();
                    break;
            }
}


        private void PointingHelper(bool isShow, bool isAPressed = true)
        {


            if (!isShow)
            {
                // ラベルを非表示にする
                uiButtonHelper.SetLabelVisible(UIButtonLabelType.A, false);
                uiButtonHelper.SetLabelVisible(UIButtonLabelType.RightTrigger, false);
                return;
            }

            // Aボタンが押されているかどうかでラベルを切り替える
            if (isAPressed)
            {
                // A押してる間：Trigger = 単一選択
                uiButtonHelper.SetLabel(
                    UIButtonLabelType.RightTrigger,
                    "単一選択",
                    Color.black,
                    Color.green
                );

                // Aのラベルは不要・または非表示にしてもよい
                uiButtonHelper.SetLabelVisible(UIButtonLabelType.A, false);
            }
            else
            {
                // A押してないとき：A = レーザーにする、Trigger = 範囲選択にする
                uiButtonHelper.SetLabel(
                    UIButtonLabelType.A,
                    "レーザーにする",
                    Color.black,
                    Color.gray
                );

                uiButtonHelper.SetLabel(
                    UIButtonLabelType.RightTrigger,
                    "範囲選択",
                    Color.black,
                    Color.blue
                );

                uiButtonHelper.SetLabelVisible(UIButtonLabelType.A, true);
            }
            ;
        }






        protected override void Start()
        {
            base.Start();

            this.onBeginOperation.AddListener(() =>
            {
                PointingHelper(true, false);
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
            OperationProceed();
        }

        private void OnVoiceRecognized(string recognizedText)
        {

            saUIManager.SetInstructionText("Press Y to send to Agent");
            
            // 上書きから追記に変更 ↓↓↓
            recognizedWord += recognizedText + " "; // スペースで区切る
            lastRecognizedWord = recognizedWord; // 最後の認識単語を更新
            saUIManager.SetRecognizedTxt(recognizedWord);
            currentState = "recorded";
            Debug.Log($"[LabelExecutor] 音声認識結果を更新: {recognizedWord}");
            StateUIHelper();
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
                 PointingHelper(true, true);
            }
            if (OVRInput.GetUp(OVRInput.RawButton.A))
            {
                Debug.Log("[PointerSystemExecutor] Aボタン離す：Ray Interactorを無効化");

                rayInteractor.gameObject.SetActive(false);
                circleSelector.SetSelectionStarted(true);
                PointingHelper(true,false);

            }




            if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
            {
                gripHeld = true;
               StateUIHelper();
            }
            if (OVRInput.GetUp(OVRInput.RawButton.LHandTrigger))
            {
                gripHeld = false;
                StateUIHelper();
            }

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
                        Debug.Log("Y pressed without Grip - no action");
                    }
                }
                else
                {
                    OperationProceed();
                    
                }
            }

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
                currentState = "pointing";
                experimentManager.BackToShowDevice();
            
        }
        }
        
        private async void OperationProceed()
        {

            StateUIHelper();
            Debug.Log($"<color=green>PointerExecutor State: {currentState}</color>");


            switch (currentState)
            {

                // 初期状態
                // ここで音声認識を開始する
                case "pointing":
                    PointingHelper(true, false);
                    SADeviceRef.Instance.ClearAllDeviceOperation();
                    saUIManager.SetInstructionText("Point and Press Y to send");
                    elapsedTime = 0f;
                    timerStarted = true;
                    break;

                case "recorded":
                    PointingHelper(false, false);
                    saUIManager.DisplaySendingLLM("[Pointed Devices]");
                    saUIManager.StartSendLLM();

                    string taskId = experimentManager.GetCurrentTaskId();
                    var pointed = SADeviceRef.Instance.GetAllSelectedDevices();

                    if (!LLMQueryRequest.Instance.IsRequesting)
                    {
                        
                        if(recognizedWord == "")
                        {
                            Debug.LogWarning("No recognized word, skipping LLM query.");
                            return;
                        }

                        if (isEvaluation)
                        {
                            var pointedDevices = new List<DeviceSpatialData>();
                            foreach (var d in pointed)
                            {
                                pointedDevices.Add(d.GetDevicePositionalRelativeToUser(userCamera.transform));
                            }

                            await PointingQueryRequest.Instance.SendQueryMulti(recognizedWord, taskId, this.experimentTask.NextGuid, pointedDevices);
    

                            break;
                        }

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
                       lastRecognizedWord,
                       this.elapsedTime.ToString(),
                       operatedIds
                   );


                    this.wordLogger.AddOrUpdateTaskData(this.experimentTask.GetExperimentTaskData());

                    saUIManager.FinishLoadingAndDisplayResponse(latestLLMOutput);
                    currentState = "checking";
                    timerStarted = false;
                    
                    elapsedTime = 0f;
                    OperationProceed();
                    break;

                case "checking":
                    saUIManager.SetInstructionText("Grip+Y to confirm, or press Y to retry");
            
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