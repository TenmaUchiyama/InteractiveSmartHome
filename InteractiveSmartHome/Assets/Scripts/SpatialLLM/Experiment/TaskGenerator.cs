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
    public class TaskData
    {
        
        public string taskName; 

        public string taskId;
        public SpatialType taskType; 
        public List<SADevice>  devices;
    }


public class TaskGenerator : MonoBehaviour
{


// taskId 
// taskName
// taskType 
// devices: devicename[] 
// condition

   

    public string taskName = null; 
    public SpatialType taskType; 
    public List<SADevice> inputTaskDevices;



    public List<TaskData> taskDataList;



    public GameObject parentObject; 
   






  public class TaskDataJson
    {
        public string taskId;
        public string taskName; 
        public string taskType; 
        public List<string> devices;
    }
     

     private void OnValidate()
    {
        if(taskDataList != null)
        {
            this.UpdateTaskData();
        }
    }


    [ContextMenu("Update Task Data")]
    public void UpdateTaskData()
    {

        WriteTaskDataJson(this.taskDataList);
     

    }


    [ContextMenu("Add Task Data")]
    public void AddTaskData()
    {
        this.LoadTaskData();

        TaskData taskData = new TaskData();
        taskData.taskId = Guid.NewGuid().ToString();
        taskData.taskName = taskName;
        taskData.taskType = taskType;
        taskData.devices = inputTaskDevices;


        taskDataList.Add(taskData);
        UpdateTaskData();
       
        
    }



    [ContextMenu("Load Task Data")]
    public void LoadTaskData()
    {
    
        taskDataList = ReadTaskData();
    
      
    }


    public void WriteTaskDataJson(List<TaskData> tasks)
    {
        List<TaskDataJson> serializableJsonData = tasks.Select(x => new TaskDataJson {
            taskId = x.taskId,
            taskName = x.taskName,
            taskType = x.taskType.ToString(),
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


    public List<TaskData> ReadTaskData() 
    {
         string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json"); 

        if(!File.Exists(filePath))
        {
            Debug.LogError("File does not exist");
            return new List<TaskData>();
        }

        string json = File.ReadAllText(filePath);

        List<TaskDataJson> jsonDatas = JsonConvert.DeserializeObject<List<TaskDataJson>>(json);

        List<TaskData> taskDatas =  jsonDatas.Select(x => new TaskData{
            taskId = x.taskId,
            taskName = x.taskName,
            taskType = (SpatialType)Enum.Parse(typeof(SpatialType) , x.taskType),
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
        taskName = null; 
        taskType = SpatialType.ViewpointBased;
        inputTaskDevices.Clear();
    }



    

    
   

}
}