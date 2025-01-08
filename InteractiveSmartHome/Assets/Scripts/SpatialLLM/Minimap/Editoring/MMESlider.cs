using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using SpatialLLM.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SpatialLLM.Network.NetworkDataType;

public class MMESlider : MonoBehaviour, IMinimapEditor
{

    [SerializeField] private Slider slider;


    public void OnUIValueChanged(SADevice  saDevice)
    {

        OperatingDeviceData operatingDeviceData = saDevice.GetCurrentOperateData();
        slider.onValueChanged.AddListener((float value) => {
            operatingDeviceData.intensity = (int)value;
            saDevice.OperateDevice(operatingDeviceData);
        });
    }
}
