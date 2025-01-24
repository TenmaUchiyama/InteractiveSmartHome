using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SpatialLLM.Device;
using Unity.VisualScripting;

[CustomEditor(typeof(SADevice))]
public class DeviceSelectorEditorHelper : Editor 
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameObject selectedObject = ((SADevice)target).gameObject;


        EditorGUILayout.LabelField("SElected Object:", selectedObject.name); 
        Debug.Log("Selected Object: " + selectedObject.name);

        DrawDefaultInspector();     

    }
}
