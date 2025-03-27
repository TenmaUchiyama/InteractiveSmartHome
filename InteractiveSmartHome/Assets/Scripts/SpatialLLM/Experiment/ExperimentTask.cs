using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialLLM.Device;
using UnityEngine;
namespace SpatialLLM.Experiment
{

    

[Serializable]
public class ExperimentTaskData
{
    public string user_name {get;set;}
    public string taskId { get; set; }
    public string taskName { get; set; }
    public string arrange_data_id { get; set; }
    public float taskCompletionTime { get; set; }
    public float llmResponseTime { get; set; }
    public TaskScore taskScore { get; set; }
    public LLMPerformanceTest llmPerformanceTest { get; set; }
    
}

[Serializable]
public class TaskScore
{
    public int TP { get; set; }
    public int FP { get; set; }
    public int TN { get; set; }
    public int FN { get; set; }
}


[Serializable]
public class LLMPerformanceTest
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }


    public void CalculatePerformanceTest(TaskScore taskScore)
{
    double total = taskScore.TP + taskScore.TN + taskScore.FP + taskScore.FN;
    Accuracy = total != 0 ? (double)(taskScore.TP + taskScore.TN) / total : 0.0;

    double precisionDenominator = taskScore.TP + taskScore.FP;
    Precision = precisionDenominator != 0 ? (double)taskScore.TP / precisionDenominator : 0.0;

    double recallDenominator = taskScore.TP + taskScore.FN;
    Recall = recallDenominator != 0 ? (double)taskScore.TP / recallDenominator : 0.0;

    double  f1Denominator = Precision + Recall;
    F1Score = f1Denominator != 0 ? 2 * (Precision * Recall) / f1Denominator : 0.0;
}

    public void CalculatePerformanceTest(int TP, int FP, int TN, int FN)
    {
        Accuracy = (TP + TN) / (TP + TN + FP + FN);
        Precision = TP / (TP + FP);
        Recall = TP / (TP + FN);
        F1Score = 2 * (Precision * Recall) / (Precision + Recall);
    }


    public void SetTestData(float accuracy, float precision, float recall, float f1Score)
    {
        Accuracy = accuracy;
        Precision = precision;
        Recall = recall;
        F1Score = f1Score;
    }
}


public class ExperimentTask : MonoBehaviour
{
    
    [SerializeField] private SAWebsocket saWebsocket; 
 
    private ExperimentalDataManager experimentalDataManager;
    
   private bool isTaskTimerStarted = false;
    private bool isLLMTimerStarted = false;
    private float llmResponseTime = 0f; 
    private float taskCompletionTime = 0f; 

    private string currentTaskId; 
    private string deviceArrangeId; 
    private string task_name;
    private string user_name; 

    private DeviceArrangementGenerator arrangementGenerator;




    

    public ExperimentTaskData GetExperimentTaskData()
    {
        ExperimentTaskData taskData = new ExperimentTaskData();
        taskData.user_name = this.user_name;
        taskData.taskId = this.currentTaskId;
        taskData.taskCompletionTime = this.taskCompletionTime;
        taskData.llmResponseTime = this.llmResponseTime;
        taskData.taskScore = this.taskScore;
        taskData.llmPerformanceTest = this.llmPerformanceTest;
        taskData.arrange_data_id = this.deviceArrangeId;
        return taskData;
    }
    
    public void Initialize (DeviceArrangeData task)
    {
        this.ClearAllData();
        this.user_name=this.experimentalDataManager.GetUserName();
        this.deviceArrangeId = task.device_arrange_id; 
        this.currentTaskId = Guid.NewGuid().ToString();
    }


    void Start()
    {   

        experimentalDataManager = GetComponent<ExperimentalDataManager>();



    }



private TaskScore taskScore = new TaskScore(){
    TP = 0,
    FP = 0,
    TN = 0,
    FN = 0
};

   private LLMPerformanceTest llmPerformanceTest = new LLMPerformanceTest(){
    Accuracy = 0f,
    Precision = 0f,
    Recall = 0f,
    F1Score = 0f
   };

    private void Update() {
        if(isTaskTimerStarted)
        {
            taskCompletionTime += Time.deltaTime;
        }


        if(isLLMTimerStarted)
        {
              llmResponseTime += Time.deltaTime;
        }
    }


    public void StartTaskTimer()
    {
        isTaskTimerStarted = true;       
    }


    public void StopTaskTimer()
    {
        isTaskTimerStarted = false;

    }


    public void StartLLMTimer()
    {
        isLLMTimerStarted = true;       
    }

    public void StopLLMTimer()
    {
        isLLMTimerStarted = false;
    }



    public void CalculateScores(DeviceArrangeData arrangeData) 
{


    
    // arrangeData.devices は「オンになってほしい」デバイスのリストと仮定
    List<SADevice> desiredDevices = arrangeData.devices.Select(x => x.device).ToList();
    
    List<SADevice> allDevices = SADeviceRef.Instance.GetAllDevices();

    

    
    // allDevices 内の各デバイスについて、期待する状態と実際の状態を比較
    foreach (SADevice device in allDevices)
    {
        // desiredDevices に含まれているなら、デバイスはオンであるべき
        bool shouldBeOn = desiredDevices.Contains(device);
        bool isOn = device.IsDeviceOn; // SADevice のプロパティ
        
        Debug.Log($"<color=yellow>{device.name}, {shouldBeOn}, {isOn}</color>");
        if (shouldBeOn && isOn)
        {
            // 期待通りオンになっている: True Positive
            taskScore.TP++;
        }
        else if (shouldBeOn && !isOn)
        {
            // オンであるべきなのにオフになっている: False Negative
            taskScore.FN++;
        }
        else if (!shouldBeOn && isOn)
        {
            // オンにする必要がないのにオンになっている: False Positive
            taskScore.FP++;
        }
        else // (!shouldBeOn && !isOn)
        {
            // 正しくオフになっている: True Negative
            taskScore.TN++;
        }
    }


    llmPerformanceTest.CalculatePerformanceTest(taskScore);
}




    public void ClearAllData() 
    {
        taskCompletionTime = 0f; 
        llmResponseTime = 0f; 
        taskScore.TP = 0;
        taskScore.FP = 0;
        taskScore.TN = 0;
        taskScore.FN = 0;
        llmPerformanceTest.Accuracy = 0f;
        llmPerformanceTest.Precision = 0f;
        llmPerformanceTest.Recall = 0f;
        llmPerformanceTest.F1Score = 0f;

        
    }


}
}