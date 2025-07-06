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
        [Header("Arrangement Settings")]
        [Tooltip("新しい配置データの名前")]
        public string arrangementName;

        [Tooltip("選択中のデバイス色")]
        public DeviceColor selectingDeviceColor = DeviceColor.White;
        public Color selectingCustomColor = Color.white;

        [Tooltip("利用する空間タイプ一覧")]
        [SerializeField] public List<SpatialType> arrangementType = new List<SpatialType>();

        [Tooltip("タスクに含めるデバイスと色のペア")]
        [SerializeField] public List<DeviceColorPair> inputTaskDevices = new List<DeviceColorPair>();

        [Header("Loaded Data")]
        [Tooltip("読み込まれた配置データ一覧（読み取り専用で表示）")]
        [SerializeField] private List<DeviceArrangeData> arrangementDataList = new List<DeviceArrangeData>();
        public IReadOnlyList<DeviceArrangeData> ArrangementDataList => arrangementDataList;

        void Start()
        {
            LoadTaskData();
        }

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
                devices = new List<DeviceColorPair>(inputTaskDevices.Select(pair =>
                {
                    if (pair == null) return null;

                    return new DeviceColorPair
                    {
                        device = pair.device,
                        color = pair.color,
                        customColor = pair.color == DeviceColor.Custom ? pair.customColor : default
                    };
                }))
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
            string jsonFileRelativePath = $"ArrangeData/PreTaskArrangement{EXDataHolder.Instance.TaskSetName}.json";
            string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", jsonFileRelativePath);
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

                string id = SADeviceRef.Instance.GetDeviceIdByName(d.device.gameObject?.name);
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
            if (serializedData == null) return new List<DeviceArrangeData>();

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

        /// <summary>
        /// JSONファイルからタスクデータを読み込みます。
        /// IsSystemEvaluationがtrueの場合、A, B, C, DのJSONを統合して読み込みます。
        /// </summary>
        /// <returns>読み込まれたデバイス配置データのリスト</returns>
        public List<DeviceArrangeData> ReadTaskData()
        {
            // IsSystemEvaluationがtrueの場合、複数のファイルを統合
            if (EXDataHolder.Instance != null && EXDataHolder.Instance.IsSystemEvaluation)
            {
                var integratedDataList = new List<DeviceArrangeDataSerializable>();
                var taskSetNames = new[] { "A", "B", "C", "D" };

                Debug.Log("System Evaluation mode enabled. Integrating multiple arrangement files.");

                foreach (var setName in taskSetNames)
                {
                    string jsonFileRelativePath = $"ArrangeData/PreTaskArrangement{setName}.json";
                    string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", jsonFileRelativePath);

                    if (File.Exists(filePath))
                    {
                        string json = File.ReadAllText(filePath);
                        var dataList = JsonConvert.DeserializeObject<List<DeviceArrangeDataSerializable>>(json);
                        if (dataList != null)
                        {
                            integratedDataList.AddRange(dataList);
                            Debug.Log($"Loaded and integrated data from: {filePath}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"File does not exist, skipping: {filePath}");
                    }
                }

                return ConvertFromSerializable(integratedDataList);
            }
            else // それ以外の場合は、単一のファイルを読み込む
            {
                string filePath = GetJsonFilePath();
                if (!File.Exists(filePath))
                {
                    Debug.LogError("File does not exist: " + filePath);
                    return new List<DeviceArrangeData>();
                }
                string json = File.ReadAllText(filePath);
                var dataList = JsonConvert.DeserializeObject<List<DeviceArrangeDataSerializable>>(json);
                return ConvertFromSerializable(dataList);
            }
        }

        private void ClearInput()
        {
            arrangementName = string.Empty;
            arrangementType.Clear();
            inputTaskDevices.Clear();
        }

        public List<DeviceArrangeData> GetDeviceArrangeDatas()
        {
            if (arrangementDataList == null || arrangementDataList.Count == 0)
            {
                LoadTaskData();
            }
            return arrangementDataList;
        }
    }
}

