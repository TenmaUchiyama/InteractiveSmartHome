using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionDataTypes;
using MRFlow.Component;
using MRFlow.Core;
using Newtonsoft.Json;
using NodeTypes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



namespace MRFlow.Network{
public class ActionServerController : Singleton<ActionServerController>
{


  [SerializeField] ActionServerConnector actionServerConnector;
  public async Task<List<MRNodeData>> GetMRNodeDatas() 
   {
     List<DBNode> DBNodes = await actionServerConnector.GetAllNodes();
   
    
    List<MRNodeData> mrNodeDatas = await DBNodeToMRNodeData(DBNodes);


    return mrNodeDatas;

   }


   public async Task<RoutineData> GetRoutineData(Guid routineId)
   {
         RoutineData routineData = await actionServerConnector.GetRoutine(routineId.ToString());
         return routineData;
   }

   public async Task CreateNewRoutineDataFromNewRoutineEdge(MRRoutineEdgeData mrRtoutineEdgeData)
   {
       RoutineData newRoutineData = new RoutineData(
           mrRtoutineEdgeData.routine_id,
              mrRtoutineEdgeData.routine_name,
              new List<Routine>()
       );
       await actionServerConnector.AddNewRoutineDataFromNewRoutineEdge(newRoutineData);
   }


// DBからのデータをMRNodeDataに変換する
 private async Task<List<MRNodeData>> DBNodeToMRNodeData(List<DBNode> dbNodes)
    {

        
    int c = dbNodes.Count; 
    Debug.Log($"<color=red>DBNodes {c}</color>");
        List<MRNodeData> mrNodeDatas = new List<MRNodeData>();

        foreach (DBNode dbNode in dbNodes)
        {   
            // actionBlockのオブジェクトを取得する
            var actionBlock = await actionServerConnector.GetAction(dbNode.data_action_id);

            // actionBlockのオブジェクトからactionTypeを取得する
            string actionType = actionBlock["action_type"].ToString();

            // action_typeの文字列からActionBlockTypeを取得する
            ActionBlockType actionBlockType = BlockActionTypeMap.GetStringTypeFromActionType(actionType);

            // ActionBlockTypeから正確な型を取得する
            Type dataCastType = BlockActionTypeMap.GetActionDataType(actionBlockType);

            // デシリアライズ前に確認のために出力
            if(dataCastType == null)
            {
                Debug.LogError("Failed to get dataCastType from actionBlockType: " + actionBlockType);
                
            }{
            Debug.Log($"[Action Server Controller] Deserializing with type: {dataCastType} Data: {actionBlock.ToString()}");
            }
            // 正しい型でデシリアライズを実行
            var actionData = JsonConvert.DeserializeObject(actionBlock.ToString(), dataCastType);


            

            if (actionData == null)
            {
                Debug.LogError("Failed to deserialize actionBlock to expected type.");
                continue; // 次のdbNodeに移動
            }

            // actionDataの内容を出力して確認
            Debug.Log("Deserialized actionData: " + actionData);


            Vector3 spawnMRNodePosition; 

            if(dbNode.mr_position != null)
            {
                spawnMRNodePosition = dbNode.mr_position;
            }else{
                spawnMRNodePosition = new Vector3(0,0,0);
            }

            // MRNodeDataを作成
            MRNodeData mRNodeData = new MRNodeData(
                new Guid(dbNode.id),
                dbNode.type,
                actionData as IActionBlock,
                spawnMRNodePosition
            );

           
            mrNodeDatas.Add(mRNodeData);
        }

        return mrNodeDatas;
    }


   public async Task AddNodes(List<MRNodeData> mRNodeDatas)
   {
       JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        List<IActionBlock> actionBlocks = new List<IActionBlock>();
        List<DBNode> dbNodes = new List<DBNode>();
        foreach (MRNodeData mRNodeData in mRNodeDatas)
        {


            
            string json = JsonConvert.SerializeObject(mRNodeData, settings);
            Debug.Log($"<color=yellow>Adding Node: {json}</color>");
            DBNode dbNode = new DBNode(
                mRNodeData.id.ToString(),
                mRNodeData.type,
                mRNodeData.action_data.id.ToString(),
                mRNodeData.position
            );

            IActionBlock actionBlock = mRNodeData.action_data; 
            actionBlocks.Add(actionBlock);
            dbNodes.Add(dbNode);
        }

        await actionServerConnector.AddNode(dbNodes);
        await actionServerConnector.AddActionBlock(actionBlocks);
      

      
    
   }

   public async Task RemoveNode(MRNodeData mRNodeData)
    {
        
       await actionServerConnector.DeleteNode(mRNodeData.id);
       await actionServerConnector.DeleteActionBlock(mRNodeData.action_data.id);
    }
    



   public async Task AddRoutineEdge(MRRoutineEdgeData  mrRoutineEdgeData)
   {

     DBRoutineEdge dbRoutineEdge = new DBRoutineEdge(
            mrRoutineEdgeData.id.ToString(),
            mrRoutineEdgeData.routine_id.ToString(),
            mrRoutineEdgeData.routine_name,
            mrRoutineEdgeData.edges.Select(edge => CreateDBEdgeFromMREdgeData(edge)).ToArray(),
            mrRoutineEdgeData.nodes.Select(nodeId => nodeId.ToString()).ToArray()
        );

        await actionServerConnector.AddRoutineEdge(dbRoutineEdge);
   }


   public async Task<List<MRRoutineEdgeData>> GetMRRoutineEdgeData()
   {
        List<DBRoutineEdge> dBRoutineEdges = await actionServerConnector.GetAllRoutineEdges();
        

        List<MRRoutineEdgeData> mrRoutineEdgeDatas = dBRoutineEdges.Select(dbEdge => {
            List<MREdgeData> edges = dbEdge.edges.Select(edge => new MREdgeData(
                edge.id,
                new Guid(edge.source),
                new Guid(edge.target)
            )).ToList();

            List<Guid> nodes = dbEdge.nodes.Select(nodeId => new Guid(nodeId)).ToList();
            List<MREdgeData> edgeDatas = dbEdge.edges.Select(edge => new MREdgeData(
                edge.id,
                new Guid(edge.source),
                new Guid(edge.target)
            )).ToList();

            return new MRRoutineEdgeData(
                new Guid(dbEdge.id),
                new Guid(dbEdge.associated_routine_id), 
                dbEdge.routine_name,
                nodes,
                edgeDatas
            );

        }).ToList();

        return mrRoutineEdgeDatas;
        
            
   }


   public async Task UpdateRoutineEdge(MRRoutineEdgeData mrRoutineEdgeData)
   {
       List<DBEdge> dbEdges = mrRoutineEdgeData.edges
        .Select(edge => CreateDBEdgeFromMREdgeData(edge))
        .ToList();

    List<MRNode> allNodes = NodeManager.Instance.NodeList; 

    List<DBNode> dbNodes = mrRoutineEdgeData.nodes
        .Select(nodeId => allNodes.Find(node => node.GetMRNodeData().id == nodeId))
        .Where(mrNode => mrNode != null)  // ノードが見つからなかった場合を除外
        .Select(mrNode => CreateDBNodeFromMRNodeData(mrNode.GetMRNodeData()))
        .ToList();
        

        DBRoutineEdge dbRoutineEdge = new DBRoutineEdge(
            mrRoutineEdgeData.id.ToString(),
            mrRoutineEdgeData.routine_id.ToString(),
            mrRoutineEdgeData.routine_name,
            dbEdges.ToArray(),
            mrRoutineEdgeData.nodes.Select(nodeId => nodeId.ToString()).ToArray()
        );

        await actionServerConnector.UpdateRoutineEdge(dbRoutineEdge);
   }


   public async Task UpdateNodeListByNodeData(List<MRNodeData> mrNodeDatas)
   {
        List<DBNode> dbNodes = mrNodeDatas.Select(mrNodeData => CreateDBNodeFromMRNodeData(mrNodeData)).ToList();
        await actionServerConnector.UpdateMultipleNodes(dbNodes);
   }
   public async Task UpdateNodeListById(List<Guid> nodeIds)
   {

        List<IActionBlock> actionBlocks = new List<IActionBlock>();
        List<MRNodeData> nodes = NodeManager.Instance.NodeList.Select(mrNode => mrNode.GetMRNodeData()).ToList(); 

        // nodeIdsに一致するMRNodeDataを抽出
        List<MRNodeData> matchedNodes = nodes
            .Where(mrNodeData => nodeIds.Contains(mrNodeData.id))
            .ToList();

        // DBNodeのリストを生成
        List<DBNode> dbNodes = matchedNodes
            .Select(mrNodeData => {
                actionBlocks.Add(mrNodeData.action_data);
                return ActionServerController.CreateDBNodeFromMRNodeData(mrNodeData);
            })
            .ToList();



        string json = JsonConvert.SerializeObject(actionBlocks);
        Debug.Log($"<color=yellow>UpdateNodeList: {json}</color>");

       await actionServerConnector.UpdateMultipleNodes(dbNodes);
   }



   public static DBEdge CreateDBEdgeFromMREdgeData(MREdgeData mrEdgeData)
   {
       return new DBEdge(
           mrEdgeData.id,
           mrEdgeData.node_out.ToString(),
           mrEdgeData.node_in.ToString()
       );
   }

   public static DBNode CreateDBNodeFromMRNodeData(MRNodeData mrNodeData)
   {
       return new DBNode(
           mrNodeData.id.ToString(),
           mrNodeData.type,
           mrNodeData.action_data.id.ToString(),
           mrNodeData.position
       );
   }


   public async Task UpdateRoutineData(RoutineData routineData)
   {
    await actionServerConnector.UpdateRoutineData(routineData);
    await UpdateRoutineById(routineData.action_routine, routineData.id.ToString());
   }


   public async Task UpdateRoutineById(List<Routine> routine,string routineId)
   {
    await actionServerConnector.UpdateRoutineById(routine,routineId);
   }

    public async Task StartRoutine(Guid routineId)
    {
        await actionServerConnector.StartRoutine(routineId.ToString());
    }
}
}