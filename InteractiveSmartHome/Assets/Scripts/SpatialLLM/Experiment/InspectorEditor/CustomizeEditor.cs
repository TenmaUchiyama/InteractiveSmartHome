using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;

[CustomEditor(typeof(TaskGenerator)), CanEditMultipleObjects]
public class CustomizeEditor : Editor
{   public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TaskGenerator experimentSetup = (TaskGenerator)target;

        if (GUILayout.Button("Add Task Data"))
        {
            experimentSetup.AddTaskData();
        }
        if (GUILayout.Button("Update Task Data"))
        {
            experimentSetup.UpdateTaskData();
        }
    }
}