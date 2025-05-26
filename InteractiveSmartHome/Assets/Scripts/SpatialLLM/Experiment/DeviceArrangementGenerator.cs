using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using SpatialLLM.Device;

namespace SpatialLLM.Experiment
{
    [Serializable]
    public class DeviceArrangeData
    {
        public string device_arrange_id;
        public string device_arrange_name;
        public List<SpatialType> device_arrange_type;
        public List<DeviceColorPair> devices;
    }

    [Serializable]
    public class DeviceArrangeDataSerializable
    {
        public string device_arrange_id;
        public string device_arrange_name;
        public List<string> device_arrange_type;
        public List<DeviceColorPairSerializable> devices;
    }

    public class DeviceArrangementGenerator : MonoBehaviour
    {
        [Header("File Settings")]
        [Tooltip("EXPERIMENT フォルダ内の相対パスを指定（例: TaskDeviceData.json または ArrangeData/Test.json）")]
        [SerializeField] private string jsonFileRelativePath = "TaskDeviceData.json";

        [Header("Arrangement Settings")]
        [Tooltip("新しい配置データの名前")]
        public string arrangementName;

        [Tooltip("選択中のデバイス色")]
        public DeviceColor selectingDeviceColor = DeviceColor.White;

        [Tooltip("利用する空間タイプ一覧")]
        [SerializeField] public List<SpatialType> arrangementType = new List<SpatialType>();

        [Tooltip("タスクに含めるデバイスと色のペア")]
        [SerializeField] public List<DeviceColorPair> inputTaskDevices = new List<DeviceColorPair>();

        [Header("Loaded Data")]
        [Tooltip("読み込まれた配置データ一覧（読み取り専用で表示）")]
        [SerializeField] private List<DeviceArrangeData> arrangementDataList = new List<DeviceArrangeData>();
        public IReadOnlyList<DeviceArrangeData> ArrangementDataList => arrangementDataList;

        [ContextMenu("Update Task Data")]
        public void UpdateTaskData()
        {
            WriteTaskDataJson(arrangementDataList);
        }

        [ContextMenu("Add Task Data")]
        public void AddTaskData()
        {
            LoadTaskData();

            var arrangementData = new DeviceArrangeData
            {
                device_arrange_id = Guid.NewGuid().ToString(),
                device_arrange_name = arrangementName,
                device_arrange_type = new List<SpatialType>(arrangementType),
                devices = new List<DeviceColorPair>(inputTaskDevices)
            };

            arrangementDataList.Add(arrangementData);
            ClearInput();
            UpdateTaskData();
        }

        [ContextMenu("Load Task Data")]
        public void LoadTaskData()
        {
            var newData = ReadTaskData();
            arrangementDataList.Clear();
            arrangementDataList.AddRange(newData);
            Debug.Log($"Loaded {arrangementDataList.Count} entries.");
        }

        /// <summary>
        /// JSON シリアライズ可能なデータを取得します。
        /// </summary>
        public List<DeviceArrangeDataSerializable> GetAllSerializedDeviceArrangeData()
        {
            return ConvertToSerializable(arrangementDataList);
        }

        private string GetJsonFilePath()
        {
            // EXPERIMENT フォルダを基点とした相対パスを結合
            string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", jsonFileRelativePath);
            // フォルダ部分を取得して存在確認・作成
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return filePath;
        }

        private List<DeviceColorPairSerializable> ConvertToSerializable(List<DeviceColorPair> devicePairs)
        {
            Debug.Log($"<color=yellow>Device Pairs Count: {devicePairs.Count}");
            return devicePairs.Select(d =>
            {
                if (d == null || d.device == null)
                {
                    Debug.LogWarning("Invalid DeviceColorPair encountered during serialization");
                    return null;
                }
                
                string id = SADeviceRef.Instance.GetDeviceIdByName(d.device.gameObject?.name); ;
                return new DeviceColorPairSerializable
                {
                    deviceName = d.device.gameObject?.name ?? "Unknown",
                    deviceId = id ?? "Unknown",
                    colorName = d.color.ToString()
                };
            })
            .Where(x => x != null)
            .ToList();
        }

        private List<DeviceColorPair> ConvertFromSerializable(List<DeviceColorPairSerializable> devicePairs)
        {
            var allDevices = SADeviceRef.Instance.GetAllDevices();
            return devicePairs.Select(json =>
            {
                foreach (var dev in allDevices)
                {
                    if (dev != null && dev.name == json.deviceName)
                    {
                        if (!Enum.TryParse(json.colorName, out DeviceColor parsedColor))
                            parsedColor = DeviceColor.White;
                        return new DeviceColorPair { device = dev, color = parsedColor };
                    }
                }
                return null;
            })
            .Where(x => x != null)
            .ToList();
        }

        private List<DeviceArrangeData> ConvertFromSerializable(List<DeviceArrangeDataSerializable> serializedData)
        {
            return serializedData.Select(x => new DeviceArrangeData
            {
                device_arrange_id = x.device_arrange_id,
                device_arrange_name = x.device_arrange_name,
                device_arrange_type = x.device_arrange_type.Select(type => (SpatialType)Enum.Parse(typeof(SpatialType), type)).ToList(),
                devices = ConvertFromSerializable(x.devices)
            }).ToList();
        }

        private List<DeviceArrangeDataSerializable> ConvertToSerializable(List<DeviceArrangeData> arrangeData)
        {
            return arrangeData.Select(x => new DeviceArrangeDataSerializable
            {
                device_arrange_id = x.device_arrange_id,
                device_arrange_name = x.device_arrange_name,
                device_arrange_type = x.device_arrange_type.Select(t => t.ToString()).ToList(),
                devices = ConvertToSerializable(x.devices)
            }).ToList();
        }

        public void WriteTaskDataJson(List<DeviceArrangeData> tasks)
        {
            var serializableData = ConvertToSerializable(tasks);
            string serialized = JsonConvert.SerializeObject(serializableData, Formatting.Indented);
            string filePath = GetJsonFilePath();
            File.WriteAllText(filePath, serialized);
            Debug.Log("JSON saved to: " + filePath);
        }

        public List<DeviceArrangeData> ReadTaskData()
        {
            string filePath = GetJsonFilePath();
            if (!File.Exists(filePath))
            {
                Debug.LogError("File does not exist: " + filePath);
                return new List<DeviceArrangeData>();
            }
            string json = File.ReadAllText(filePath);
            Debug.Log(json);
            var dataList = JsonConvert.DeserializeObject<List<DeviceArrangeDataSerializable>>(json);
            return ConvertFromSerializable(dataList);
        }

        private void ClearInput()
        {
            arrangementName = string.Empty;
            arrangementType.Clear();
            inputTaskDevices.Clear();
        }
    }
}