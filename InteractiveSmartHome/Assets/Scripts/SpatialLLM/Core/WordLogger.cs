using System.Collections;
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
    [SerializeField] private string fileName = "recognized_history.json";
    [SerializeField] private string folderName = "default_folder"; // Default folder name
    private string filePath;


    private List<RecognizedEntry> recognizedEntries = new List<RecognizedEntry>();

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "EXPERIMENT", "VOICE_LOG", folderName, fileName);
        LoadIfExists();
    }




    public void AddRecognizedEntry(string taskId, string recognizedText)
    {

        Debug.Log($"Adding recognized entry: {taskId} - {recognizedText}");
        if (string.IsNullOrWhiteSpace(recognizedText)) return;

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
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to load recognized entries: " + e.Message);
                recognizedEntries = new List<RecognizedEntry>();
            }
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
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

