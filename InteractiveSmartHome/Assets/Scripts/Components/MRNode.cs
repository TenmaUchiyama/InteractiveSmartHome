using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using ActionDataTypes;
using MRFlow.Core;
using MRFlow.ServerController;
using MRFlow.SpatialAnchors;
using MRFlow.Test;
using MRFlow.UI;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;




namespace MRFlow.Component{
public class MRNode : MonoBehaviour
{    
    
    [SerializeField] Canvas rootCanvas; 

    protected MRNodeData _mrNodeData;

    private List<MREdge> connectedEdges = new List<MREdge>();

    [SerializeField] private NodeHandler nodeInHandler = null; 
    [SerializeField] private NodeHandler nodeOutHandler = null;



    
    public virtual void InitNewNode(){}
    public RectTransform GetCanvasRect()
    {
        return this.rootCanvas.GetComponent<RectTransform>();
    }



    protected async void NewNode()
    {
       await ActionServerController.Instance.AddNodes(new List<MRNodeData>{this._mrNodeData});
    }



    public async void DestroyNode()
    {
       
         Debug.Log($"<color=red>MRNode Destroy {this._mrNodeData.id}</color>");
         await ActionServerController.Instance.RemoveNode(this._mrNodeData);
         Destroy(this.gameObject);
    }

    public bool IsIDEquals(Guid id)
    {
        return this._mrNodeData.id == id;
    }

    public MRNodeData GetMRNodeData() 
    {
        // Vector3 currentPosition = MRSpatialAnchorManager.Instance.ConvertToAnchorRelativePosition(this.transform.position);
        this._mrNodeData.position = this.transform.position;
        return this._mrNodeData;
    }

    public UITheme GetUITheme() 
    {
        return this.GetComponent<UIThemeSetter>().GetTheme();
    }

    public void SetConnectedEdge(MREdge edge)
    {
        connectedEdges.Add(edge);
        
    }


    public void SetMRNodeData(MRNodeData mRNodeData)
    {
        this._mrNodeData = mRNodeData;
    }



    

    public NodeHandler GetNodeHandler(HandlerType  handlerType)
    {
        if(handlerType == HandlerType.HANDLER_IN)
        {
            return this.nodeInHandler;
        }


        if(handlerType == HandlerType.HANDLER_OUT)
        {
            return this.nodeOutHandler;
        }


        return null;
    }
   
}

}