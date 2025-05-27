using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine;
using SpatialLLM.Experiment;

[CustomEditor(typeof(ExperimentManager))]
public class ExperimentManagerHelper : Editor
{
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

