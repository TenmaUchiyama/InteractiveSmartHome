using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Experiment;
using UnityEngine;

public class PointingDevices : MonoBehaviour
{

    [SerializeField] ExperimentManager experimentManager;
    [SerializeField] PointerSystemExecutor pointerSystemExecutor;


    private List<SADevice> allDevices;

    [SerializeField] private List<DeviceColorPair> selectedDevices;

    void Start()
    {
        allDevices = SADeviceRef.Instance.GetAllDevices();
    }

 public void GetSelected()
{
    if (experimentManager == null)
    {
        Debug.LogError("ExperimentManager がアタッチされていません！");
        return;
    }

    // ExperimentManager から現在の DeviceArrangeData を取得
    DeviceArrangeData currentArrange = experimentManager.GetCurrentArrangeData();
    if (currentArrange == null || currentArrange.devices == null)
    {
        Debug.LogError("ExperimentManager から有効な DeviceArrangeData を取得できませんでした！");
        return;
    }

    Debug.Log("<color=yellow>Pinched</color>");

    // 現在のデバイスリスト（DeviceColorPair のリスト）
    List<DeviceColorPair> arrangedDevices = currentArrange.devices;

    foreach (var device in allDevices)
    {
        if (device.IsDeviceSelected()) // デバイスが選択されている場合
        {
            // DeviceArrangeData の devices から該当する DeviceColorPair を探す
            DeviceColorPair foundPair = arrangedDevices.Find(pair => pair.device == device);
            
            if (foundPair != null)
            {
                // 既に selectedDevices にあるか確認
                bool exists = selectedDevices.Exists(pair => pair.device == foundPair.device);
                if (!exists)
                {
                    // ない場合は追加
                    selectedDevices.Add(new DeviceColorPair { device = foundPair.device, color = foundPair.color });
                    Debug.Log($"<color=yellow>Added device from ArrangeData: {foundPair.device.name} ({foundPair.color})</color>");
                }
            }else{
               selectedDevices.Add(new DeviceColorPair { device = device, color= DeviceColor.White  });
            }

           
        }
    }



       selectedDevices.RemoveAll(pair => !pair.device.IsDeviceSelected());
}



[ContextMenu("Pointing Done")]
// public void PointingDone() {
//     this.pointerSystemExecutor.PointSystemDone();

// }

 // 🎯 **Inspector にボタンを追加し、押されたときに `TurnOnWithColor()` を呼び出す**
    [ContextMenu("Turn On Selected Devices")]
    public void TurnOnSelectedDevices()
    {
        if (selectedDevices.Count == 0)
        {
            Debug.LogWarning("No devices selected!");
            return;
        }

        foreach (DeviceColorPair pair in selectedDevices)
        {

            DrawOnHover drawOnHover = pair.device.gameObject.GetComponent<DrawOnHover>();
            if (pair.device != null)
            {
                pair.device.TurnOnWithColor(pair.GetUnityColor());
                if (drawOnHover != null)
                {
                    drawOnHover.VisualizeTargetDevice(Color.blue); 
                }
                
            }
        }

          
          selectedDevices.RemoveAll(pair => pair.device.IsDeviceSelected());

          
    }
    
}

