using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using SpatialLLM.Device;

namespace SpatialLLM.Experiment
{

    [Serializable]
    public class DeviceArrangeData
    {
        public string device_arrange_name;
        public string device_arrange_id;
        public List<SpatialType> device_arrange_type;
        public List<DeviceColorPair> devices;
    }

    public class DeviceArrangementGenerator : MonoBehaviour
    {
        public string arrangementName = null;
        public List<SpatialType> arrangementType;
        public List<DeviceColorPair> inputTaskDevices;

        [Header("──────────────────────────────────────────────────────")]
        public List<DeviceArrangeData> arrangementDataList;

        public GameObject parentObject;

        [Serializable]
        public class TaskDataJson
        {
            public string arrangementId;
            public string arrangementName;
            public List<string> arrangementType;
            public List<DeviceJsonData> devices;
        }

        [Serializable]
        public class DeviceJsonData
        {
            public string deviceName;
            public string colorHex;
        }

        [ContextMenu("Update Task Data")]
        public void UpdateTaskData()
        {
            WriteTaskDataJson(this.arrangementDataList);
        }

        [ContextMenu("Add Task Data")]
        public void AddTaskData()
        {
            this.LoadTaskData();

            DeviceArrangeData arrangementData = new DeviceArrangeData
            {
                device_arrange_id = Guid.NewGuid().ToString(),
                device_arrange_name = arrangementName,
                device_arrange_type = arrangementType,
                devices = inputTaskDevices
            };

            arrangementDataList.Add(arrangementData);
            UpdateTaskData();
        }

        [ContextMenu("Load Task Data")]
        public void LoadTaskData()
        {
            arrangementDataList = ReadTaskData();
        }

        public void WriteTaskDataJson(List<DeviceArrangeData> tasks)
        {
            List<TaskDataJson> serializableJsonData = tasks.Select(x => new TaskDataJson
            {
                arrangementId = x.device_arrange_id,
                arrangementName = x.device_arrange_name,
                arrangementType = x.device_arrange_type.Select(type => type.ToString()).ToList(),
                devices = x.devices.Select(deviceColorPair => new DeviceJsonData
                {
                    deviceName = deviceColorPair.device.gameObject.name,
                    colorHex = ColorUtility.ToHtmlStringRGB(deviceColorPair.GetFinalColor())
                }).ToList()
            }).ToList();

            string serialized = JsonConvert.SerializeObject(serializableJsonData, Formatting.Indented);
            string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json");

            if (!Directory.Exists(Path.Combine(Application.dataPath, "EXPERIMENT")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "EXPERIMENT"));
            }

            File.WriteAllText(filePath, serialized);
            Debug.Log("JSON saved to:" + filePath);
        }

        public List<DeviceArrangeData> ReadTaskData()
        {
            string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json");

            if (!File.Exists(filePath))
            {
                Debug.LogError("File does not exist");
                return new List<DeviceArrangeData>();
            }

            string json = File.ReadAllText(filePath);
            List<TaskDataJson> jsonDatas = JsonConvert.DeserializeObject<List<TaskDataJson>>(json);

            List<DeviceArrangeData> taskDatas = jsonDatas.Select(x => new DeviceArrangeData
            {
                device_arrange_id = x.arrangementId,
                device_arrange_name = x.arrangementName,
                device_arrange_type = x.arrangementType.Select(type => (SpatialType)Enum.Parse(typeof(SpatialType), type)).ToList(),
                devices = x.devices.Select(deviceJson =>
    {
            foreach (Transform child in parentObject.transform)
            {
                SADevice saDevice = child.GetComponent<SADevice>();

                if (saDevice != null && saDevice.name == deviceJson.deviceName)
                {
                    // Enumに変換（エラー処理なしでOK）
                    DeviceColor deviceColor = (DeviceColor)Enum.Parse(typeof(DeviceColor), deviceJson.color);

                    return new DeviceColorPair
                    {
                        device = saDevice,
                        presetColor = deviceColor
                    };
                }
            }
        return null;
    }).Where(devicePair => devicePair != null).ToList()
            }).ToList();

            return taskDatas;
        }

        void ClearInput()
        {
            arrangementName = null;
            arrangementType = new List<SpatialType>();
            inputTaskDevices.Clear();
        }
    }
}
