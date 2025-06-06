using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using SpatialLLM.Experiment;
using SpatialLLM.Device;
using System.Linq;

[CustomEditor(typeof(DeviceArrangementGenerator))]
public class DeviceArrangementEditorHelper : Editor
{
    private SerializedProperty inputTaskDevicesProperty;
    private SerializedProperty selectingDeviceColorProperty;

    private void OnEnable()
    {
        inputTaskDevicesProperty = serializedObject.FindProperty("inputTaskDevices");
        selectingDeviceColorProperty = serializedObject.FindProperty("selectingDeviceColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Device Arrangement Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Set Selected Objects to DeviceArrangement (Ctrl+Shift+D)"))
        {
            SetSelectedObjectsToArrangement();
        }

        EditorGUILayout.Space();

        for (int i = 0; i < inputTaskDevicesProperty.arraySize; i++)
        {
            SerializedProperty item = inputTaskDevicesProperty.GetArrayElementAtIndex(i);
            SerializedProperty device = item.FindPropertyRelative("device");
            SerializedProperty color = item.FindPropertyRelative("color");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.ObjectField(device, new GUIContent("Device"));
            EditorGUILayout.PropertyField(color, new GUIContent("Color"));
            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("Tools/Set Selected Devices to Arrangement %#x")] // Ctrl+Shift+X
    private static void SetSelectedObjectsToArrangement()
    {
        DeviceArrangementGenerator selectedObject = FindObjectOfType<DeviceArrangementGenerator>();
        if (selectedObject == null)
        {
            Debug.LogWarning("DeviceArrangementGenerator がシーン内に見つかりませんでした。");
            return;
        }

        Undo.RecordObject(selectedObject, "Set Selected Objects");

        GameObject[] selectedObjects = Selection.gameObjects;


        Debug.Log($"{selectedObjects.Count()}");

        List<DeviceColorPair> newList = new List<DeviceColorPair>();

        foreach (GameObject obj in selectedObjects)
        {
            SADevice saDevice = obj.GetComponent<SADevice>();
            if (saDevice != null)
            {
                DeviceColorPair deviceColorPair = new DeviceColorPair()
                {
                    device = saDevice,
                    color = selectedObject.selectingDeviceColor // selectingDeviceColor を適用
                };
                selectedObject.inputTaskDevices.Add(deviceColorPair);
            }
        }

        EditorUtility.SetDirty(selectedObject);

        Debug.Log("選択したオブジェクトで DeviceArrangementGenerator を上書きしました。");
    }

}
