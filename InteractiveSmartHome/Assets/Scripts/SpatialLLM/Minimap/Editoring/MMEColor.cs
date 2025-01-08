using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;

public class MMEColor : MonoBehaviour, IMinimapEditor
{
   [SerializeField] private FlexibleColorPicker colorPicker;

    public void OnUIValueChanged(SADevice saDevice)
    {
    OperatingDeviceData operatingDeviceData = saDevice.GetCurrentOperateData();
    colorPicker.onColorChange.AddListener((Color color) => {

        Debug.Log($"Color Changed: {color}");
        ColorData colorData = new ColorData(); 
        colorData.r = (int)(color.r * 255);
        colorData.g = (int)(color.g * 255);
        colorData.b = (int)(color.b * 255);
        operatingDeviceData.color = colorData;  
        saDevice.OperateDevice(operatingDeviceData);
    });
    }

}
