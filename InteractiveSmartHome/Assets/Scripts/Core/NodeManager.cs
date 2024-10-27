using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MRFlow.Component;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Lumin;

public class NodeManager : Singleton<NodeManager>
{

    [SerializeField] private NodeHolder nodeUiHolder;
    [SerializeField] private NodeObjectMapSO nodeObjectMapSO;

    private NodeType selectedNodeUIType = NodeType.None;
    private List<MRNode> nodeList = new List<MRNode>();
    public List<MRNode> NodeList => nodeList;

    private List<MRNode> currentNodes = new List<MRNode>();

    public List<MRNode> CurrentNodes => currentNodes;



    


    public void AddNode(MRNode node)
    {
        nodeList.Add(node);
    }

    public List<DBNode> ConvertFromNodeDataToDBData(List<MRNodeData> mrNodeDatas)
{
    return mrNodeDatas.Select(nodeData => new DBNode(
        nodeData.id,                   
        nodeData.type,
        nodeData.action_data.id.ToString(),        
        nodeData.mrAnchorId,
        nodeData.position  
    )).ToList();
}
    public List<MRNodeData> GetMRNodeDataFromNodeIds(List<Guid> nodeIds)
    {
         

        return nodeList
            .Where(node => nodeIds.Contains(node.GetMRNodeData().id))
            .Select(node => node.GetMRNodeData())
            .ToList();
    }


    public void SetSelectedNodeUI(MRNodeUI nodeUi)
    {
        
    
        if(nodeUi == null) {
            if(selectedNodeUIType != NodeType.None) {
                SpawnNewNode();
            }
            this.ClearSelectedNode();
            return;
        }

        selectedNodeUIType = nodeUi.GetNodeType();
        nodeUiHolder.SetHoldNodeType(selectedNodeUIType);


    }


    public void SpawnNewNode()
    {

        Debug.Log("<color=green>Spawning new node</color>");    
        if (selectedNodeUIType == NodeType.None) return;
        GameObject selectedNodeObject = nodeObjectMapSO.GetNodeObject(selectedNodeUIType);

        if (selectedNodeObject == null) return;


        GameObject newNode = Instantiate(selectedNodeObject, this.transform);
        newNode.transform.position = this.nodeUiHolder.transform.position;
        

        
        this.ClearSelectedNode();

    }


    public void ClearSelectedNode() 
    {
        selectedNodeUIType = NodeType.None;
        nodeUiHolder.CancelHoldNode();
    }
  

}
