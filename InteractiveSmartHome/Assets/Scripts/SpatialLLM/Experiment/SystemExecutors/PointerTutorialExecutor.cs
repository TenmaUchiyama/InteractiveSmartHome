using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oculus.Interaction;
using SpatialLLM.Core;
using SpatialLLM.Network;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;

public class PointerTutorialExecutor : MonoBehaviour
 {
        [SerializeField] private RayInteractor rayInteractor;
        [SerializeField] private VRCircleSelector circleSelector;
        [SerializeField] private bool IS_SPATIAL_POINTING = false; // Spatial Pointingを有効にするかどうか
        [SerializeField] private SAUIManager saUIManager;
        [SerializeField] private Camera userCamera;
        private string currentState = "preparation";
        private string latestLLMOutput = "[No output]";
        private string recognizedText = "";

     void Start()
        {


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

      public void  Update()
    {




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

       
    }

        private async void OperationProceed()
        {
            Debug.Log($"<color=green>PointerExecutor State: {currentState}</color>");

            switch (currentState)
            {
                case "preparation":
                    SADeviceRef.Instance.ClearAllDeviceOperation();
                    saUIManager.SetInstructionText("Point and Press Y to send");
                   
                    break;

                case "recorded":
                    saUIManager.DisplaySendingLLM("[Pointed Devices]");
                    saUIManager.StartSendLLM();

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

                            await PointingQueryRequest.Instance.SendQuery(recognizedText, "0", "0", pointedDevices);
                        }
                        else
                        {
                            var pointedIds = new List<DeviceLabelData>();

                            foreach (var d in pointed)
                            {
                                pointedIds.Add(new DeviceLabelData() { id = d.GetDeviceID(), name = d.GetDBDeviceData().device_name });
                            }


                            await PointingQueryRequest.Instance.SendQuery(recognizedText, "0", "0", pointedIds);
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

              

                    saUIManager.FinishLoadingAndDisplayResponse(latestLLMOutput);
                    currentState = "checking";
                  
                    break;

                case "checking":
                    saUIManager.SetInstructionText("Grip+Y to confirm, or press Y to retry");
                    currentState = "preparation";
                    break;

                case "done":
                  
                    saUIManager.ClearRecognizedWord();
         
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
