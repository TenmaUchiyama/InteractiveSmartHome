using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Resources;
using Newtonsoft.Json;
using ActionDataTypes;
using JetBrains.Annotations;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using NodeTypes;


public class ActionServerConnector : MonoBehaviour
{
     private  string baseUrl = "http://localhost:4049";
    private  string deviceUrl = "/device";
    private  string actionUrl = "/action";
    private  string routineUrl = "/routine";
    private  string nodeUrl = "/flow-node";
    private  string edgeUrl = "/flow-edge";




    public static ActionServerConnector Instance; 
    

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        deviceUrl = baseUrl + deviceUrl;
        actionUrl = baseUrl + actionUrl ;
        routineUrl = baseUrl + routineUrl;
        nodeUrl = baseUrl + nodeUrl;
        edgeUrl = baseUrl + edgeUrl;
    }

public async Task< List<DeviceData>> GetAllDevices()
{
    string url = deviceUrl + "/get-all";
    string result = await GetRequest(url);
    List<DeviceData> deviceDatas = JsonConvert.DeserializeObject< List<DeviceData>>(result);
    return deviceDatas;
}

public async Task<DeviceData> GetDevice(string id)
{
    string url = deviceUrl + "/get/" + id;

    string result = await GetRequest(url);
    DeviceData deviceData = JsonConvert.DeserializeObject<DeviceData>(result);
    return deviceData;
}

public async Task<List<RoutineData>> GetAllRoutine()
{
    string url = routineUrl + "/get-all";
    string result = await GetRequest(url);
    List<RoutineData> routineDatas = JsonConvert.DeserializeObject<List<RoutineData>>(result);
   
    return routineDatas;
}

public async Task<RoutineData> GetRoutine(string id)    
{
    string url = routineUrl + "/get/" + id;
    string result = await GetRequest(url);
    RoutineData routineData = JsonConvert.DeserializeObject<RoutineData>(result);
    return routineData;

}   


public async Task<JObject> GetAction(string id) 
{
   string url = actionUrl + "/get/" + id;
    string result = await GetRequest(url);
object actionData = JsonConvert.DeserializeObject<object>(result);

    return actionData as JObject;
}


public async Task<List<DBNode>> GetAllNodes()
{
    string url = nodeUrl + "/get-all";
    string result = await GetRequest(url);
    List<DBNode> nodeDatas = JsonConvert.DeserializeObject<List<DBNode>>(result);
    return nodeDatas;
}

public async Task<DBNode> GetNode(string id)
{
    string url = nodeUrl + "/get/" + id;
    string result = await GetRequest(url);
    DBNode nodeData = JsonConvert.DeserializeObject<DBNode>(result);
    return nodeData;
}


public async Task<List<DBNode>> GetMultipleNodes(string[] ids)
{
    string url = nodeUrl + "/get";
    string jsonIds = JsonConvert.SerializeObject(ids);
    string result = await PostRequest(url, jsonIds);
    List<DBNode> nodeDatas = JsonConvert.DeserializeObject<List<DBNode>>(result);

    return nodeDatas;
}



public async Task<List<RoutineEdge>> GetAllRoutineEdges()
{
    string url = edgeUrl + "/get-all";
    string result = await GetRequest(url);
    List<RoutineEdge> edgeDatas = JsonConvert.DeserializeObject<List<RoutineEdge>>(result);
    return edgeDatas;
}

// public async Task<Edge> GetRoutineEdge(string id)
// {
//     string url = edgeUrl + "/get/" + id;
//     string result = await GetRequest(url);
//     Edge edgeData = JsonConvert.DeserializeObject<Edge>(result);
//     return edgeData;
// }
 private async Task<string> GetRequest(string url)
    {
      using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error} (Response Code: {request.responseCode})");
                return null; // エラー時にはnullを返す
            }
            else
            {
               
                string result = Encoding.UTF8.GetString(request.downloadHandler.data);
                return result; // 成功時にレスポンスボディを返す
            }
        }
    }
 

 private async Task<string> PostRequest(string url, string jsonBody)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error} (Response Code: {request.responseCode})");
                return null; // エラー時にはnullを返す
            }
            else
            {
                // レスポンスデータを文字列として取得
                string result = request.downloadHandler.text;
                return result; // 成功時にレスポンスボディを返す
            }
        }
    }
}



