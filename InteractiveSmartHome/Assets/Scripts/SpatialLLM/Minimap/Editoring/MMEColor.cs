using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpatialLLM.Network.NetworkDataType;

public class MMEColor : MonoBehaviour, IMinimapEditor
{
    [SerializeField] private Image previewColor; 
    [SerializeField] private Image R; 
    [SerializeField] private Image G;
    [SerializeField] private Image B;

    [SerializeField] Slider sliderR;
    [SerializeField] Slider sliderG;
    [SerializeField] Slider sliderB;

    [SerializeField] private TextMeshProUGUI rText; 
    [SerializeField] private TextMeshProUGUI gText;
    [SerializeField] private TextMeshProUGUI bText;
    

    Color color = new Color(0, 0, 0);

//     private void Start() {
        

//       sliderR.onValueChanged.AddListener((float value) => {
//     Debug.Log("R: " + value);
//     color.r = value / 255f;  // 0-255から0-1に変換
//     rText.text = ((int)value).ToString();
//     SetColor(color);
// });

// sliderG.onValueChanged.AddListener((float value) => {
//     Debug.Log("G: " + value);
//     color.g = value / 255f;  // 0-255から0-1に変換
//     gText.text = ((int)value).ToString();
//     SetColor(color);
// });

// sliderB.onValueChanged.AddListener((float value) => {
//     Debug.Log("B: " + value);
//     color.b = value / 255f;  // 0-255から0-1に変換
//     bText.text = ((int)value).ToString();
//     SetColor(color);
// });

//     }



    private void SetColor(Color colorData)
    {
        previewColor.color = colorData;
        R.color = new Color(colorData.r, 0, 0);
        G.color = new Color(0, colorData.g, 0);
        B.color = new Color(0, 0, colorData.b);
    }

    public void OnUIValueChanged(SADevice saDevice)
    {

        OperatingDeviceData operatingDeviceData = saDevice.GetCurrentOperateData();
        ColorData colorData = operatingDeviceData.color;
        previewColor.color = new Color(colorData.r / 255f, colorData.g / 255f, colorData.b / 255f);
        R.color = new Color(colorData.r / 255f, 0, 0);
        G.color = new Color(0, colorData.g / 255f, 0);
        B.color = new Color(0, 0, colorData.b / 255f);

        sliderR.value = colorData.r;
        sliderG.value = colorData.g;
        sliderB.value = colorData.b;

        sliderR.onValueChanged.AddListener((float value) => {
            Debug.Log("R: " + value);
            colorData.r = (int)value;
            operatingDeviceData.color = colorData;
            saDevice.OperateDevice(operatingDeviceData);
        });

        sliderG.onValueChanged.AddListener((float value) => {
            Debug.Log("G: " + value);
            colorData.g = (int)value;
            operatingDeviceData.color = colorData;
            saDevice.OperateDevice(operatingDeviceData);
        });

        sliderB.onValueChanged.AddListener((float value) => {
            Debug.Log("B: " + value);
            colorData.b = (int)value;
            operatingDeviceData.color = colorData;
            saDevice.OperateDevice(operatingDeviceData);
        });
    }


//    [SerializeField] private FlexibleColorPicker colorPicker;

//     public void OnUIValueChanged(SADevice saDevice)
//     {
//     OperatingDeviceData operatingDeviceData = saDevice.GetCurrentOperateData();
//     colorPicker.onColorChange.AddListener((Color color) => {

//         Debug.Log($"Color Changed: {color}");
//         ColorData colorData = new ColorData(); 
//         colorData.r = (int)(color.r * 255);
//         colorData.g = (int)(color.g * 255);
//         colorData.b = (int)(color.b * 255);
//         operatingDeviceData.color = colorData;  
//         saDevice.OperateDevice(operatingDeviceData);
//     });
//     }

}
