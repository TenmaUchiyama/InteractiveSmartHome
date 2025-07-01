using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SpatialLLM.Device;
using static SpatialLLM.Network.NetworkDataType;

public class SystemEvaluationDataGenerator : MonoBehaviour
{  

     public string keyword = "Shelf";

    public string outputFolder = "EXPERIMENT/DeviceSpatialData";

    void Update()
    {
          if (Input.GetKeyDown(KeyCode.Space))
        {
            List<DeviceSpatialData> filteredDevices = new();

            foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
            {
                string deviceName = device.GetDBDeviceData().device_name;

                if (!deviceName.Contains(keyword)) continue;  // ← キーワードにマッチするか

                DeviceSpatialData data = device.GetDevicePositionalRelativeToUser(Camera.main.transform);
                filteredDevices.Add(data);
            }

            string dirPath = Path.Combine(Application.dataPath, "EXPERIMENT/DeviceSpatialData");
            Directory.CreateDirectory(dirPath);

            string path = Path.Combine(dirPath, $"{keyword}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(filteredDevices, Formatting.Indented));

            Debug.Log($"✅ Saved {filteredDevices.Count} devices to: {path}");
        }
    }
}
