using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpatialLLM.Experiment;
using SpatialLLM.Core;

namespace SpatialLLM.Network
{
    public class LabelLLMQueryRequest : Singleton<LabelLLMQueryRequest>
    {
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 8800;
        [SerializeField] private bool speechRequired = true;
        [SerializeField] private string debugText = "";
        [SerializeField] private ExperimentManager experimentManager;

        public UnityEvent<string> OnReceiveResponseFromLLM;
        private bool _isRequesting = false;
        public bool IsRequesting => _isRequesting;

        private const string endpoint = "label";


        public async Task SendQueryForDebug(string text)
        {
            string useText = string.IsNullOrEmpty(debugText) ? text : debugText;
            var task_id = experimentManager ? experimentManager.GetCurrentTaskId().ToString() : "test_id";
            await SendQuery(useText, taskId: task_id);
        }


        public async Task SendQuery(string userMessage, string taskId = "test_id", string attemptId = "0")
        {
            Debug.Log($"<color=yellow>Sending Label Query: {userMessage}, {taskId}</color>");
            string url = $"http://{host}:{port}/{endpoint}";

            var data = new
            {
                llm_message = userMessage,
                task_id = taskId,
                attempt_id = attemptId
            };

            string jsonData = JsonConvert.SerializeObject(data);
            _isRequesting = true;

            await PostRequestAsync(url, jsonData);

            _isRequesting = false;
        }

        private async Task PostRequestAsync(string url, string jsonData)
        {
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
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
                }
            }
        }
    }
}
