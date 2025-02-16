using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using SpatialLLM.Experiment;
using SpatialLLM.Device;

[CustomEditor(typeof(DeviceArrangementGenerator))]
public class DeviceArrangementEditorHelper : Editor
{
    private SerializedProperty inputTaskDevicesProperty;

    private void OnEnable()
    {
        inputTaskDevicesProperty = serializedObject.FindProperty("inputTaskDevices");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Device Arrangement Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Set Selected Objects to DeviceArrangement"))
        {
            SetSelectedObjectsToArrangement();
        }

        EditorGUILayout.Space();

        for (int i = 0; i < inputTaskDevicesProperty.arraySize; i++)
        {
            SerializedProperty item = inputTaskDevicesProperty.GetArrayElementAtIndex(i);
            SerializedProperty device = item.FindPropertyRelative("device");
            SerializedProperty presetColor = item.FindPropertyRelative("presetColor");
            SerializedProperty customColor = item.FindPropertyRelative("customColor");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.ObjectField(device, new GUIContent("Device"));
            EditorGUILayout.PropertyField(presetColor, new GUIContent("Preset Color"));

            // Custom 色が選択されている場合は、Color Picker を表示
            if ((PresetColor)presetColor.enumValueIndex == PresetColor.Custom)
            {
                EditorGUILayout.PropertyField(customColor, new GUIContent("Custom Color"));
            }

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void SetSelectedObjectsToArrangement()
    {
        DeviceArrangementGenerator selectedObject = (DeviceArrangementGenerator)target;
        if (selectedObject == null)
        {
            Debug.LogWarning("DeviceArrangementGenerator がシーン内に見つかりませんでした。");
            return;
        }

        Undo.RecordObject(selectedObject, "Set Selected Objects");

        GameObject[] selectedObjects = Selection.gameObjects;
        selectedObject.inputTaskDevices.Clear();

        List<DeviceColorPair> newList = new List<DeviceColorPair>();

        foreach (GameObject obj in selectedObjects)
        {
            SADevice saDevice = obj.GetComponent<SADevice>(); 
            if (saDevice != null)
            {
                DeviceColorPair deviceColorPair = new DeviceColorPair()
                {
                    device = saDevice,
                    presetColor = PresetColor.White, // 初期値は White
                    customColor = Color.white
                };
                newList.Add(deviceColorPair);
            }
        }

        selectedObject.inputTaskDevices = newList;
        EditorUtility.SetDirty(selectedObject);

        Debug.Log("選択したオブジェクトで DeviceArrangementGenerator を上書きしました。");
    }
}
