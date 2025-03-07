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



        public string arrangementName = null;
        public DeviceColor selectingDeviceColor= DeviceColor.White;
        [SerializeField] private bool isTutorial = false;

        [SerializeField] public List<SpatialType> arrangementType;
        [SerializeField] public List<DeviceColorPair> inputTaskDevices;

        [Header("──────────────────────────────────────────────────────")]
        [SerializeField] private  List<DeviceArrangeData> arrangementDataList;
        public List<DeviceArrangeData> ArrangementDataList => arrangementDataList;

        public GameObject parentObject;

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
                device_arrange_type = new List<SpatialType>(arrangementType), // 新しいリストインスタンスを生成
                devices = new List<DeviceColorPair>(inputTaskDevices)
            };

            arrangementDataList.Add(arrangementData);
            this.inputTaskDevices.Clear();
            UpdateTaskData();
        }

        [ContextMenu("Load Task Data")]
        public void LoadTaskData()
{
        // 新しいリストを生成するのではなく、既存のリストを更新する
        var newData = ReadTaskData();
        
        if (arrangementDataList == null)
            arrangementDataList = new List<DeviceArrangeData>();
        else
            arrangementDataList.Clear();
        
        arrangementDataList.AddRange(newData);

        Debug.Log(arrangementDataList.Count());
    }

        private List<DeviceColorPairSerializable> ConvertToSerializable(List<DeviceColorPair> devicePairs)
        {
            return devicePairs.Select(d => new DeviceColorPairSerializable
            {
                deviceName = d.device.gameObject.name,
                colorName = d.color.ToString()
            }).ToList();
        }

        private List<DeviceColorPair> ConvertFromSerializable(List<DeviceColorPairSerializable> devicePairs)
        {

            List<SADevice>allDevices = new List<SADevice>(parentObject.GetComponentsInChildren<SADevice>(false));
            return devicePairs.Select(deviceJson =>
            {
                foreach (SADevice child in allDevices)
                {
                    SADevice saDevice = child.GetComponent<SADevice>();

                    if (saDevice != null && saDevice.name == deviceJson.deviceName)
                    {
                        if (!Enum.TryParse(deviceJson.colorName, out DeviceColor parsedColor))
                        {
                            parsedColor = DeviceColor.White;
                        }

                        return new DeviceColorPair
                        {
                            device = saDevice,
                            color = parsedColor
                        };
                    }
                }
                return null;
            }).Where(devicePair => devicePair != null).ToList();
        }


        private List<DeviceArrangeData> ConvertFromSerializable(List<DeviceArrangeDataSerializable> serializedData)
    {
        List<DeviceArrangeData> taskDatas = serializedData.Select(x => new DeviceArrangeData
        {
            device_arrange_id = x.device_arrange_id,
            device_arrange_name = x.device_arrange_name,
            device_arrange_type = x.device_arrange_type.Select(type => (SpatialType)Enum.Parse(typeof(SpatialType), type)).ToList(),
            devices = ConvertFromSerializable(x.devices) // ここを修正
        }).ToList();

        return taskDatas;
    }


    public List<DeviceArrangeDataSerializable> GetAllSerializedDeviceArrangeData() 
    {
        return ConvertToSerializable(arrangementDataList);
    }


        private List<DeviceArrangeDataSerializable> ConvertToSerializable(List<DeviceArrangeData> arrangeData)
        {
            List<DeviceArrangeDataSerializable> serializedData =arrangeData.Select(x => new DeviceArrangeDataSerializable() 
            {
                device_arrange_id  = x.device_arrange_id,
                device_arrange_name = x.device_arrange_name,
                device_arrange_type = x.device_arrange_type.Select(x => x.ToString()).ToList(),
                devices = ConvertToSerializable(x.devices)
            }).ToList();

            return serializedData; 

        }
        public void WriteTaskDataJson(List<DeviceArrangeData> tasks)
        {
            var serializableData = ConvertToSerializable(tasks);

            string serialized = JsonConvert.SerializeObject(serializableData, Formatting.Indented);
            string filePath = Path.Combine(Application.dataPath, "EXPERIMENT",isTutorial ?   "TutorialDeviceData.json":"TaskDeviceData.json");

            if (!Directory.Exists(Path.Combine(Application.dataPath, "EXPERIMENT")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "EXPERIMENT"));
            }

            File.WriteAllText(filePath, serialized);
            Debug.Log("JSON saved to: " + filePath);
        }

        public List<DeviceArrangeData> ReadTaskData()
        {
            string filePath = Path.Combine(Application.dataPath, "EXPERIMENT",  isTutorial ?   "TutorialDeviceData.json":"TaskDeviceData.json");

            if (!File.Exists(filePath))
            {
                Debug.LogError("File does not exist");
                return new List<DeviceArrangeData>();
            }

            string json = File.ReadAllText(filePath);


            Debug.Log($"{json}");
            var jsonDataList = JsonConvert.DeserializeObject<List<DeviceArrangeDataSerializable>>(json);

            List<DeviceArrangeData> taskDatas = ConvertFromSerializable(jsonDataList);

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
