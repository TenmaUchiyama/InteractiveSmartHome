using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpatialLLM.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static SpatialLLM.Network.NetworkDataType;



namespace SpatialLLM.Network
{
public class PointingQueryRequest :Singleton<PointingQueryRequest>
{
     private bool _isRequesting = false; 
    public bool IsRequesting => _isRequesting;
    // 任意のコマンドを指定
    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private int port = 8800;

    [SerializeField] public bool speechRequired = true;
        [SerializeField] public bool isTutorial = false;

    public UnityEvent<string> OnReceiveResponseFromLLM;


    private void Start() {
  
    }

        public async Task SendQuery(string sending_text, string task_id, string attempt_id, List<DeviceSpatialData> deviceSpatialData)
        {
            string jsonData = JsonConvert.SerializeObject(new
            {
                 llm_message = sending_text,
                task_id = task_id,
                attempt_id = attempt_id,
                pointed_devices = deviceSpatialData
            });

            Debug.Log($"<color=yellow>Sending Pointing Query SPATIAL: {jsonData}</color>");
            

            if (isTutorial)
            {
               
            await _SendQuery("pointing_spatial_tutorial", jsonData);
            }
            else
            {
                await _SendQuery("pointing_spatial", jsonData);
            }
        }

        public async Task SendQuery(string sending_text, string task_id, string attempt_id, List<DeviceLabelData> deviceLabelData)
        {
            string jsonData = JsonConvert.SerializeObject(new
            {
                llm_message = sending_text,
                task_id = task_id,
                attempt_id = attempt_id,
                pointed_devices = deviceLabelData
            });

            Debug.Log($"<color=yellow>Sending Pointing Query ONLY_POINTING: {jsonData}</color>");
            await _SendQuery("pointing", jsonData);
        }
        

        public async Task SendQueryMulti(string sending_text, string task_id, string attempt_id, List<DeviceSpatialData> deviceSpatialData)
        {
            string jsonData = JsonConvert.SerializeObject(new
            {
                 llm_message = sending_text,
                task_id = task_id,
                attempt_id = attempt_id,
                pointed_devices = deviceSpatialData
            });

            Debug.Log($"<color=yellow>Sending Pointing Query SPATIAL: {jsonData}</color>");
            

            if (isTutorial)
            {
               
            await _SendQuery("pointing_spatial_tutorial", jsonData);
            }
            else
            {
                await _SendQuery("pointing_spatial_multi", jsonData);
            }
        }

  private async Task _SendQuery(string path, string jsonData)
        {
            Debug.Log($"<color=yellow>Sending Query: {jsonData}</color>");
            string url = $"http://{host}:{port}/{path}";


            // 非同期POSTリクエストの送信
            _isRequesting = true;
            await PostRequestAsync(url, jsonData);
            _isRequesting = false;
        }

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