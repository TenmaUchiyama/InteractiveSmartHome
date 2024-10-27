using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using ActionDataTypes;
using MRFlow.Core;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;




namespace MRFlow.Component{
public class MRNode : MonoBehaviour
{ 
    
    [SerializeField] Canvas rootCanvas; 

    protected MRNodeData _mrNodeData;
    



    public RectTransform GetCanvasRect()
    {
        return this.rootCanvas.GetComponent<RectTransform>();
    }
    void Start()
    {
         NodeManager.Instance.AddNode(this);
    }

    public bool IsIDEquals(Guid id)
    {
        return this._mrNodeData.id == id;
    }

    public MRNodeData GetMRNodeData() 
    {
        return this._mrNodeData;
    }

    public UITheme GetUITheme() 
    {
        return this.GetComponent<UIThemeSetter>().GetTheme();
    }
}

}