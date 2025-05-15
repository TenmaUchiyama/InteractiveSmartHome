using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpatialLLM.Experiment;
using SpatialLLM.Core;

namespace SpatialLLM.Network
{
    public class PromptLLMQueryRequest : Singleton<PromptLLMQueryRequest>
    {
        [SerializeField] private string host = "localhost";
        [SerializeField] private int port = 8800;
        [SerializeField] private bool speechRequired = true;
        [SerializeField] private string debugText = "";
        [SerializeField] private ExperimentManager experimentManager;

        public UnityEvent<string> OnReceiveResponseFromLLM;
        private bool _isRequesting = false;
        public bool IsRequesting => _isRequesting;

        private const string endpoint = "llm_agent";

        private void Start()
        {
            if (speechRequired)
            {
                SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
            }
        }

        

        public void SendQueryForDebug(string text)
        {
            string useText = string.IsNullOrEmpty(debugText) ? text : debugText;
            OnVoiceRecognized(useText);
        }

        private async void OnVoiceRecognized(string recognizedText)
        {
            await SendQuery(recognizedText);
        }

        public async Task SendQuery(string userMessage, string taskId = "test_id", string promptId = "0")
        {
            Debug.Log($"<color=yellow>Sending Query: {userMessage}, {taskId}</color>");
            string url = $"http://{host}:{port}/{endpoint}";

            var data = new
            {
                llm_message = userMessage,
                task_id = taskId,
                prompt_id = promptId
            };

            string jsonData = JsonConvert.SerializeObject(data);
            _isRequesting = true;

            experimentManager?.StartLLMResponse();
            await PostRequestAsync(url, jsonData);
            experimentManager?.StopLLMResponse(userMessage);

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
