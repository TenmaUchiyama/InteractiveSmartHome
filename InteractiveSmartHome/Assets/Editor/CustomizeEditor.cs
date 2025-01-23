using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;

[CustomEditor(typeof(CustomizeEditor))]
public class CustomizeEditor : Editor
{   public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("ボタンの名前"))
        {
            TaskGenerator experimentSetup = (TaskGenerator)target;
            experimentSetup.UpdateTaskData();
        }
    }
}