using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using static TaskGenerator;

public class ExperimentManager : MonoBehaviour
{
    List<TaskData> taskDatas = new List<TaskData>();


    GameObject parentObject;




    private void Start() {
        ReadTaskData();


        foreach (var taskData in taskDatas)
        {
            Debug.Log("=====================================");
            Debug.Log($"<color=yellow>Task ID: {taskData.taskId}</color>");
            Debug.Log($"<color=yellow>Task Name: {taskData.taskName}</color>");
            Debug.Log($"<color=yellow>Task Type: {taskData.taskType}</color>");
            Debug.Log($"<color=yellow>Devices: {string.Join(",", taskData.devices)}</color>");
            Debug.Log("=====================================");
        }
    }




    private void ReadTaskData() 
    {

        string taskDataPath = Path.Combine(Application.dataPath, "EXPERIMENT" , "TaskDeviceData.json");
        if (File.Exists(taskDataPath))
        {
            string json = File.ReadAllText(taskDataPath);

            taskDatas = JsonConvert.DeserializeObject<List<TaskData>>(json);
        }
        else
        {
            Debug.LogError("Task Data File Not Found");
            return;
        }
    }



}
