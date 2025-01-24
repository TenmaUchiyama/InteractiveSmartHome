using UnityEngine;
using UnityEngine.Networking;
using SpatialLLM.Core;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Collections.Generic;
using SpatialLLM.Device;
using static SpatialLLM.Network.NetworkDataType;
using System.Linq;
using Newtonsoft.Json;




namespace SpatialLLM.Network
{


public enum LLMQueryMode
{
    Spatial, 
    Pointing, 
    Label,
    Map,
    Multiple_Select,
    Multiple_Select_Pointing,
}


public class LLMQueryRequest : Singleton<LLMQueryRequest>
{


    private bool _isRequesting = false; 
    public bool IsRequesting => _isRequesting;
    // 任意のコマンドを指定
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 8800;
    [SerializeField] private LLMQueryMode queryMode = LLMQueryMode.Spatial;

    [SerializeField] public bool speechRequired = true;

    [SerializeField] private string debugText = "";


    public UnityEvent<string> OnReceiveResponseFromLLM;
    private Dictionary<LLMQueryMode, string> queryModeUrl = new Dictionary<LLMQueryMode, string>(){
    {LLMQueryMode.Spatial, "llm_agent"},
    {LLMQueryMode.Pointing, "pointing_agent"},
    {LLMQueryMode.Label, "label_agent"},
    
    {LLMQueryMode.Multiple_Select, "multiple_select_agent"},
    {LLMQueryMode.Multiple_Select_Pointing, "multiple_select_agent"},

};


    private void Start() {
        if(speechRequired) SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
    }


    public void SendQueryForDebug(string text)
    {

        text = debugText == "" ? text : debugText; 
        OnVoiceRecognized(text); 
    }


    private async void OnVoiceRecognized(string recognizedText)
    {
        switch(queryMode)
        {
            case LLMQueryMode.Spatial:
                await SendQuery(recognizedText);
            break;
            case LLMQueryMode.Label:
                List<SADevice> devices = SpatialAwarnessProvider.Instance.GetAllDevices(); 
                List<DeviceLabel> deviceLabels = devices.Select(device => {
                    DeviceLabel deviceLabel = new DeviceLabel(); 
                    deviceLabel.id = device.GetDBDeviceData().device_id;
                    deviceLabel.name = device.GetDBDeviceData().device_name; 
                    deviceLabel.type = device.GetDBDeviceData().device_type;

                    return deviceLabel;
                }).ToList(); 

                LabelQueryDataType labelQueryData = new LabelQueryDataType();
                labelQueryData.user_message = recognizedText;
                labelQueryData.devices = deviceLabels;

                JsonSerializerSettings settings = new JsonSerializerSettings(){
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string json = JsonConvert.SerializeObject(labelQueryData, settings);

                Debug.Log($"<color=yellow>Send Query: {json}</color>");
                await SendQuery(json);
            break; 
            case LLMQueryMode.Multiple_Select_Pointing:
                Debug.Log("================================================================");
                 List<SADevice> allDevices = SpatialAwarnessProvider.Instance.GetAllDevices(); 
                List<DeviceLabel> selectedDeviceLabel = allDevices
                .Where(device => device.IsDeviceSelected())
                .Select(device => {
                    DeviceLabel deviceLabel = new DeviceLabel(); 
                    deviceLabel.id = device.GetDBDeviceData().device_id;
                    deviceLabel.name = device.GetDBDeviceData().device_name; 
                    deviceLabel.type = device.GetDBDeviceData().device_type;

                    return deviceLabel;
                }).ToList(); 


                LabelQueryDataType multipleSelectQueryData = new LabelQueryDataType();
                multipleSelectQueryData.user_message = recognizedText;

                multipleSelectQueryData.devices = selectedDeviceLabel;

                JsonSerializerSettings multipleSelectSettings = new JsonSerializerSettings(){
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string multipleSelectJson = JsonConvert.SerializeObject(multipleSelectQueryData, multipleSelectSettings);

                Debug.Log($"<color=yellow>Send Query: {multipleSelectJson}</color>");
                await SendQuery(multipleSelectJson);
                

            break;
            default :
            break;
        }
    }

  

 public async Task SendQuery(string sending_text)
    {
        Debug.Log($"<color=yellow>Sending Query: {sending_text}</color>");
        string path = queryModeUrl[queryMode];
        string url = $"http://{host}:{port}/{path}";

        var data = new  { llm_message = sending_text };
        string jsonData = JsonConvert.SerializeObject(data);
        _isRequesting = true;
        await PostRequestAsync(url, jsonData);
        
        _isRequesting = false;
    }

    // POSTリクエストを送信するコルーチン
    private async Task PostRequestAsync(string url, string jsonData)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // リクエストヘッダーの設定
            request.SetRequestHeader("Content-Type", "application/json");

            // JSONデータをリクエストに追加
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // リクエストの送信と結果の取得
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"<color=green>Response: {request.downloadHandler.text}</color>");
                OnReceiveResponseFromLLM?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"<color=red>Error: {request.error}</color>");
            }
        }
    }
}
}