using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ActionDataTypes;
using ActionDataTypes.Logic;
using MRFlow.Component;
using MRFlow.Types;
using NodeTypes;
using TMPro;
using UnityEngine;
namespace MRFlow.Component{
public class SimpleComparatorNode : MRNode
{
    private string node_name = "Simple Comparator";
    private string description = "Simple Comparator Description";
    public enum OperatorType
    {
        GreaterThan,   // ">"
        LessThan,      // "<"
        Equal,         // "="
        NotEqual,      // "!="
        GreaterOrEqual, // ">="
        LessOrEqual    // "<="
    }

    [SerializeField] private float comparatingValue = 0.0f;

    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI valueText;
    

    private OperatorType operatorType = OperatorType.Equal;



   protected override void Start(){
        base.Start();
       SetDisplayText(operatorType, comparatingValue);
  

    }


        public override void InitNewNode()
        {
            IActionBlock simpleComparatorBlock = new SimpleComparatorBlockData(
                Guid.NewGuid(), 
                "Unity Test Comparator",
                "Simple Comparator Description",
                BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_SimpleComparator),
                GetOperatorSymbol(operatorType),
                comparatingValue.ToString()
            ); 

            string nodeType = NodeTypeMap.GetNodeTypeString(NodeType.Logic_SimpleComparator);

            MRNodeData newNode = new MRNodeData(
                Guid.NewGuid(),
                nodeType,
                simpleComparatorBlock,
                this.transform.position
            );

            this.SetMRNodeData(newNode);
        }



        protected override void OnReceiveMsgFromServer(string payload)
        {
            MqttDataType mqttData = JsonUtility.FromJson<MqttDataType>(payload);
            
            if(mqttData.data_type == "boolean")
            {
                if(bool.TryParse(mqttData.value.ToLower(), out bool boolValue))
                {
                    this.statusText.text = boolValue ? "Pass" : "Block";
                    this.statusText.color = boolValue ? Color.green : Color.red;
                }
            }
        }

        public override void SetMRNodeData(MRNodeData mRNodeData)
    {
        string symbol = (mRNodeData.action_data as SimpleComparatorBlockData).comparator;
        this.operatorType = GetOperatorType(symbol);

        SetDisplayText(operatorType, comparatingValue);
        base.SetMRNodeData(mRNodeData);
    }

    public string GetOperatorSymbol(OperatorType operatorType)
    {
        switch (operatorType)
        {
            case OperatorType.GreaterThan: return ">";
            case OperatorType.LessThan: return "<";
            case OperatorType.Equal: return "=";
            case OperatorType.NotEqual: return "!=";
            case OperatorType.GreaterOrEqual: return ">=";
            case OperatorType.LessOrEqual: return "<=";
            default: return "";
        }
    }

    public OperatorType GetOperatorType(string operatorSymbol)
    {
        switch (operatorSymbol)
        {
            case ">": return OperatorType.GreaterThan;
            case "<": return OperatorType.LessThan;
            case "=": return OperatorType.Equal;
            case "!=": return OperatorType.NotEqual;
            case ">=": return OperatorType.GreaterOrEqual;
            case "<=": return OperatorType.LessOrEqual;
            default: return OperatorType.Equal;
        }
    }



    private void SetDisplayText(OperatorType operatorType, float comparatingValue)
    {
        string symbol = GetOperatorSymbol(operatorType);
        valueText.text = $"{symbol} {comparatingValue}";
    }


}
}