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
using System;
using System.Linq;


namespace MRFlow.ServerController
{
public class ActionServerConnector : Singleton<ActionServerConnector>
{
     private  string baseUrl = "http://localhost:4049";
    private  string deviceUrl = "/device";
    private  string actionUrl = "/action";
    private  string routineUrl = "/routine";
    private  string nodeUrl = "/flow-node";
    private  string edgeUrl = "/flow-edge";


    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private void Awake()
    {
       

        deviceUrl = baseUrl + deviceUrl;
        actionUrl = baseUrl + actionUrl ;
        routineUrl = baseUrl + routineUrl;
        nodeUrl = baseUrl + nodeUrl;
        edgeUrl = baseUrl + edgeUrl;
    }

#region Commands

    public async Task StartRoutine(string routineId)
    {
        string url = routineUrl + "/start/" + routineId;
        Debug.Log($"<color=red>Routine ID: {url}</color>");
        string result = await GetRequest(url);
        Debug.Log($"<color=purple>StartRoutine: {result}</color>");
    }
#endregion


#region Getters


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

public async Task<List<JObject>> GetAllActions()
{
    string url = actionUrl + "/get-all";
    string result = await GetRequest(url);
    List<JObject> actionDatas = JsonConvert.DeserializeObject<List<JObject>>(result);
    return actionDatas;
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



public async Task<List<DBRoutineEdge>> GetAllRoutineEdges()
{
    string url = edgeUrl + "/get-all";
    string result = await GetRequest(url);

  
    List<DBRoutineEdge> edgeDatas = JsonConvert.DeserializeObject<List<DBRoutineEdge>>(result);

    return edgeDatas;
}

#endregion



#region Adders

public async Task AddRoutineEdge(DBRoutineEdge edgeData)
{
    string jsonEdge = JsonConvert.SerializeObject(edgeData,settings);
    string edgeUrl = this.edgeUrl + "/add";

    string result = await PostRequest(edgeUrl, jsonEdge);
}
public async Task AddActionBlock(List<IActionBlock> actionData)
{
    
    string jsonify = JsonConvert.SerializeObject(actionData,settings);
    string url = actionUrl + "/add";
    string result = await PostRequest(url, jsonify);
    
}

public async Task AddNode(List<DBNode> nodeData)
{
    
    string jsonNode = JsonConvert.SerializeObject(nodeData,settings);
    string nodeUrl = this.nodeUrl + "/add";

    string result = await PostRequest(nodeUrl, jsonNode);
    
}

public async Task AddEdge(DBEdge dbEdge)
{
    
    string jsonEdge = JsonConvert.SerializeObject(dbEdge,settings);
    string edgeUrl = this.edgeUrl + "/add";

    string result = await PostRequest(edgeUrl, jsonEdge);
}


public async Task AddNewRoutineDataFromNewRoutineEdge(RoutineData routineData)
{
    string jsonRoutine = JsonConvert.SerializeObject(routineData,settings);
    string routineUrl = this.routineUrl + "/add";

    string result = await PostRequest(routineUrl, jsonRoutine);
}

#endregion


#region Updaters

public async Task UpdateNode(DBNode nodeData)
{
    string jsonNode = JsonConvert.SerializeObject(nodeData,settings);
    string nodeUrl = this.nodeUrl + "/update";

    string result = await PostRequest(nodeUrl, jsonNode);
}

public async Task UpdateRoutineData(RoutineData routineData)
{

    JsonSerializerSettings  settings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore
    };
    string jsonRoutine = JsonConvert.SerializeObject(routineData,settings);
    string routineUrl = this.routineUrl + "/update";

    string result = await PutRequest(routineUrl, jsonRoutine);
}

public async Task UpdateRoutineById(List<Routine> routine, string routineId)
{
    string routineUrl = $"{this.routineUrl}/update/{routineId}";
    JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
        string jsonRoutine = JsonConvert.SerializeObject(routine, settings);


        string result = await PutRequest(routineUrl, jsonRoutine);
        
        
}

public async Task UpdateActionBlock(IActionBlock actionData)
{
    string jsonify = JsonConvert.SerializeObject(actionData,settings);
    string url = actionUrl + "/update";
    string result = await PostRequest(url, jsonify);
}


public async Task UpdateMultipleNodes(List<DBNode> nodeDatas)
{
    string url = nodeUrl + "/update";
    string jsonNodes = JsonConvert.SerializeObject(nodeDatas,settings);
    string result = await PutRequest(url, jsonNodes);
}   


public async Task UpdateMultipleActions(List<IActionBlock> actionDatas)
{
    string url = actionUrl + "/update";
    string jsonActions = JsonConvert.SerializeObject(actionDatas,settings);
    string result = await PutRequest(url, jsonActions);
}


public async Task UpdateRoutineEdge(DBRoutineEdge edgeData)
{
    string jsonEdge = JsonConvert.SerializeObject(edgeData,settings);
    string edgeUrl = this.edgeUrl + "/update";
    string result = await PutRequest(edgeUrl, jsonEdge);
}


#endregion

#region Deleters

public async Task DeleteNode(Guid id)
{
    string url = nodeUrl + "/delete/";
    var payload = new { ids = new[] { id.ToString() } };
    string jsonBody = JsonConvert.SerializeObject(payload);

    Debug.Log($"<color=yellow>Node jsonBody: {jsonBody}</color>");
    
    string result = await DeleteRequest(url, jsonBody);
}


public async Task DeleteActionBlock(Guid id)
{
    string url = actionUrl + "/delete/";
    Debug.Log($"URL: {url}");
     var payload = new { ids = new[] { id.ToString() } };
    string jsonBody = JsonConvert.SerializeObject(payload);

    Debug.Log($"<color=yellow>Node jsonBody: {jsonBody}</color>");
 
    string result = await DeleteRequest(url, jsonBody);
}


#endregion


#region Requesters

    private async Task<string> GetRequest(string url)
        {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                string result = HandleResponse(request);
                return result;
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

                string result = HandleResponse(request);
                return result;
              
            }
        }

public async Task<string> PutRequest(string url, string jsonBody, Dictionary<string, string> queryParams = null)
{
    // クエリパラメータが存在する場合、URLに追加
    if (queryParams != null && queryParams.Count > 0)
    {
        var queryString = new StringBuilder("?");
        foreach (var param in queryParams)
        {
            queryString.Append($"{UnityWebRequest.EscapeURL(param.Key)}={UnityWebRequest.EscapeURL(param.Value)}&");
        }
        // 最後の & を削除
        queryString.Length -= 1;
        url += queryString.ToString();
    }

    using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
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

        string result = HandleResponse(request);
        return result;
    }
}

    private async Task<string> DeleteRequest(string url, string jsonBody)
        {
            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                string result = HandleResponse(request);
                return result;

            }
        }

private string HandleResponse(UnityWebRequest request)
{
    if (request.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError($"Error: {request.error} (Response Code: {request.responseCode})");
        return null; // エラー時にはnullを返す
    }
    else
    {
        // downloadHandlerやdataがnullでないことを確認してから取得
        if (request.downloadHandler != null && request.downloadHandler.data != null)
        {
            string result = Encoding.UTF8.GetString(request.downloadHandler.data);


            if (string.IsNullOrEmpty(result) || result == "null")
            {
                Debug.LogWarning("Server returned null or empty response.");
                return null; 
            }


            return result; // 成功時にレスポンスボディを返す
        }
        else
        {
            Debug.LogWarning("No response data received from the server.");
            return ""; // またはnullや特定のメッセージなどを返す
        }
    }
}
  
#endregion
      
      
  }


}

