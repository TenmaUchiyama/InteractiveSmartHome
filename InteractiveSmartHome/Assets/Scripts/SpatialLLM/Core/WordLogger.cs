using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SpatialLLM.Experiment;
using UnityEngine;


public class WordLogger : MonoBehaviour
{
    [SerializeField] private string folderRoot = "VOICE_LOG";

    private List<ExperimentTaskDataForSave> taskDataList = new List<ExperimentTaskDataForSave>();
    private string filePath;

    void Start()
    {
        UpdateFilePathFromEXDataHolder();
        LoadIfExists();
    }







    public void SetTotalTime(float seconds)
    {
        this.totalTimeSeconds = seconds;
        SaveToFile(); // ここで保存
    }




    
    private void UpdateFilePathFromEXDataHolder()
    {
        string participant = EXDataHolder.Instance.ParticipantName;
        string condition = EXDataHolder.Instance.ConditionName;

        string directoryPath = Path.Combine(Application.dataPath, "EXPERIMENT", folderRoot, participant);
        Directory.CreateDirectory(directoryPath);

        filePath = Path.Combine(directoryPath, $"{condition}_{EXDataHolder.Instance.TaskSetName}.json");
    }

    public void AddOrUpdateTaskData(ExperimentTaskDataForSave newData)
    {
        UpdateFilePathFromEXDataHolder();
        LoadIfExists();

        var existing = taskDataList.Find(d => d.taskId == newData.taskId);
        if (existing != null)
        {
            existing.taskAttempts.AddRange(newData.taskAttempts);
            existing.taskAttemptCount = existing.taskAttempts.Count;
            existing.finalId = newData.finalId;
        }
        else
        {
            taskDataList.Add(newData);
        }

        SaveToFile();
    }

    private void LoadIfExists()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                taskDataList = JsonConvert.DeserializeObject<List<ExperimentTaskDataForSave>>(json) ?? new List<ExperimentTaskDataForSave>();
                Debug.Log($"Loaded {taskDataList.Count} task entries from {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to load task entries: " + e.Message);
                taskDataList = new List<ExperimentTaskDataForSave>();
            }
        }
        else
        {
            taskDataList = new List<ExperimentTaskDataForSave>();
        }
    }

    private void SaveToFile()
    {
        try
        {
            string json = JsonConvert.SerializeObject(taskDataList, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Debug.Log($"Saved task entries to {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save task entries: " + e.Message);
        }
    }
}
