using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SpatialLLM.Device;
using SpatialLLM.Experiment;

[CustomEditor(typeof(ExperimentManager))]
public class ExperimentManagerHelper : Editor
{
    private static Dictionary<GameObject, Color> selectedDeviceColorMap = new();

    // === 色割り当て ===
    [MenuItem("Tool/Set Light Color - Red %#q")] // Ctrl + Shift + Q
    private static void SetRed() => SetLightColor(Color.red);

    [MenuItem("Tool/Set Light Color - White %#w")] // Ctrl + Shift + W
    private static void SetWhite() => SetLightColor(Color.white);

    [MenuItem("Tool/Set Light Color - Blue %#e")] // Ctrl + Shift + E
    private static void SetBlue() => SetLightColor(Color.blue);

    private static void SetLightColor(Color color)
    {
        foreach (var obj in Selection.gameObjects)
        {
            selectedDeviceColorMap[obj] = color;
            Debug.Log($"✅ Assigned {ColorToName(color)} to {obj.name}");
        }

        Debug.Log("=== 📋 Current Device Assignments ===");
        foreach (var kvp in selectedDeviceColorMap)
        {
            Debug.Log($"{kvp.Key.name}: {ColorToName(kvp.Value)}");
        }
    }

    // === 一括点灯 ===
    // === 一括点灯 ===
[MenuItem("Tool/Execute: Turn On Assigned Lights %#a")] // Ctrl + Shift + A
private static void TurnOnAssignedLights()
{
    Debug.Log("=== 💡 Executing Light Activation ===");

    foreach (var kvp in selectedDeviceColorMap)
    {
        GameObject obj = kvp.Key;
        Color color = kvp.Value;

        if (obj == null) continue;

        // SADevice を自身または親から取得
        SADevice saDevice = obj.GetComponent<SADevice>() ?? obj.GetComponentInParent<SADevice>();
        if (saDevice != null)
        {
            Debug.Log($"🔷 Turning on {obj.name} with color {ColorToName(color)}");
            saDevice.TurnOnWithColor(color);

            // DrawOnHover も自身または親から取得してビジュアライズ
            DrawOnHover draw = saDevice.GetComponent<DrawOnHover>() ?? saDevice.GetComponentInChildren<DrawOnHover>();
            draw?.VisualizeTargetDevice(color);
        }
        else
        {
            Debug.LogWarning($"⚠️ SADevice not found on {obj.name} or its parents.");
        }
    }

    Debug.Log("✅ All assigned lights have been turned on.");
}

    // === 割り当て削除 ===
    [MenuItem("Tool/Clear All Assignments %#d")] // Ctrl + Shift + D
    private static void ClearAssignments()
    {
        selectedDeviceColorMap.Clear();
        Debug.Log("🧹 Cleared all device color assignments.");
    }

    // === 実験送信処理 ===
    [MenuItem("Tool/End Experiment and Submit %#t")] // Ctrl + Shift + T
    private static void SubmitExperiment()
    {
        Debug.Log("📤 Submitting experiment results...");
        // TODO: 実際の送信処理をここに記述する
        Debug.Log("✅ Experiment submitted successfully.");



        

    }

    // === カラー → 名前 ===
    private static string ColorToName(Color color)
    {
        if (color == Color.white) return "White";
        if (color == Color.blue) return "Blue";
        if (color == Color.red) return "Red";
        return $"R:{color.r:F2} G:{color.g:F2} B:{color.b:F2}";
    }

    // === Inspector拡張 ===
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExperimentManager script = (ExperimentManager)target;

        if (GUILayout.Button("Turn Off All Devices"))
        {
            script.DisableAllDevices();
        }

        if (GUILayout.Button("Turn On Selected Devices"))
        {
            script.TurnOnSelectedDevices();
        }
    }
}
