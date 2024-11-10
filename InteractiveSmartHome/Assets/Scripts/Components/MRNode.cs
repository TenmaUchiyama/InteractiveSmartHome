using System;
using System.Collections.Generic;
using MRFlow.Network;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.UI;




namespace MRFlow.Component{
public class MRNode : MonoBehaviour
{    
    
    [SerializeField] Canvas rootCanvas; 
    [SerializeField] Button editButton;

    protected MRNodeData _mrNodeData;
    protected NodeType nodeType;

    private List<MREdge> connectedEdges = new List<MREdge>();

    [SerializeField] private NodeHandler nodeInHandler = null; 
    [SerializeField] private NodeHandler nodeOutHandler = null;


    public void Awake()
    {
        editButton.onClick.AddListener(OpenEditor);
    }

    
    public virtual void InitNewNode(){}

    public virtual NodeType GetNodeType()
    {
        return this.nodeType;
    }


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
        Debug.Log($"<color=yellow>GetMRNodeData: {this._mrNodeData}</color>");
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


    public virtual void SetMRNodeData(MRNodeData mRNodeData)
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


    public void OpenEditor() 
    {
        NodeEditor.Instance.OpenEditor(this);
    }
   
}

}