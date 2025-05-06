using System;
using ActionDataTypes;
using ActionDataTypes.Logic;
using Meta.WitAi.Json;
using MRFlow.Types;
using NodeTypes;
using TMPro;
using UnityEngine;

namespace MRFlow.Component 
{
    public class NotGateLogicNode : MRNode
{
    private string node_name = "Not Gate Logic";
    private string description = "Simple Not Gate Logic Description";


    [SerializeField] protected TextMeshProUGUI displayInputStatus; 
    [SerializeField] protected TextMeshProUGUI displayOutputStatus;


  protected override void Start()
    {
    base.Start(); 


   }


    public override void InitNewNode() 
    {
        IActionBlock notGateLogicBlock = new NotGateLogicBlockData(Guid.NewGuid(), node_name, description);

        string nodeType = NodeTypeMap.GetNodeTypeString(NodeType.Logic_NotGate);


        MRNodeData newNode = new MRNodeData(
                Guid.NewGuid(),
                nodeType,
                notGateLogicBlock,
                this.transform.position
            );

        

        this.SetMRNodeData(newNode);

    }



    protected override void OnReceiveMsgFromServer(string payload)
    {
       MqttDataType mqttData = JsonConvert.DeserializeObject<MqttDataType>(payload);

        Debug.Log($"<color=green>[NotGateLogicNode] Received MQTT Data: {mqttData.value}</color>");
        if(mqttData.data_type == "boolean")
        {
            bool output = Convert.ToBoolean(mqttData.value);
            bool input = !output;
            displayInputStatus.text = input.ToString();
            displayOutputStatus.text = output.ToString();

            displayInputStatus.color = input ? Color.green : Color.red;
            displayOutputStatus.color = output ? Color.green : Color.red;
        }
    //    if(mqttData.metadata != null)
    //    {
    //     var metadataObject = mqttData.metadata as JObject;

    //     if(metadataObject.ContainsKey("input"))
    //     {
    //         string input = metadataObject["input"].ToString();
    //         displayInputStatus.text = input;
    //         bool inputBool = Convert.ToBoolean(input);
    //         bool output = !inputBool;
    //         displayOutputStatus.text = output.ToString();
    //     }

       
    //    }
    }


    




}
}