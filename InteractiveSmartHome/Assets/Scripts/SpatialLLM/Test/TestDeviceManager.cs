using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meta.WitAi.Json;
using MRFlow.Network;
using SpatialLLM.Core;
using SpatialLLM.Type;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.iOS;

public class TestDeviceManager : Singleton<TestDeviceManager>
{
   [SerializeField] GameObject parentObject; 

   private List<TestDevice> testDevices = new List<TestDevice>();
    async void Start()
    {

      // await ActionServerController.Instance.DeleteAllDevice(); 

        Debug.Log("<color=yellow>Starting TestDeviceManager</color>");
      var allChildren = parentObject.GetComponentsInChildren<TestDevice>().ToList();
        testDevices = allChildren;
        
        List<DBDeviceData> dbDatas = allChildren.Select(data => data.GetDBDeviceData()).ToList();   


       await ActionServerController.Instance.AddDevices(dbDatas); 



       MRMqttController.Instance.OnConnectionCompleted += () => {
         MRMqttController.Instance.SubscribeDeviceTopic("Manager", "device/reset", (string data)=> {this.ResetAllColor();});
       };

    }

  // private async Task OnDestroy() {
  //     await ActionServerController.Instance.DeleteAllDevice();
  // }

    public void ResetAllColor() 
    {
        testDevices.ForEach(data => data.ResetColor()); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
