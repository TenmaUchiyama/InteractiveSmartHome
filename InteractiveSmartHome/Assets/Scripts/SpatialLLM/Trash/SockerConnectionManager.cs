using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using static SpatialLLM.Network.NetworkDataType.LLMServerDataUtil;

namespace SpatialLLM.Network
{
    public class SocketConnectionManager : MonoBehaviour
    {
        // private WebsocketConnector websocketConnector;

        // // リクエストIDとその完了ソースのマッピング
        // private Dictionary<string, TaskCompletionSource<ReceivedFunctionRequest>> pendingRequests = new Dictionary<string, TaskCompletionSource<ReceivedFunctionRequest>>();

        // private void Awake()
        // {
        //     websocketConnector = GetComponent<WebsocketConnector>();
        //     if (websocketConnector == null)
        //     {
        //         Debug.LogError("ConnectionManager requires a WebsocketConnector component.");
        //     }

        //     // イベントハンドラの登録
        //     websocketConnector.OnMessageReceivedEvent.AddListener(OnMessageReceived);
        //     websocketConnector.OnConnectedEvent.AddListener(OnConnected);
        //     websocketConnector.OnDisconnectedEvent.AddListener(OnDisconnected);
        // }

        // // 接続が確立されたときの処理
        // private void OnConnected()
        // {
        //     Debug.Log("[ConnectionManager] WebSocketに接続されました。");
        // }

        // // 切断されたときの処理
        // private void OnDisconnected()
        // {
        //     Debug.Log("[ConnectionManager] WebSocketが切断されました。");
        //     // 必要に応じて再接続のロジックを追加
        // }

        // // メッセージ受信時の処理
        // private void OnMessageReceived(string message)
        // {
        //     Debug.Log($"[ConnectionManager] 受信メッセージ: {message}");

        //     try
        //     {
        //         ReceivedFunctionRequest response = JsonConvert.DeserializeObject<ReceivedFunctionRequest>(message);
        //         if (response != null && !string.IsNullOrEmpty(response.request_id))
        //         {
        //             if (pendingRequests.TryGetValue(response.request_id, out var tcs))
        //             {
        //                 tcs.SetResult(response);
        //                 pendingRequests.Remove(response.request_id);
        //             }
        //             else
        //             {
        //                 Debug.LogWarning($"[ConnectionManager] 未知のrequest_id: {response.request_id}");
        //             }
        //         }
        //         else
        //         {
        //             Debug.LogWarning("[ConnectionManager] レスポンスにrequest_idが含まれていません。");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError($"[ConnectionManager] メッセージのパース中にエラーが発生しました: {ex.Message}");
        //     }
        // }

        /// <summary>
        /// サーバーにリクエストを送信し、レスポンスを待機します。
        /// </summary>
        /// <param name="action">アクションの種類</param>
        /// <param name="body">アクションに関連するデータ</param>
        /// <returns>レスポンスメッセージ</returns>
        // public async Task<ReceivedFunctionRequest> SendRequestAsync(LLMServerAction action, string body)
        // {
        //     if (websocketConnector == null)
        //     {
        //         throw new InvalidOperationException("WebsocketConnectorが見つかりません。");
        //     }

        //     // リクエストIDを生成
        //     string requestId = Guid.NewGuid().ToString();

        //     // 送信するメッセージを作成
        //     SendingLLMDataType sendingMessage = new SendingLLMDataType(requestId, action, body);

        //     // TaskCompletionSourceを作成し、pendingRequestsに追加
        //     var tcs = new TaskCompletionSource<ReceivedFunctionRequest>();
        //     pendingRequests[requestId] = tcs;

        //     // メッセージを送信
        //     await websocketConnector.SendSocketMessageAsync(sendingMessage);

        //     // タスクを待機（タイムアウトを設定することを推奨）
        //     return await tcs.Task;
        // }

    }
}
