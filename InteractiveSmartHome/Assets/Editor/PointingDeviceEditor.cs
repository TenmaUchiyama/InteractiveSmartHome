using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PointingDevices))]
public class PointingDeviceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトの Inspector を描画
        DrawDefaultInspector();

        // PointingDevices スクリプトのインスタンスを取得
        PointingDevices script = (PointingDevices)target;

        // **Inspector にボタンを追加**
        if (GUILayout.Button("Turn On Selected Devices"))
        {
            script.TurnOnSelectedDevices();
        }


        if(GUILayout.Button("Pointing Done"))
        {
            
        }
    }
}

