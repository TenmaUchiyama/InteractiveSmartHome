
using ActionDataTypes.Logic;

using NodeTypes;
using UnityEngine;
using System;
using ActionDataTypes;
using Meta.WitAi.Json;
using TMPro;
using MRFlow.Network;
using MRFlow.Types;

namespace MRFlow.Component{
public class TimerNode : MRNode
{

    private string node_name = "Timer Node";
    private string description = "Timer Node Description";
    [SerializeField] private float waitTime = 1.0f;

    [SerializeField] private TextMeshProUGUI timerText;









        public override void SetMRNodeData(MRNodeData mRNodeData)
    {
        
        this.waitTime = (mRNodeData.action_data as TimerBlockData).waitTime;
        string timeText = this.waitTime.ToString();
        this.timerText.text = timeText;
        Debug.Log($"<color=yellow>[TimerNode] SetMRNodeData: {timeText}</color>");
        base.SetMRNodeData(mRNodeData);
        
        
    }


    protected override void Start(){
        base.Start();
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
        Debug.Log($"<color=yellow>[TimerNode] InitNewNode: {this._mrNodeData}</color>");

      
        base.InitNewNode();
    }

    protected override void OnReceiveMsgFromServer(string payload)
    {
        
        MqttDataType mqttData = JsonConvert.DeserializeObject<MqttDataType>(payload);
        int intValue; 
       if (int.TryParse(mqttData.value, out intValue))
        {
          
            string outputText = intValue == 0 ? this.waitTime.ToString() : mqttData.value;
            this.timerText.text = outputText;
        }

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