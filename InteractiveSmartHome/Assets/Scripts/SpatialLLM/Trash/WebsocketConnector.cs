using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Meta.Net.NativeWebSocket;
using UnityEngine.Events;
using static SpatialLLM.Network.NetworkDataType;
using Meta.WitAi.Json;
using static SpatialLLM.Network.NetworkDataType.LLMServerDataUtil;


namespace SpatialLLM.Network
{
    public class WebsocketConnector : MonoBehaviour
    {
    private WebSocket websocket;

        public UnityEvent<string> OnMessageReceivedEvent;
        public UnityEvent OnConnectedEvent;

        public UnityEvent OnDisconnectedEvent;

        
        [SerializeField]
        private string serverUrl = "ws://localhost:7070/ws"; // WebSocketサーバーのURL

        // **サーバーに接続する**
        public async void Connect()
        {
            websocket = new WebSocket(serverUrl);

            websocket.OnOpen += () =>
            {
                Debug.Log("[WebSocketManager] サーバーに接続しました");
                OnConnectedEvent?.Invoke();
            };

            websocket.OnMessage += (bytes,offset,length) =>
            {
                // メッセージをUTF-8文字列に変換
                string message = Encoding.UTF8.GetString(bytes);
                Debug.Log($"[WebSocketManager] メッセージ受信: {message}");
                OnMessageReceivedEvent?.Invoke(message); // 受信したメッセージを通知
            };
    
            websocket.OnClose += (code) =>
            {
                Debug.Log("[WebSocketManager] サーバーから切断されました");
                OnDisconnectedEvent?.Invoke();
            };

            await websocket.Connect();
        }

        // **メッセージをサーバーに送信する**
          public async Task SendSocketMessageAsync(string message)
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                string json = JsonConvert.SerializeObject(message);
                await websocket.SendText(json);
                Debug.Log($"[WebSocketManager] 送信メッセージ: {message}");
            }
            else
            {
                Debug.LogWarning("[WebSocketManager] WebSocketが開いていません");
            };
        }

        // **アプリケーション終了時にWebSocketを閉じる**
        private async void OnApplicationQuit()
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                await websocket.Close();
            }
        }
    }
}

