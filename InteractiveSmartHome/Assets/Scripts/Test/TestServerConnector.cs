
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ActionDataTypes;
using MRFlow.Component;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodeTypes;
using Unity.VisualScripting;
using UnityEngine;
using MRFlow.ServerController; 



namespace MRFlow.Test 
{
public class TestServerConnector : Singleton<TestServerConnector>
{

      
    bool debug = false;

   public async Task<List<MRNodeData>> GetMRNodeDatas() 
   {
     List<DBNode> DBNodes = await ActionServerConnector.Instance.GetAllNodes();
       
    List<MRNodeData> mrNodeDatas = await DBNodeToMRNodeData(DBNodes);


    return mrNodeDatas;

   }


    private async Task<List<MRNodeData>> DBNodeToMRNodeData(List<DBNode> dbNodes)
    {
        List<MRNodeData> mrNodeDatas = new List<MRNodeData>();

        foreach (DBNode dbNode in dbNodes)
        {   
            // actionBlockのオブジェクトを取得する
            var actionBlock = await ActionServerConnector.Instance.GetAction(dbNode.data_action_id);

            // actionBlockのオブジェクトからactionTypeを取得する
            string actionType = actionBlock["action_type"].ToString();
            ActionBlockType actionBlockType = BlockActionTypeMap.GetStringTypeFromActionType(actionType);

            // ActionBlockTypeから正確な型を取得する
            Type dataCastType = BlockActionTypeMap.GetActionDataType(actionBlockType);

            // デシリアライズ前に確認のために出力
            Debug.Log("Deserializing with type: " + dataCastType);

            // 正しい型でデシリアライズを実行
            var actionData = JsonConvert.DeserializeObject(actionBlock.ToString(), dataCastType) as IActionBlock;

            if (actionData == null)
            {
                Console.WriteLine("Failed to deserialize actionBlock to expected type.");
                continue; // 次のdbNodeに移動
            }

            // actionDataの内容を出力して確認
            Debug.Log("Deserialized actionData: " + actionData);

            // MRNodeDataを作成
            MRNodeData mRNodeData = new MRNodeData(
                new Guid(dbNode.id),
                dbNode.type,
                actionData,
                dbNode.mr_position
            );

            mrNodeDatas.Add(mRNodeData);
        }

        return mrNodeDatas;
    }


   

    public async Task  TestAddNode(MRNodeData mRNodeData)
    {   
         if(debug)return;
            JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        Debug.Log("TestUploadNode");
        string json = JsonConvert.SerializeObject(mRNodeData, settings);
        Debug.Log($"<color=yellow>mRNodeData.id: {json}</color>");
        DBNode dbNode = new DBNode(
            mRNodeData.id.ToString(),
            mRNodeData.type,
            mRNodeData.action_data.id.ToString(),
            mRNodeData.position
        );
      

      IActionBlock actionBlock = mRNodeData.action_data; 
    //   await ActionServerConnector.Instance.AddActionBlock(actionBlock);
    //   await ActionServerConnector.Instance.AddNode(dbNode);
    
        
    }

    public async void TestRemoveNode(MRNodeData mRNodeData)
    {
        if(debug)return;
        
       await ActionServerConnector.Instance.DeleteNode(mRNodeData.id);
       await ActionServerConnector.Instance.DeleteActionBlock(mRNodeData.action_data.id);
    }


    private Guid ConvertFromStringToGuid(string id )
    {
        return new Guid(id);
    }

    private List<Guid> ConvertFromStringToGuidList(string[] ids)
    {
        List<Guid> guidList = new List<Guid>();
        foreach (string id in ids)
        {
            guidList.Add(new Guid(id));
        }
        return guidList;
    }
    
}
}