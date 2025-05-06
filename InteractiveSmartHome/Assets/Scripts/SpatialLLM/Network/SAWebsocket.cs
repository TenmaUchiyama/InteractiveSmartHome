using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
public class SAWebsocket : MonoBehaviour
{

    private WebSocketServer wss;


    void Awake()
    {
        wss = new WebSocketServer("ws://localhost:9876");
        wss.AddWebSocketService<EchoService>("/"); 
        wss.Start(); 


        Debug.Log("WebSocket Server started on ws://localhost:9876");
    }

    [ContextMenu("SendTest")]
public void SendTest()
{
    SendData("TEST");
}
     public void SendData(string data )
    {
        // 1対1なので、接続中のクライアントが1件だけだと仮定してBroadcastでもOK
        var service = wss.WebSocketServices["/"];
        service.Sessions.Broadcast(data);
        Debug.Log($"[WebSocket] Sent to client: {data}");
    }

    public class EchoService: WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Debug.Log("Received:" + e.Data);
            base.Send(e.Data); 
        }
    }
}
