using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using ActionDataTypes.Logic;

using NodeTypes;
using UnityEngine;
using System;
using ActionDataTypes;
using Meta.WitAi.Json;
using TMPro;
using Unity.VisualScripting;
using MRFlow.UI;
namespace MRFlow.Component{
public class TimerNode : MRNode
{

    private string node_name = "Timer Node";
    private string description = "Timer Node Description";
    private float waitTime = 1.0f;

    [SerializeField] private TextMeshProUGUI timerText;


    
    // private void Start() {
    

    //     NodeEditor.Instance.SetNodeEditor(nodeEditInputComponent);
    // }

    void Start(){
        InitNewNode();
    }

    


     public override void InitNewNode() {
        // Define Action Block data
        IActionBlock timerActionBlock = new TimerBlockData(
            Guid.NewGuid(),
            "Unity Test Timer",
            "Unity Test Timer Description",
            this.waitTime
        );

        string nodeTypeName = NodeTypeMap.GetNodeTypeString(NodeType.Logic_Timer);


        // Define Node Data
        this._mrNodeData = new MRNodeData(
            Guid.NewGuid(),
            nodeTypeName,
            timerActionBlock,
            this.transform.position
        );

        NodeEditor.Instance.SetMRNodeData(this._mrNodeData);
        // base.NewNode();
    }



    public void UpdateActionBlockData(MRNodeData mRNode) 
    {
        string jsonify = JsonConvert.SerializeObject(mRNode);
        Debug.Log($"<color=yellow>UpdateActionBlockData: {jsonify}</color>");
    }





    public void SetWaitTime(float waitTime)
    {
        this.waitTime = waitTime;
    }
}
}