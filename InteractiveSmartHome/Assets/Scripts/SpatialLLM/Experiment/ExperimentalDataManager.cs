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
        [SerializeField] SystemExecutor systemExecutor;
        private ExperimentManager experimentManager;


        public void TurnOnSend()
        {

        }


        void Start()
        {
            experimentManager = GetComponent<ExperimentManager>();
        }


        public string GetUserName()
        {
            return user_name;
        }


        // public async Task WriteExperimentalDataAsync(ExperimentTask experimentalTask)
        // {

        //     Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Writing!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        //     ExperimentTaskData taskData = experimentalTask.GetExperimentTaskData(); 

        //     string serialized = JsonConvert.SerializeObject(taskData, Formatting.Indented);

        //     string filePath = Path.Combine(Application.dataPath, "EXPERIMENT", user_name , $"{experimentManager.CurrentArrangeIndex}_task_{taskData.taskId}.json");

        //     if(!Directory.Exists(Path.Combine(Application.dataPath, "EXPERIMENT", user_name)))
        //     {
        //          Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Creating!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        //         Directory.CreateDirectory(Path.Combine(Application.dataPath, "EXPERIMENT", user_name));
        //     }  

        //     await File.WriteAllTextAsync(filePath, serialized);

        //     await WriteWholeData(taskData);
        // }


        // public async Task WriteWholeData(ExperimentTaskDataForSave taskData) 
        // {
        //      string wholeDirectory = Path.Combine(Application.dataPath, "EXPERIMENT", "WHOLE");
        // if (!Directory.Exists(wholeDirectory))
        // {
        //     Directory.CreateDirectory(wholeDirectory);
        // }
        //     string wholeFilePath = Path.Combine(wholeDirectory, $"{user_name}.json");

        //         // 既存の全タスクデータを読み込む（ファイルが存在すれば、JSON 配列を List<ExperimentTaskData> にデシリアライズする）
        //         List<ExperimentTaskDataForSave> allTasks;
        //         if (File.Exists(wholeFilePath))
        //         {
        //             string existingData = File.ReadAllText(wholeFilePath);
        //             // ファイルが空の場合も考慮
        //             allTasks = string.IsNullOrEmpty(existingData) 
        //                     ? new List<ExperimentTaskDataForSave>() 
        //                     : JsonConvert.DeserializeObject<List<ExperimentTaskDataForSave>>(existingData);
        //         }
        //         else
        //         {
        //             allTasks = new List<ExperimentTaskDataForSave>();
        //         }

        //         // 新しいタスクデータを配列に追加
        //         allTasks.Add(taskData);

        //         // 更新した配列を JSON 文字列にシリアライズして再保存
        //         string wholeSerialized = JsonConvert.SerializeObject(allTasks, Formatting.Indented);
        //         await File.WriteAllTextAsync(wholeFilePath, wholeSerialized);
        //     }
        // }




    }
}