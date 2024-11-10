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
using MRFlow.Network;
using MRFlow.Core;
using MRFlow.Types;
using UniRx;
namespace MRFlow.Component{
public class TimerNode : MRNode
{

    private string node_name = "Timer Node";
    private string description = "Timer Node Description";
    [SerializeField] private float waitTime = 1.0f;

    [SerializeField] private TextMeshProUGUI timerText;


    
    
    public override void SetMRNodeData(MRNodeData mRNodeData)
    {

        Debug.Log($"<color=yellow>[TimerNode]SetMRNodeData: {mRNodeData}</color>");
        base.SetMRNodeData(mRNodeData);

        this.timerText.text = (mRNodeData.action_data as TimerBlockData).waitTime.ToString();
        
    }


    void Start(){
        timerText.text = waitTime.ToString();
     
    }

    


     public override void InitNewNode() {

        this.nodeType = NodeType.Logic_Timer;


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

        
        MRMqttController.Instance.SubscribeTopic(this._mrNodeData.action_data.id.ToString(), OnReceiveMsgFromServer);
      
        Debug.Log($"<color=yellow>[TimerNode] InitNewNode: {this._mrNodeData}</color>");
        // base.InitNewNode();
    }

        private void OnReceiveMsgFromServer(string arg0)
        {
            Debug.Log($"<color=yellow>[TimerNode] OnReceiveMsgFromServer: {arg0}</color>");
            MqttDataType mqttData = JsonConvert.DeserializeObject<MqttDataType>(arg0);
            Debug.Log($"<color=yellow>[TimerNode] OnReceiveMsgFromServer: {mqttData.value}</color>");
        }

        public override NodeType GetNodeType()
        {
            return base.GetNodeType();
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