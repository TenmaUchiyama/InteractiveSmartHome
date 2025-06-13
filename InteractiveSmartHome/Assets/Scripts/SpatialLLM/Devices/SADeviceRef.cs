using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Meta.WitAi.Json;
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



        WriteAllDeviceList();



        
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

    public List<SADevice> GetAllOperatedDevices()
    {
        List<SADevice> operatedDevice = new List<SADevice>();

        foreach (SADevice device in this.GetAllDevices())
        {
            if (device.IsDeviceOn)
            {
                operatedDevice.Add(device);
            }
         
        }
     

        return operatedDevice;
    }



    public List<SADevice> GetAllSelectedDevices()
    {
        return saDevices.Where(device => device.IsDeviceSelected()).ToList();
    }
    public void ClearAllDeviceOperation()
    {
        foreach (SADevice device in this.GetAllDevices())
        {
            device.TurnOff();
        }
    }
    public void AddSADevice(SADevice saDevice)
    {
        saDevice.transform.SetParent(parentObject.transform, true);
    }

    public GameObject GetSADeviceParent() => this.parentObject;
    public List<SADevice> GetAllDevicesRealTime() => GetAllDevices();


    public void UnSelectAllDevices()
    {
        foreach (SADevice device in this.GetAllDevices())
        {
            device.SetIsSelected(false);
        }
    }

    public void WriteAllDeviceList()
    {
        List<SADevice> devices = GetAllDevices();

        List<object> deviceList = new List<object>();

        foreach (var device in devices)
        {
            deviceList.Add(new
            {
                id = device.GetDBDeviceData().device_id,
                name = device.gameObject.name,
            });


            Debug.Log($"[WriteAllDeviceList] Device ID: {device.GetDBDeviceData().device_id}, Name: {device.gameObject.name}");



        }




       string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "device_list.json");
        string json = JsonConvert.SerializeObject(deviceList);

        File.WriteAllText(filePath, json);
            
        Debug.Log($"[WriteAllDeviceList] デバイスリストを {filePath} に書き込みました。");
    }



}


