using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionDataTypes;
using MRFlow.Component;
using MRFlow.Core;
using MRFlow.Network;
using MRFlow.Test;
using Newtonsoft.Json;
using NodeTypes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;



namespace MRFlow.Core{
public class RoutineEdgeManager : Singleton<RoutineEdgeManager>
{

    private List<MRNodeData> entireMRNodeData = new List<MRNodeData>();
    private List<MRRoutineEdgeData> routineEdgeDatas = new List<MRRoutineEdgeData>();
    private List<Routine> routines = new List<Routine>(); 


    public List<MRRoutineEdgeData> GetMRROutineEdgeDatas()
    {
        return routineEdgeDatas;
    }


    private void Start() {
        MRMqttController.Instance.OnConnectionCompleted += InitRoutineEdge;
    }
    void OnDestroy()
    {
        MRMqttController.Instance.OnConnectionCompleted -= InitRoutineEdge;
    }
    private async void InitRoutineEdge() {    

    

            //　RoutineEdgeの初期化。データベースが空の場合は新規作成、そうでなければ既存のデータを取得する。
            Debug.Log("<color=yellow>[RoutineEdgeManager] Start</color>");
            
           routineEdgeDatas = await ActionServerController.Instance.GetMRRoutineEdgeData();

            MRRoutineEdgeData currentMRRoutineEdgeData = null;
            if(routineEdgeDatas.Count == 0){
                Debug.Log("<color=yellow>[RoutineEdgeManager] Routine Edge Data Does Not Exist</color>");
                currentMRRoutineEdgeData = new MRRoutineEdgeData(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "unity-routine-test",
                new List<Guid>(), 
                new List<MREdgeData>()
            ); 


            await ActionServerController.Instance.AddRoutineEdge(currentMRRoutineEdgeData);

            }else{
                Debug.Log("<color=yellow>[EdgeManager] Routine Edge Data Exists</color>");
                currentMRRoutineEdgeData = routineEdgeDatas[0];
            }

        



            RoutineData routineData = await ActionServerController.Instance.GetRoutineData(currentMRRoutineEdgeData.routine_id);
            


            // Generate Routine From Routine Edge
            if(routineData == null)
            {
                await ActionServerController.Instance.CreateNewRoutineDataFromNewRoutineEdge(currentMRRoutineEdgeData);
            }else{
            // if routine already exists, it updates the data
                await ActionServerController.Instance.UpdateRoutineData(routineData);
            }


        
            EdgeManager.Instance.SetCurrentMRRoutineEdgeData(currentMRRoutineEdgeData);
            
            




            // ノードデータを取得する。
            List<MRNodeData> mrNodeDatas = await ActionServerController.Instance.GetMRNodeDatas();
            entireMRNodeData = mrNodeDatas;
            
            int coun = entireMRNodeData.Count; 
            Debug.Log($"<color=red>{coun}</color>");

            List<Guid> currentNodes = currentMRRoutineEdgeData.nodes;
           
            List<MRNodeData> nodes = mrNodeDatas.Where(node =>{
                if(currentNodes.Contains(node.id))
                {
                    return true; 
                    
                }else{
                    Guid id = node.id;
                    Debug.Log($"<color=red>{id}</color>");
                    return false;
                }

            }).ToList();
            List<MRNode> mrNode = NodeManager.Instance.SpawnNodes(nodes);

 
            // //　ノード間でエッジを作成する。
            List<MREdgeData> edges = currentMRRoutineEdgeData.edges;

            string jsonify = JsonConvert.SerializeObject(edges, settings);
            string nodeJ = JsonConvert.SerializeObject(nodes, settings); 
            
            ConnectEdges(edges, mrNode);      

    }
   JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
             NullValueHandling = NullValueHandling.Ignore
        };


    // Update Node, Routine, RoutineEdge then start the routine
    public async void StartRoutine(MRRoutineEdgeData mrRoutineEdgeData)
    {
    
        


        await UpdateRoutine(mrRoutineEdgeData);
        await ActionServerController.Instance.StartRoutine(mrRoutineEdgeData.routine_id);

        
    }
    public async Task UpdateRoutine(MRRoutineEdgeData mrRoutineEdgeData)
    {

// ノードをIDからUpdateする。
        List<Guid> nodeIds = mrRoutineEdgeData.nodes;


        // EdgeからRoutineを作成する。
        List<MREdgeData> edges = mrRoutineEdgeData.edges;
        List<MRNodeData> nodes = NodeManager.Instance.GetMRNodeDataFromNodeIds(nodeIds); 
        List<Routine> routines = ConvertEdgesToRoutines(edges, nodes);
        RoutineData routineData = new RoutineData(
            mrRoutineEdgeData.routine_id, 
            mrRoutineEdgeData.routine_name,
            routines  
        );


        string jsonify = JsonConvert.SerializeObject(routines, settings);
        Debug.Log($"<color=yellow>Routines: {jsonify}</color>");
        await NodeManager.Instance.UpdateNodeListDBById(nodeIds);
        await ActionServerController.Instance.UpdateRoutineData(routineData);
        await ActionServerController.Instance.UpdateRoutineEdge(mrRoutineEdgeData);
        
    }


public async void TestUpdateRoutineEdge() 
{
    MRRoutineEdgeData routineEdge = EdgeManager.Instance.GetCurrentRoutineEdge(); 
    List<MRNodeData> nodeDatas = NodeManager.Instance.GetMRNodeDataFromNodeIds(routineEdge.nodes);
    string jsonify = JsonConvert.SerializeObject(nodeDatas, settings);
    List<Routine> routines = ConvertEdgesToRoutines(routineEdge.edges,nodeDatas);
    await ActionServerController.Instance.UpdateRoutineEdge(routineEdge);
    await ActionServerController.Instance.UpdateRoutineById(routines, routineEdge.routine_id.ToString());
    
}
private  List<Routine> ConvertEdgesToRoutines(List<MREdgeData> edges, List<MRNodeData> nodes)
{


      Dictionary<Guid, Guid> nodeIdToActionId = new Dictionary<Guid,Guid>();

        foreach (MRNodeData node in nodes)
        {
            nodeIdToActionId.Add(node.id, node.action_data.id);
        }

        List<Guid> nodeInIDs = edges.Select(edge => edge.node_in).ToList();
        List<Guid> nodeOutIDs = edges.Select(edge => edge.node_out).ToList();
        List<Guid> nodeInIDset = new HashSet<Guid>(nodeInIDs).ToList();
        List<Guid> nodeOutIDset = new HashSet<Guid>(nodeOutIDs).ToList();




        List<Guid> firstNodeIds = nodeOutIDset.Where(id => !nodeInIDset.Contains(id)).ToList();
        List<Guid> lastNodeIds = nodeInIDset.Where(id => !nodeOutIDset.Contains(id)).ToList();

        List<Routine> routines = edges.Select(edge => {
            Guid currentActionid = nodeIdToActionId.ContainsKey(edge.node_out) ? nodeIdToActionId[edge.node_out] : Guid.Empty;
            Guid nextActionid = nodeIdToActionId.ContainsKey(edge.node_in) ? nodeIdToActionId[edge.node_in] : Guid.Empty;


            Routine routine = new Routine(
                currentActionid,
                nextActionid 
            ); 

            if(firstNodeIds.Contains(edge.node_out))
            {
                routine.first = true; 
            };
            return routine; 
        }).ToList();


        foreach (Guid lastNodeId in lastNodeIds)
        {
            Guid currentActionid = nodeIdToActionId.ContainsKey(lastNodeId) ? nodeIdToActionId[lastNodeId] : Guid.Empty;
            Routine existingRoutine = routines.FirstOrDefault(routine => routine.current_block_id == currentActionid && routine.next_block_id == Guid.Empty);
            if(existingRoutine != null)
            {
                existingRoutine.last = true; 
            }
            else
            {
                Routine routine = new Routine(
                    currentActionid,
                    Guid.Empty
                ); 
                routine.last = true; 
                routines.Add(routine); 
            }
        }

        return routines.ToList();
}


private void ConnectEdges(List<MREdgeData> edges, List<MRNode> nodes)
{
    var edgesCopy = new List<MREdgeData>(edges); // コピーを作成
    foreach (MREdgeData edge in edgesCopy)
    {
        MRNode nodeIn = nodes.Find(node => node.GetMRNodeData().id.Equals(edge.node_in));
        MRNode nodeOut = nodes.Find(node => node.GetMRNodeData().id.Equals(edge.node_out));
        NodeHandler nodeHandlerIn = nodeIn.GetNodeHandler(HandlerType.HANDLER_IN); 
        NodeHandler nodeHandlerOut = nodeOut.GetNodeHandler(HandlerType.HANDLER_OUT);

     
        bool isEitherNull = nodeHandlerIn == null || nodeHandlerOut == null;
        if (isEitherNull) 
        { 
            Debug.LogError($"<color=red>NodeHandlerIn: {nodeHandlerIn} NodeHandlerOut: {nodeHandlerOut}</color>"); 
            return;
        }
        EdgeManager.Instance.CreateEdge(nodeHandlerOut, nodeHandlerIn, edge);
    }
}



private void DebugLog(string prefix, object anything)
{
    string json = JsonConvert.SerializeObject(anything, settings);
    Debug.Log($"<color=yellow>{prefix} {json}</color>");
}
}

}