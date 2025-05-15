using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialLLM.Core;
using SpatialLLM.Type;
using SpatialLLM.Network;
using MRFlow.Network;
using System.Threading.Tasks;

public class TestSearchDevice : MonoBehaviour
{

   
   ActionServerConnector actionServerConnector; 


   private async void Start() {
      // actionServerConnector = GetComponent<ActionServerConnector>();


      // DBDeviceData dBDeviceData = new DBDeviceData(
      //    device_id :"test_id", 
      //    device_type: "test",
      //    device_name : "test device",
      //    anchor_id: "test",
      //    connector_type : "switchbot", 
      //    connector_topic: "9C9E6EDCDB72",
      //    description: ""
      // ); 

      // Debug.Log("<color=yellow>here it is</color>");
      // await actionServerConnector.AddDevices(new List<DBDeviceData>{dBDeviceData});
   }


}
