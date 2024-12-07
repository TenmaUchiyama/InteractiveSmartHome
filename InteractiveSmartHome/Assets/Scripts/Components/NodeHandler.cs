
using System.Collections.Generic;
using MRFlow.Component;
using MRFlow.Core;
using NodeTypes;
using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeHandler : MonoBehaviour
{

    [SerializeField]  MRNode parentNode; 
    [SerializeField] HandlerType handlerType;
    
    private  List<MRNode> connectedMrNodes = new List<MRNode>();
    private  List<MREdge> connectedEdge = new List<MREdge>();


    public UnityEvent<NodeHandler> OnEdgeConnected = new UnityEvent<NodeHandler>();




 



public MRNode GetNode()
{
    return parentNode;
}
public MRNodeData GetNodeData()
{
    return parentNode.GetMRNodeData();
}

public int GetConnectedEdgeCount() 
{
    return this.connectedEdge.Count;
}


public void ClearConnection() 
{
    connectedMrNodes = new List<MRNode>();
    connectedEdge = null;
}

public HandlerType GetHandlerType()
{
    return handlerType;
}







public void SetConnectedNode(MRNode node)
{
    connectedMrNodes.Add(node);
}


public void SetConnectedEdge(MREdge edge)
{

  
    this.parentNode.SetConnectedEdge(edge);
    this.connectedEdge.Add(edge);
    OnEdgeConnected.Invoke(this);
}

public bool IsAlreadyConnected(MRNode node)
{
    bool foundConnection  = false; 

    foreach(MRNode connectedNode in this.connectedMrNodes)
    {
        foundConnection = connectedNode == node;
    }
    return foundConnection;
}


}
