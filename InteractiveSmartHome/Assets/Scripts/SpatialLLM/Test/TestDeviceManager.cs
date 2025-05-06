using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meta.WitAi.Json;
using MRFlow.Network;
using SpatialLLM.Device; 
using SpatialLLM.Type;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.iOS;

public class TestDeviceManager : Singleton<TestDeviceManager>
{
   [SerializeField] GameObject parentObject; 
   private List<SADevice> saDevices = new List<SADevice>();
    async void Start()
    {

        Debug.Log("<color=yellow>Starting TestDeviceManager</color>");


        MRMqttController.Instance.OnConnectionCompleted += async () => {
      var allChildren = parentObject.GetComponentsInChildren<SADevice>().ToList();
        saDevices = allChildren;



      
        List<DBDeviceData> dbDatas = allChildren.Select(data => data.GetDBDeviceData()).ToList();   

       await ActionServerController.Instance.AddDevices(dbDatas); 



       MRMqttController.Instance.OnConnectionCompleted += () => {
         MRMqttController.Instance.SubscribeDeviceTopic("Manager", "device/reset", (string data)=> {this.ResetAllColor();});
        };
    
       };

    }

    private async Task OnDestroy() {
        await ActionServerController.Instance.DeleteAllDevice();
    }

    public void ResetAllColor() 
    {
        // saDevices.ForEach(data => data.ResetColor()); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
