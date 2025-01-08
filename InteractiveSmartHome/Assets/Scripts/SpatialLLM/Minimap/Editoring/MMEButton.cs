using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using SpatialLLM.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SpatialLLM.Network.NetworkDataType;

public class MMEButton : MonoBehaviour, IMinimapEditor
{
    [SerializeField] Toggle toggle;


   

    public void OnUIValueChanged(SADevice saDevice)
    {
        OperatingDeviceData operatingDeviceData = saDevice.GetCurrentOperateData();

        toggle.onValueChanged.AddListener((bool value) => {
         
            operatingDeviceData.state = value; 
            Debug.Log("Button Value: " + value);
            saDevice.OperateDevice(operatingDeviceData);
        });
    }
}
