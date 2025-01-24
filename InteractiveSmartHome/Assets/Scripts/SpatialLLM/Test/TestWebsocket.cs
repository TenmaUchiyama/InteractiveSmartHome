using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Network;
using UnityEngine;
using Newtonsoft.Json;
public class TestWebsocket : WebsocketConnector
{
   
    private void Start() {
        Connect();
    }
    // // Update is called once per frame
    // void Update()
    // {
    //     // if (Input.GetKeyDown(KeyCode.Space))
    //     // {

    //     //     var jsonData = JsonConvert.SerializeObject(new { action = "echo", message = "Hello World" });
    //     //     // SendSocketMessage(jsonData);

    //     // }
    // }
}
