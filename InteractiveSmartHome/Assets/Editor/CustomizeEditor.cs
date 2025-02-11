using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using SpatialLLM.Experiment;

[CustomEditor(typeof(DeviceArrangementGenerator))]
public class CustomizeEditor : Editor
{   public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


          DeviceArrangementGenerator experimentSetup = (DeviceArrangementGenerator)target;


        if (GUILayout.Button("Add Task Data"))
        {
            
            experimentSetup.AddTaskData();
        }

        if (GUILayout.Button("Update Task Data"))
        {
           
            experimentSetup.UpdateTaskData();
        }


         if (GUILayout.Button("Load Task Data"))
        {
          
            experimentSetup.LoadTaskData();
        }
    }
}