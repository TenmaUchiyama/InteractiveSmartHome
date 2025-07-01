using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json;
using UnityEngine;
namespace SpatialLLM.Experiment
{



    [Serializable]
    public class SingleTaskAttempt
    {

        public string attemptId;

        /// <summary>
        /// タスクの試行開始時間
        /// 確認した後、からカウントをスタートする
        /// </summary>
        public string taskElapsedTime;


        /// <summary>
        /// ユーザーの発した音声コマンド
        /// </summary>
        public string userCommand;


        public List<string> outputDevices = new List<string>();
        
    }



    [Serializable]
    public class ExperimentTaskDataForSave
    {
        public string taskId;
        public int taskAttemptCount = 0;
        public float totalElapsedTime = 0f;
        public string finalId = "";
        public List<SingleTaskAttempt> taskAttempts = new List<SingleTaskAttempt>();


        public ExperimentTaskDataForSave(string taskId)
        {
            this.taskId = taskId;

        }


        public void IncrementTaskAttemptCount()
        {
            taskAttemptCount++;
        }   



        public void AddTaskAttempt(SingleTaskAttempt attempt)
        {
            if (attempt == null) return;

            this.finalId = attempt.attemptId;
            taskAttempts.Add(attempt);
        }
     

    }

    /// <summary>
    /// これは、それぞれのタスクにおいて、実行時間、音声コマンド、試行回数などを保存するためのデータクラス。
    /// </summary>
    public class ExperimentTask : MonoBehaviour
    {

        private ExperimentTaskDataForSave data;


        private string nextGuid = Guid.NewGuid().ToString();
        public string NextGuid => nextGuid;

        public void InitExperimentTask(string taskId)
        {
            data = new ExperimentTaskDataForSave(taskId);
        }


        public void IncrementTaskAttemptCount()
        {
            data.IncrementTaskAttemptCount();
        }


        public string AddTaskAttempt(string recognizedWord, string timestamp, List<string> deviceIds)
            {
            string attemptId = nextGuid;

                    var attempt = new SingleTaskAttempt
                    {
                        attemptId = attemptId,
                        taskElapsedTime = timestamp,
                        userCommand = recognizedWord,
                        outputDevices = deviceIds
                    };

                    data.AddTaskAttempt(attempt);
                nextGuid = Guid.NewGuid().ToString(); // 次の試行のために新しいGUIDを生成
                return attemptId;
            }

        public ExperimentTaskDataForSave GetExperimentTaskDataForSave()
        {
            return data;
        }

        public ExperimentTaskDataForSave GetExperimentTaskData()
        {
            return data;
        }



        public string GetSerializedTaskData()
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

    }
}