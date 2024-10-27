using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using ActionDataTypes.Logic;

using NodeTypes;
using UnityEngine;
using System;
using ActionDataTypes;

public class TimerNode : MRNode
{
    [SerializeField] private float waitTime = 1.0f;


    



    private void Awake() {

        // Define Action Block data
        ActionBlock timerActionBlock = new TimerBlockData(
            Guid.NewGuid(),
            "Unity Test Timer",
            "Unity Test Timer Description",
            BlockActionTypeMap.GetActionType(ActionBlockType.Logic_Timer),
            (int)this.waitTime
        );

        string nodeTypeName = NodeTypeMap.GetNodeType(NodeType.Logic_Timer);


        // Define Node Data
        this._mrNodeData = new MRNodeData(
            Guid.NewGuid(),
            nodeTypeName,
            timerActionBlock,
            this.transform.position
        );
    }


    public void SetWaitTime(float waitTime)
    {
        this.waitTime = waitTime;
    }
}