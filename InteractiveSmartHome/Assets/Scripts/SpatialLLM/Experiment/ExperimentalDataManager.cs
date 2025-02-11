using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;





namespace SpatialLLM.Experiment
{


public class ExperimentalDataManager : MonoBehaviour
{
    
    [SerializeField] private string user_name; 
    


    
    

    public async Task WriteExperimentalDataAsync(ExperimentTask experimentalTask)
    {
        ExperimentTaskData taskData = experimentalTask.GetExperimentTaskData(); 

        string serialized = JsonConvert.SerializeObject(taskData, Formatting.Indented);

        string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", user_name , $"experiment_task_data_{taskData.taskId}.json");

        if(!Directory.Exists(Path.Combine(Application.dataPath, "EXPERIMENT", user_name)))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "EXPERIMENT", user_name));
        }  

        await File.WriteAllTextAsync(filePath, serialized);

        await WriteWholeData(taskData);
    }


    public async Task WriteWholeData(ExperimentTaskData taskData) 
    {
         string wholeDirectory = Path.Combine(Application.dataPath, "EXPERIMENT", "WHOLE");
    if (!Directory.Exists(wholeDirectory))
    {
        Directory.CreateDirectory(wholeDirectory);
    }
        string wholeFilePath = Path.Combine(wholeDirectory, $"{user_name}.json");

            // 既存の全タスクデータを読み込む（ファイルが存在すれば、JSON 配列を List<ExperimentTaskData> にデシリアライズする）
            List<ExperimentTaskData> allTasks;
            if (File.Exists(wholeFilePath))
            {
                string existingData = File.ReadAllText(wholeFilePath);
                // ファイルが空の場合も考慮
                allTasks = string.IsNullOrEmpty(existingData) 
                        ? new List<ExperimentTaskData>() 
                        : JsonConvert.DeserializeObject<List<ExperimentTaskData>>(existingData);
            }
            else
            {
                allTasks = new List<ExperimentTaskData>();
            }

            // 新しいタスクデータを配列に追加
            allTasks.Add(taskData);

            // 更新した配列を JSON 文字列にシリアライズして再保存
            string wholeSerialized = JsonConvert.SerializeObject(allTasks, Formatting.Indented);
            await File.WriteAllTextAsync(wholeFilePath, wholeSerialized);
        }
    }
    





}