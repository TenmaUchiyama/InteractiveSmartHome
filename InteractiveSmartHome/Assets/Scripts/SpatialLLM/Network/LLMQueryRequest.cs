using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Unity.VisualScripting;
using TMPro;
using SpatialLLM.Core;
using System;
using System.Text;
using Meta.WitAi.Json;
using UnityEngine.Events;




namespace SpatialLLM.Network
{
public class LLMQueryRequest : Singleton<LLMQueryRequest>
{


    private bool _isRequesting = false; 
    public bool IsRequesting => _isRequesting;
    // 任意のコマンドを指定
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 8800;

    [SerializeField] public bool speechRequired = true;


    public UnityEvent<string> OnReceiveResponseFromLLM;


    private void Start() {
        if(speechRequired) SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
    }

    private void OnVoiceRecognized(string recognizedText)
    {
        SendQuery(recognizedText);
    }

  

 public void SendQuery(string sending_text)
    {
        Debug.Log($"<color=yellow>Sending Query: {sending_text}</color>");
        string url = $"http://{host}:{port}/llm_agent";

        var data = new  { llm_message = sending_text };
        string jsonData = JsonConvert.SerializeObject(data);
        _isRequesting = true;
        StartCoroutine(PostRequest(url, jsonData));
    }

    // POSTリクエストを送信するコルーチン
    IEnumerator PostRequest(string url, string json)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // リクエストを送信し、レスポンスを待機
            yield return webRequest.SendWebRequest();

            // エラーチェック
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                OnReceiveResponseFromLLM.Invoke(webRequest.error);

            }
            else
            {
                // レスポンスデータを取得
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"Response: {responseText}");
                
                // 必要に応じてレスポンスを処理
                // 例: JSONデータのパースなど
                _isRequesting = false;
                OnReceiveResponseFromLLM.Invoke(responseText);
            }
        }
    }
}
}