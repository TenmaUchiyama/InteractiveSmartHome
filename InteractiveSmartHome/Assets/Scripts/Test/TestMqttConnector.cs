using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.Network;
using UnityEngine;

public class TestMqttConnector : MonoBehaviour
{
    
    string topic1 = "test1";
    string topic2 = "test2";


  
    private void ProcessMessage2(string arg0)
    {
        Debug.Log($"<color=red>[MqttConnector] Received: {arg0} from {topic2}</color>");
    }

    private void ProcessMessage1(string arg0)
    {
        Debug.Log($"<color=red>[MqttConnector] Received: {arg0} from {topic1}</color>");
    }
}
