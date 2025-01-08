using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpatialLLM.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;



namespace SpatialLLM.Network
{
public class PointingQueryRequest :Singleton<PointingQueryRequest>
{
     private bool _isRequesting = false; 
    public bool IsRequesting => _isRequesting;
    // 任意のコマンドを指定
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 8800;

    [SerializeField] public bool speechRequired = true;


    public UnityEvent<string> OnReceiveResponseFromLLM;


    private void Start() {
  
    }


  

  public async Task SendQuery(string sending_text)
    {
        Debug.Log($"<color=yellow>Sending Query: {sending_text}</color>");
        string url = $"http://{host}:{port}/pointing_agent";

        // JSONデータの生成
        var data = new { llm_message = sending_text };
        string jsonData = JsonConvert.SerializeObject(data);

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