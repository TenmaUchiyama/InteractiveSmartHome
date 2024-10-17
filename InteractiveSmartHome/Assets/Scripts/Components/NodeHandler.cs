using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using NodeTypes;
using Unity.VisualScripting;
using UnityEngine;

public class NodeHandler : MonoBehaviour
{
    [SerializeField]  MRNode parentNode; 
    [SerializeField] HandlerType handlerType;
    
    private MREdge connectedEdge; 


public MRNode GetNode()
{
    return parentNode;
}
public MRNodeData GetNodeData()
{
return parentNode.mrNodeData;
}

public HandlerType GetHandlerType()
{
    return handlerType;
}


public void SetConnectedEdge(MREdge edge)
{
    this.connectedEdge = edge;
}

    



}
