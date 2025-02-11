using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using SpatialLLM.Device;
using Unity.Collections;


namespace SpatialLLM.Experiment{ 


 public enum SpatialType 
    {
        ViewpointBased,
        PositionBased,
        DistanceBased,
        DirectionBased,
        HeightBased

    }

 

 

     [Serializable]
    public class DeviceArrangeData
    {
        
        public string device_arrange_name; 

        public string device_arrange_id;
        public SpatialType device_arrange_type; 
        public List<SADevice>  devices;
    }


public class DeviceArrangementGenerator : MonoBehaviour
{


// taskId uuuuu
// taskName
// taskType 
// devices: devicename[] 
// condition

   

    public string arrangementName = null; 
    public SpatialType arrangementType; 
    public List<SADevice> inputTaskDevices;



    public List<DeviceArrangeData> arrangementDataList;



    public GameObject parentObject; 
   






  public class TaskDataJson
    {
        public string arrangementId;
        public string arrangementName; 
        public string arrangementType; 
        public List<string> devices;
    }
     

     private void OnValidate()
    {
        if(arrangementDataList != null)
        {
            this.UpdateTaskData();
        }
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

        DeviceArrangeData arrangementData = new DeviceArrangeData();
        arrangementData.device_arrange_id = Guid.NewGuid().ToString();
        arrangementData.device_arrange_name = arrangementName;
        arrangementData.device_arrange_type = arrangementType;
        arrangementData.devices = inputTaskDevices;


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
        List<TaskDataJson> serializableJsonData = tasks.Select(x => new TaskDataJson {
            arrangementId = x.device_arrange_id,
            arrangementName = x.device_arrange_name,
            arrangementType = x.device_arrange_type.ToString(),
            devices = x.devices.Select(x => x.gameObject.name).ToList()
        }).ToList();
        string serialized = JsonConvert.SerializeObject(serializableJsonData, Formatting.Indented);

        string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json"); 

        if(!Directory.Exists(Path.Combine(Application.dataPath, "EXPERIMENT")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "EXPERIMENT"));
        }   


        File.WriteAllText(filePath, serialized);

        Debug.Log("JSON saved to:" + filePath);
    }


    public List<DeviceArrangeData> ReadTaskData() 
    {
         string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json"); 

        if(!File.Exists(filePath))
        {
            Debug.LogError("File does not exist");
            return new List<DeviceArrangeData>();
        }

        string json = File.ReadAllText(filePath);

        List<TaskDataJson> jsonDatas = JsonConvert.DeserializeObject<List<TaskDataJson>>(json);

        List<DeviceArrangeData> taskDatas =  jsonDatas.Select(x => new DeviceArrangeData{
            device_arrange_id = x.arrangementId,
            device_arrange_name = x.arrangementName,
            device_arrange_type = (SpatialType)Enum.Parse(typeof(SpatialType) , x.arrangementType),
           devices = x.devices.Select(deviceName =>
                    {
  
                        foreach (Transform child in parentObject.transform)
                        {
                          
                            SADevice saDevice = child.GetComponent<SADevice>();
                            
                            if (saDevice != null && saDevice.name == deviceName)
                            {
                                return saDevice;
                            }
                        }
                        return null;
                    }).Where(saDevice => saDevice != null).ToList() 

        }).ToList();


        return taskDatas;
      
    }



    void ClearInput() 
    {
        arrangementName = null; 
        arrangementType = SpatialType.ViewpointBased;
        inputTaskDevices.Clear();
    }



    

    
   

}
}