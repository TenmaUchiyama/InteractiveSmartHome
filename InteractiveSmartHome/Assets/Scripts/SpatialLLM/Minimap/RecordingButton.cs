
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Minimap;
using SpatialLLM.Network;
using SpatialLLM.Type;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static SpatialLLM.Network.NetworkDataType;

public class RecordingButton : MonoBehaviour
{
    [SerializeField] private Image icon; 
    
    [SerializeField] private Sprite microphoneSprite;
    [SerializeField] private Sprite recordingSprite;

    [SerializeField] private Button button;

    [SerializeField] private Transform minimapParent;


    private bool isRecording = false;




    private void Start() {
        icon.sprite = microphoneSprite;

        SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
        button.onClick.AddListener(()=>{
            Debug.Log("Recording Button Clicked");
            
           if(isRecording)
           {
               SASpeechRecognizer.Instance.DeactivateVoice();   
               icon.sprite = microphoneSprite;
               isRecording = false;
           }
           else
           {

               SASpeechRecognizer.Instance.ActivateVoice();
               icon.sprite = recordingSprite;
               isRecording = true;
           }
        }); 
    }
    

    void OnDestroy()
    {
        SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
    }



    private async void OnVoiceRecognized(string payload)
    {
        Debug.Log($"<color=yellow>[RecordingButton] Payload: {payload}</color>");   
        LabelQueryDataType labelQueryDataType = new LabelQueryDataType();
        labelQueryDataType.user_message = payload;
        List<DeviceLabel> deviceLabels = new List<DeviceLabel>();
       foreach(Transform child in minimapParent)
       {
          MiniMapIcon icon = child.GetComponent<MiniMapIcon>();
          if(icon == null || !icon.IsSelected()) continue;
            
           SADevice sadevice = icon.GetDevice();
           DBDeviceData deviceData = sadevice.GetDBDeviceData();
           DeviceLabel deviceLabel = new DeviceLabel();
           deviceLabel.id = deviceData.device_id;
           deviceLabel.name = deviceData.device_name;
           deviceLabel.type = deviceData.device_type;
        

            deviceLabels.Add(deviceLabel);


       }
       labelQueryDataType.devices = deviceLabels;

        Debug.Log($"<color=yellow>[RecordingButton] Selected Device Count: {deviceLabels.Count}</color>");

         JsonSerializerSettings settings = new JsonSerializerSettings(){
              ReferenceLoopHandling = ReferenceLoopHandling.Ignore
         };

         string json = JsonConvert.SerializeObject(labelQueryDataType, settings);
            Debug.Log($"<color=yellow>[RecordingButton] Send Query: {json}</color>");

        await LLMQueryRequest.Instance.SendQuery(json);

    }
}
