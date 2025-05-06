using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using SpatialLLM.Core;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
using static SpatialLLM.Network.NetworkDataType;
using static SpatialLLM.Network.NetworkDataType.LLMServerDataUtil;




namespace SpatialLLM.Network
{
public class SASocketClient : MonoBehaviour
{
  
private SocketConnectionManager socketManager;

    // Start is called before the first frame update
    void Start()
    {
        // SocketConnectionManagerのコンポーネントを取得
        socketManager = GetComponent<SocketConnectionManager>();

        if (socketManager == null)
        {
            Debug.LogError("SocketConnectionManagerコンポーネントが見つかりません！");
            return;
        }

        // WebSocketサーバーに接続
        // socketManager.websocketConnector.Connect();
        
        // // メッセージを送信し、レスポンスを待機
        // StartCoroutine(SendAndReceiveMessage());
    }
        private void OnDisconnected()
        {
            throw new NotImplementedException();
        }

        private void OnConnected()
        {
          
        }

        private async void OnMessageReceived(string arg0)
        {
         
            // if(functionType == FunctionType.Direction)
            // {
            //     // DirectionUtil.Direction direction = DirectionUtil.GetDirection(request.args[0]);
            //     // List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(direction);
            //     // List<DevicePositionalData> devicePositionalData = devices.Select(device => device.GetDebugDeviceData()).ToList();
            //     // devicePositionalData.Sort((a, b) => a.distance_from_user.CompareTo(b.distance_from_user));

            //     // string json = JsonConvert.SerializeObject(devicePositionalData);
            //     // SendingLLMDataType sendData = new SendingLLMDataType(request.request_id , LLMServerAction.RespondDeviceData, json);
            //     // await SendSocketMessageAsync(sendData);
            // }
        }
void Update()
    {
    
    }


   
}


}