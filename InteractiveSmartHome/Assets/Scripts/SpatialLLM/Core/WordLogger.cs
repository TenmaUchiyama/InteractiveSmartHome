using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class RecognizedEntry
{
    public string id;
    public string text;
}

public class WordLogger : MonoBehaviour
{
    [SerializeField] private string folderRoot = "VOICE_LOG"; // root folder under EXPERIMENT

    private List<RecognizedEntry> recognizedEntries = new List<RecognizedEntry>();
    private string filePath;

    void Start()
    {
        UpdateFilePathFromEXDataHolder();
        LoadIfExists();
    }

    private void UpdateFilePathFromEXDataHolder()
    {
        string participant = EXDataHolder.Instance.ParticipantName;
        string condition = EXDataHolder.Instance.ConditionName;

        string directoryPath = Path.Combine(Application.dataPath, "EXPERIMENT", folderRoot, participant);
        Directory.CreateDirectory(directoryPath); // なければ作る

        filePath = Path.Combine(directoryPath, $"{condition}_{EXDataHolder.Instance.TaskSetName}.json");
    }

    public void AddRecognizedEntry(string taskId, string recognizedText)
    {
        Debug.Log($"Adding recognized entry: {taskId} - {recognizedText}");
        if (string.IsNullOrWhiteSpace(recognizedText)) return;

        // 毎回ファイルパスを確認（途中で参加者や条件が変わっていた場合にも対応）
        UpdateFilePathFromEXDataHolder();
        LoadIfExists(); // 再読込してマージ（既存のJSONを保持）

        var entry = new RecognizedEntry
        {
            id = taskId,
            text = recognizedText
        };

        recognizedEntries.Add(entry);
        SaveToFile();
    }

    private void LoadIfExists()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                recognizedEntries = JsonConvert.DeserializeObject<List<RecognizedEntry>>(json) ?? new List<RecognizedEntry>();
                Debug.Log($"Loaded {recognizedEntries.Count} entries from {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to load recognized entries: " + e.Message);
                recognizedEntries = new List<RecognizedEntry>();
            }
        }
        else
        {
            recognizedEntries = new List<RecognizedEntry>();
        }
    }

    private void SaveToFile()
    {
        try
        {
            string json = JsonConvert.SerializeObject(recognizedEntries, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Debug.Log($"Saved recognized entries to {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save recognized entries: " + e.Message);
        }
    }
}
