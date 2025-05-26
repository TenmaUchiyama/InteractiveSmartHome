using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpatialLLM.Device;
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

        // 各デバイスにIDを適用
        foreach (var device in saDevices)
        {
            string name = device.gameObject.name;
            if (deviceNameToId.TryGetValue(name, out string id))
            {
                device.GetDBDeviceData().device_id = id;
                Debug.Log($"[ID割当] {name} → {id}");
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
}


