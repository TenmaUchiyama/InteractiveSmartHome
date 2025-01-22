using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using Unity.VisualScripting;








public class TaskGenerator : MonoBehaviour
{


// taskId 
// taskName
// taskType 
// devices: devicename[] 
// condition

    public enum SpatialType 
    {
        ViewpointBased,
        PositionBased,
        DistanceBased,
        DirectionBased,
        HeightBased

    }
     [System.Serializable]
    public class TaskData
    {
        public string taskId;
        public string taskName; 
        public SpatialType taskType; 
        public List<string>  devices;
    }

    public string taskName = null; 
    public SpatialType taskType; 
    public List<GameObject> devices;



    public List<TaskData> taskDataList;



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
        string serialized = JsonConvert.SerializeObject(taskDataList, Formatting.Indented);

        string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json"); 

        if(!Directory.Exists(Path.Combine(Application.dataPath, "EXPERIMENT")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "EXPERIMENT"));
        }   


        File.WriteAllText(filePath, serialized);

        Debug.Log("JSON saved to: " + filePath);
    }


    [ContextMenu("Add Task Data")]
    public void AddTaskData()
    {
        this.LoadTaskData();

        TaskData taskData = new TaskData();
        List<string> deviceNames = devices.Select(d => d.name).ToList();
        taskData.taskId = Guid.NewGuid().ToString();
        taskData.taskName = taskName;
        taskData.taskType = taskType;
        taskData.devices = deviceNames;


        taskDataList.Add(taskData);
        UpdateTaskData();
        
    }



    [ContextMenu("Load Task Data")]
    public void LoadTaskData()
    {
        string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "TaskDeviceData.json"); 

        if(!File.Exists(filePath))
        {
            Debug.LogError("File does not exist");
            return;
        }

        string json = File.ReadAllText(filePath);

        taskDataList = JsonConvert.DeserializeObject<List<TaskData>>(json);
        
     


        


      
    }



    void ClearInput() 
    {
        taskName = null; 
        taskType = SpatialType.ViewpointBased;
        devices.Clear();
    }
    
   

}
