using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRFlow.Component;
using MRFlow.Network;
using Newtonsoft.Json;
using NodeTypes;
using UnityEngine;



namespace MRFlow.Core{/**
このクラスは、全体のノードの管理を行うクラス
**/
public class NodeManager : Singleton<NodeManager>
{
    [SerializeField] GameObject nodeParent;
    [SerializeField] private NodeObjectMapSO nodeObjectMapSO;



    // 現存するMRNodeのインスタンスのリスト
    private List<MRNode> nodeList = new List<MRNode>();

    public List<MRNode> NodeList => nodeList;

   
     JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };



    
    

    public void AddNode(MRNode node)
    {
        nodeList.Add(node);
    }

    public void RemoveNode(MRNode node)
    {
        nodeList.Remove(node);
        node.DestroyNode();
        
    }


    public List<MRNodeData> GetMRNodeDataFromNodeIds(List<Guid> nodeIds)
    {
        List<Guid> guids = new List<Guid>(); 
        foreach(MRNode node in nodeList)
        {
            guids.Add(node.GetMRNodeData().id);
        }
        string jsonify = JsonConvert.SerializeObject(guids,settings);

        
        return nodeList
            .Where(node => nodeIds.Contains(node.GetMRNodeData().id))
            .Select(node => node.GetMRNodeData())
            .ToList();
    }



    public async void SpawnNewNode(NodeType nodeType, Vector3 spawnTransform)
    {
         
         GameObject selectedNodeObject = nodeObjectMapSO.GetNodeObject(nodeType);
         GameObject newNode = Instantiate(selectedNodeObject, spawnTransform, Quaternion.identity, this.transform);   
         MRNode mrNode = newNode.GetComponent<MRNode>();
         mrNode.InitNewNode();
         nodeList.Add(mrNode);
         Debug.Log("[NODE MANAGER] Spawining New Node");
         await ActionServerController.Instance.AddNodes(new List<MRNodeData>{mrNode.GetMRNodeData()});
         
    }

    


    public List<MRNode> SpawnNodes(List<MRNodeData> nodeDatas)
    {
        foreach (MRNodeData nodeData in nodeDatas)
        {
            if (nodeList.Any(node => node.GetMRNodeData().id == nodeData.id))
            {
                Debug.Log($"<color=orange>Node with ID {nodeData.id} already exists. Skipping...</color>");
                continue;
            }
            string jsonify = JsonConvert.SerializeObject(nodeData,settings);
          

            NodeType nodeType = NodeTypeMap.GetNodeType(nodeData.type);
            GameObject selectedNodeObject = nodeObjectMapSO.GetNodeObject(nodeType);
            // Vector3 spawnPosition = MRSpatialAnchorManager.Instance.ConvertAnchorPositionToTargetRelativePosition(nodeData.position, this.transform);
            Debug.Log($"POSITION: {nodeData.position}");
            GameObject newNode = Instantiate(selectedNodeObject, nodeData.position, Quaternion.identity, this.transform); 
            newNode.transform.SetParent(this.transform); 
            MRNode mrNode = newNode.GetComponent<MRNode>();
            mrNode.SetMRNodeData(nodeData);
            nodeList.Add(mrNode);
            EdgeManager.Instance.UpdateNodeListToCurrentEdge(mrNode);
            
        }


        return nodeList;
    }




    public async Task UpdateNodeListDBById(List<Guid>  nodeIds)
    {
    
        await ActionServerController.Instance.UpdateNodeListById(nodeIds);
    }

    public async Task UpdateNodeListDBByData(List<MRNodeData>  nodeDatas)
    {
    
        await ActionServerController.Instance.UpdateNodeListByNodeData(nodeDatas);
    }
  

}
}