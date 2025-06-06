using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpatialLLM.Device;
using UnityEditor;
using UnityEngine;





[Serializable]
public class DeviceIdEntry
{
    public string deviceName;
    public string deviceId;
}

[Serializable]
public class DeviceIdListWrapper
{
    public List<DeviceIdEntry> devices = new List<DeviceIdEntry>();
}


public class SADeviceRef : Singleton<SADeviceRef>
{
    [SerializeField] GameObject parentObject;
    private List<SADevice> saDevices = new List<SADevice>();
    private Dictionary<string, string> deviceNameToId = new Dictionary<string, string>();

    private string savePath => Path.Combine(Application.persistentDataPath, "device_ids.json");

    void Start()
    {
        // 子オブジェクトから取得
        saDevices = parentObject.GetComponentsInChildren<SADevice>(false).ToList();

        LoadOrGenerateDeviceIds();
    }


    public string GetDeviceID(string deviceName)
    {
        if (deviceNameToId.TryGetValue(deviceName, out string deviceId))
        {
            return deviceId;
        }
        else
        {
            LoadOrGenerateDeviceIds(); // 再読み込みして最新のIDを取得
            if (deviceNameToId.TryGetValue(deviceName, out deviceId))
            {
                return deviceId;
            }
            else
            {
                Debug.LogWarning($"[GetDeviceID] デバイス名 '{deviceName}' に対応するIDが見つかりません。");
                return null;
            }
        }
    }


    private void LoadOrGenerateDeviceIds()
    {
        DeviceIdListWrapper wrapper = new DeviceIdListWrapper();

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            wrapper = JsonUtility.FromJson<DeviceIdListWrapper>(json);
            deviceNameToId = wrapper.devices.ToDictionary(d => d.deviceName, d => d.deviceId);
        }

        // 存在しないデバイスにIDを追加
        foreach (var device in saDevices)
        {
            string name = device.gameObject.name;
            if (!deviceNameToId.ContainsKey(name))
            {
                string newId = Guid.NewGuid().ToString();
                deviceNameToId[name] = newId;
                wrapper.devices.Add(new DeviceIdEntry
                {
                    deviceName = name,
                    deviceId = newId
                });
                Debug.Log($"[新規ID生成] {name} → {newId}");
            }
        }

        // 保存
        string newJson = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(savePath, newJson);
    }


    public string GetDeviceIdByName(string deviceName)
    {
        string path = Path.Combine(Application.persistentDataPath, "device_ids.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning("[GetDeviceIdByName] device_ids.json が存在しません");
            return null;
        }

        string json = File.ReadAllText(path);
        DeviceIdListWrapper wrapper = JsonUtility.FromJson<DeviceIdListWrapper>(json);

        foreach (var entry in wrapper.devices)
        {
            if (entry.deviceName == deviceName)
            {
                return entry.deviceId;
            }
        }

        Debug.LogWarning($"[GetDeviceIdByName] {deviceName} は device_ids.json に見つかりませんでした");
        return null;
    }

    public List<SADevice> GetAllDevices() => parentObject.GetComponentsInChildren<SADevice>(false).ToList();

    public SADevice GetDeviceById(string id)
    {

        return saDevices.Find(device => device.GetDBDeviceData().device_id == id);
    }

    public void AddSADevice(SADevice saDevice)
    {
        saDevice.transform.SetParent(parentObject.transform, true);
    }

    public GameObject GetSADeviceParent() => this.parentObject;
    public List<SADevice> GetAllDevicesRealTime() => GetAllDevices();



   [MenuItem("Tools/Rename SADevices Children")]
    public static void RenameSADeviceChildren()
    {
        GameObject parent = GameObject.Find("SADevices");
        if (parent == null)
        {
            Debug.LogError("GameObject 'SADevices' が見つかりませんでした。");
            return;
        }

        var saDevices = parent.GetComponentsInChildren<SADevice>(true);
        if (saDevices.Length == 0)
        {
            Debug.LogWarning("子オブジェクトに SADevice コンポーネントが見つかりませんでした。");
            return;
        }

        foreach (var device in saDevices)
        {
            string oldName = device.gameObject.name;
            string newName = ConvertToEnglishName(oldName);
            if (!string.IsNullOrEmpty(newName))
            {
                device.gameObject.name = newName;
                Debug.Log($"[Rename] {oldName} → {newName}");
            }
            else
            {
                Debug.LogWarning($"[Rename] {oldName} は変換対象ではありません。");
            }
        }
    }

    private static string ConvertToEnglishName(string oldName)
    {
        // var match = System.Text.RegularExpressions.Regex.Match(
        //     oldName,
        //     @"^(天井ライト|壁ライト|テーブルライト|ランプスタンド|フロアライト|棚ライト)(\d*)$");
        var match = System.Text.RegularExpressions.Regex.Match(
        oldName, @"^(CeilingLight|WallLight|TableLight|StandLight|FloorLight|ShelfLight|TVLight)(\d+)$");


        if (!match.Success) return null;

        string prefix = match.Groups[1].Value;
        string number = match.Groups[2].Value;

        // return prefix switch
        // {
        //     "天井ライト" => $"CeilingLight{number}",
        //     "壁ランプ" => $"WallLight{number}",
        //     "テーブルライト" => $"TableLight{number}",
        //     "ランプスタンド" => $"StandLight{number}",
        //     "フロアライト" => $"FloorLight{number}",
        //     "棚ライト" => $"ShelfLight{number}",
        //     "テレビライト" => $"TVLight{number}",

        //     _ => null,
        // };
         return prefix switch
    {
        "CeilingLight" => $"Ceiling Light {number}",
        "WallLight"    => $"Wall Light {number}",
        "TableLight"   => $"Table Light {number}",
        "StandLight"   => $"Stand Light {number}",
        "FloorLight"   => $"Floor Light {number}",
        "ShelfLight"   => $"Shelf Light {number}",
        "TVLight"      => $"TV Light {number}",
        _ => null,
    };
    }

}


