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
using SpatialLLM.Experiment;




namespace SpatialLLM.Network
{



    public class LLMQueryRequest : Singleton<LLMQueryRequest>
    {

        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 8800;
        [SerializeField] private bool speechRequired = true;
        [SerializeField] private string debugText = "";
        [SerializeField] private ExperimentManager experimentManager;

        public UnityEvent<string> OnReceiveResponseFromLLM;
        private bool _isRequesting = false;
        public bool IsRequesting => _isRequesting;

        public async Task SendQuery(string path, string jsonData)
        {
            string url = $"http://{host}:{port}/{path}";
            Debug.Log($"<color=yellow>Sending Query to {path}: {jsonData}</color>");

            await PostRequestAsync(url, jsonData);

            _isRequesting = false;
        }

    private async Task PostRequestAsync(string url, string jsonData)
{
    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
        try
        {
            request.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"<color=cyan>Label Response: {request.downloadHandler.text}</color>");
                OnReceiveResponseFromLLM?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"<color=red>Label Error: {request.error}</color>");
                throw new System.Exception(request.error); // ← 明示的に例外化も可
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"<color=red>PostRequestAsync例外: {ex.Message}</color>");
            throw;
        }
    }
}


    }
}