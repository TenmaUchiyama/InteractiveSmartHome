using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine;
using SpatialLLM.Experiment;
using SpatialLLM.Device;

[CustomEditor(typeof(ExperimentManager))]
public class ExperimentManagerHelper : Editor
{

    [MenuItem("Tool/Turn On Selected Item - White %#w")]
    private static void TurnOnWhiteDevice()
    {
        TurnOnDevicesWithColor(Color.white);
    }

    [MenuItem("Tool/Turn On Selected Item - Blue %#q")]
    private static void TurnOnBlueDevice()
    {
        TurnOnDevicesWithColor(Color.blue);
    }

    [MenuItem("Tool/Turn On Selected Item - Red %#e")]
    private static void TurnOnRedDevice()
    {
        TurnOnDevicesWithColor(Color.red);
    }


    [MenuItem("Tool/Turn Off Devices - Red %#d")]
    private static void TurnOffDevices()
    {
        SADevice[] sADevices = GameObject.FindObjectsOfType<SADevice>();


        foreach (SADevice device in sADevices)
        {
            device.TurnOff();
        }
    }


    private static void TurnOnDevicesWithColor(Color lightColor)
    {


        Debug.Log($"Turning on the selected lights with color: {lightColor}");

        foreach (var obj in Selection.gameObjects)
        {
            // 自身にSADeviceがあるか
            SADevice saDevice = obj.GetComponent<SADevice>();

            // なければ親をたどる（GetComponentInParent で親も含めて検索）
            if (saDevice == null)
            {
                saDevice = obj.GetComponentInParent<SADevice>();
            }

            if (saDevice != null)
            {
               
                saDevice.TurnOnWithColor(lightColor);
                saDevice.GetComponent<DrawOnHover>()?.VisualizeTargetDevice(Color.blue);
            }
        }

    }




 

    public override void OnInspectorGUI()
    {
        // デフォルトの Inspector を描画
        DrawDefaultInspector();

        // ExperimentManager スクリプトのインスタンスを取得
        ExperimentManager script = (ExperimentManager)target;

        // **Inspector にボタンを追加**
        if (GUILayout.Button("Turn Off All Devices"))
        {
            // すべてのデバイスをオンにするメソッドを呼び出す
            script.DisableAllDevices();
        }

        if (GUILayout.Button("Turn On Selected Devices"))
        {
            script.TurnOnSelectedDevices();
        }


    }
    

 
}

