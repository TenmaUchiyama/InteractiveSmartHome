using System;
using System.Collections.Generic;
using ActionDataTypes;
using ActionDataTypes.Logic;
using MRFlow.Types;
using Newtonsoft.Json;
using NodeTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;




namespace MRFlow.Component{
public class GateLogicNode : MRNode
{
    private string node_name = "Simple Comparator";
    private string description = "Simple Comparator Description";
    private GateLogicUtil.GateLogicType currentSelectedLogicType = GateLogicUtil.GateLogicType.AND;


    [SerializeField] protected TextMeshProUGUI displayEdgeStatus; 
    [SerializeField] protected TextMeshProUGUI statusText;
    [SerializeField] protected TextMeshProUGUI logicNameText;
    [SerializeField] protected Image logicImage;

    private int numConnectedEdge = 0;
    private int numTrueEdge = 0;

    

    [Serializable]
      public struct GateSprite
    {
        public Sprite sprite;
        public GateLogicUtil.GateLogicType operatorType;
    }


    [SerializeField] public List<GateSprite> gateSpriteMaps = new List<GateSprite>();


    protected override void Start()
    {
        base.Start();

       base.nodeInHandler.OnEdgeConnected.AddListener(OnEdgeConnected);
       

    }


    void OnDestroy()
    {
        base.nodeInHandler.OnEdgeConnected.RemoveListener(OnEdgeConnected);
    }


    public override void SetMRNodeData(MRNodeData mRNodeData)
    {

        string symbol = (mRNodeData.action_data as GateLogicBlockData).logic_operator;
        this.currentSelectedLogicType = GateLogicUtil.GetGateLogicType(symbol);

        this.SetGateLogic(this.currentSelectedLogicType);
        base.SetMRNodeData(mRNodeData);
    }

    public override void InitNewNode()
        {
            IActionBlock gateLogicBlock = new GateLogicBlockData(
                Guid.NewGuid(), 
                "Unity Test Gate Logic",
                "Simple Gate Logic  Description",
                GateLogicUtil.GetGateLogicSymbol(currentSelectedLogicType)
            ); 

            
            string nodeType = NodeTypeMap.GetNodeTypeString(NodeType.Logic_GateLogic);

            MRNodeData newNode = new MRNodeData(
                Guid.NewGuid(),
                nodeType,
                gateLogicBlock,
                this.transform.position
            );

            this.SetMRNodeData(newNode);
        }



    protected override void OnReceiveMsgFromServer(string payload)
    {
        MqttDataType mqttData = JsonConvert.DeserializeObject<MqttDataType>(payload);

        Debug.Log($"<color=green>[GateLogicNode] Received MQTT Data: {mqttData.value}</color>");
        if(mqttData.data_type == "number")
        {
            if(int.TryParse(mqttData.value, out int intValue))
            {
                int connectedEdge = this.nodeInHandler.GetConnectedEdgeCount();
                this.displayEdgeStatus.text = intValue.ToString() + "/" + connectedEdge.ToString();
            }
        }
   

        if(mqttData.data_type == "boolean")
        {
            if(bool.TryParse(mqttData.value.ToLower(), out bool boolValue))
            {
                this.statusText.text = boolValue ? "Pass" : "Block";
                this.statusText.color = boolValue ? Color.green : Color.red;
                this.displayEdgeStatus.color = boolValue ? Color.green : Color.red;
            }
        }
    }

    private void OnEdgeConnected(NodeHandler handler)
    {
        
        int connectedEdgeCount = handler.GetConnectedEdgeCount();
        Debug.Log($"<color=yellow>==================================[GateLogicNode]OnEdgeConnected: {connectedEdgeCount}</color>");
        string status = connectedEdgeCount.ToString(); 
        this.numConnectedEdge = connectedEdgeCount;
        this.displayEdgeStatus.text = this.numTrueEdge.ToString() + "/" + status;
        Debug.Log($"<color=yellow>==================================[GateLogicNode]OnEdgeConnected: {this.numConnectedEdge}</color>");
    }

    



   
    public void SetGateLogic(GateLogicUtil.GateLogicType operatorType)
    {
        this.currentSelectedLogicType = operatorType;
        string symbol = GateLogicUtil.GetGateLogicSymbol(operatorType);
        this.logicNameText.text = symbol;
        this.logicImage.sprite = gateSpriteMaps.Find(x => x.operatorType == operatorType).sprite;

    }


}
}