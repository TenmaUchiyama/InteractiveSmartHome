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
        private string currentState = "preparation";
        private string latestLLMOutput = "[No output]";
        private string recognizedText = "";

        protected override void Start()
        {
            base.Start();

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

        private void OnVoiceRecognized(string arg0)
        {
            recognizedText = arg0;
            Debug.Log($"<color=green>Recognized Word: {recognizedText}</color>");
            saUIManager.SetRecognizedTxt(recognizedText);

            if (!saUIManager.IsRecognizedWordEmplty())
            {
                recognizedText = saUIManager.GetRecognizedWord();
                saUIManager.SetInstructionText("Press Y to send to Agent");
                currentState = "recorded";
            }
        }

        protected override void Update()
        {
            base.Update();
            if (!isStarted) return;
            

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

            if (OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y))
            {
                bool isGripped = OVRInput.Get(OVRInput.RawButton.LHandTrigger) || Input.GetKey(KeyCode.G);

                if (isGripped)
                    currentState = "done";


                OperationProceed();
            }

            if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
            {
                experimentManager.BackToShowDevice();
            }
        }

        private async void OperationProceed()
        {
            Debug.Log($"<color=green>PointerExecutor State: {currentState}</color>");

            switch (currentState)
            {
                case "preparation":
                    SADeviceRef.Instance.ClearAllDeviceOperation();
                    saUIManager.SetInstructionText("Point and Press Y to send");
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

                            await PointingQueryRequest.Instance.SendQuery(recognizedText, taskId, this.experimentTask.NextGuid, pointedDevices);
                        }
                        else
                        {
                            var pointedIds = new List<DeviceLabelData>();

                            foreach (var d in pointed)
                            {
                                pointedIds.Add(new DeviceLabelData() { id = d.GetDeviceID(), name = d.GetDBDeviceData().device_name });
                            }


                            await PointingQueryRequest.Instance.SendQuery(recognizedText, taskId, this.experimentTask.NextGuid, pointedIds);
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
                        "[Pointing Input]",
                        elapsedTime.ToString(),
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
                    currentState = "preparation";
                    break;

                case "done":
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
