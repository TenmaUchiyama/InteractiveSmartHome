using System.Collections;
using System.Collections.Generic;
using UnityEngine;





namespace SpatialLLM.Experiment
{


public class TaskScore
{
    public int TP { get; set; }
    public int FP { get; set; }
    public int TN { get; set; }
    public int FN { get; set; }
}

public class LLMPerformanceTest
{
    public float Accuracy { get; set; }
    public float Precision { get; set; }
    public float Recall { get; set; }
    public float F1Score { get; set; }


    public void CalculatePerformanceTest(TaskScore taskScore)
    {
        Accuracy = (taskScore.TP + taskScore.TN) / (taskScore.TP + taskScore.TN + taskScore.FP + taskScore.FN);
        Precision = taskScore.TP / (taskScore.TP + taskScore.FP);
        Recall = taskScore.TP / (taskScore.TP + taskScore.FN);
        F1Score = 2 * (Precision * Recall) / (Precision + Recall);
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
public class ExperimentalData
{
    public float taskCompletionTime { get; set; }
    public float llmResponseTime { get; set; }
    public Dictionary<string, float> taskScore { get; set; }
    public Dictionary<string, float> llmPerformanceTest { get; set; }
}
public class ExperimentalDataManager : MonoBehaviour
{
    

    private bool isTaskTimerStarted = false;
    private bool isLLMTimerStarted = false;
    private float llmResponseTime = 0f; 
    private float taskCompletionTime = 0f; 


    private Dictionary<string, float> taskScore = new Dictionary<string, float>()
{
    { "TP", 0f }, 
    { "FP", 0f },
    { "TN" , 0f}, 
    { "FN", 0f }
};

   private Dictionary<string,float> llmPerformanceTest = new Dictionary<string, float>()
{
    { "Accuracy", 0f }, 
    { "Precision", 0f },
    { "Recall" , 0f}, 
    { "F1Score", 0f }
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


    
}
}