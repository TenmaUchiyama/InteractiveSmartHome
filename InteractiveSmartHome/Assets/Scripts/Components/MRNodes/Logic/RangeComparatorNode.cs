using System;
using System.Collections;
using System.Collections.Generic;
using ActionDataTypes;
using ActionDataTypes.Logic;
using MRFlow.Types;
using NodeTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace MRFlow.Component
{
    public class RangeComparatorNode : MRNode
{
    private string node_name = "Range Comparator";
    private string description = "Range Comparator Description";

    [SerializeField] private float minValue = 0.0f;
    [SerializeField] private float maxValue = 10.0f;

    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private TextMeshProUGUI rangeText; 
    [SerializeField] private TextMeshProUGUI comparatorText;

    private RangeComparatorUtil.ComparatorType operatorType = RangeComparatorUtil.ComparatorType.InsideRange;




    protected override void Start()
    {
        base.Start();
     
    }



     public override void InitNewNode()
        {



            RangeComparatorUtil.RangeComparatorTempData rangeComparatorBlock = RangeComparatorUtil.GetRangeComparatorData(operatorType, minValue, maxValue);

            IActionBlock simpleComparatorBlock = new RangeComparatorBlockData(
                Guid.NewGuid(), 
                "Unity Test Range Comparator",
                "Range Comparator Description",
                rangeComparatorBlock.comperatorFrom,
                rangeComparatorBlock.comperatorTo,
                rangeComparatorBlock.from,
                rangeComparatorBlock.to
                
            );

            string nodeType = NodeTypeMap.GetNodeTypeString(NodeType.Logic_RangeComparator);

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
        
        RangeComparatorUtil.ComparatorType selectedType = RangeComparatorUtil.GetComparatorType((mRNodeData.action_data as RangeComparatorBlockData).operatorFrom, (mRNodeData.action_data as RangeComparatorBlockData).operatorTo);
        switch (selectedType)
        {
            case RangeComparatorUtil.ComparatorType.InsideRange:
                this.comparatorText.text = "Inside";
                this.operatorType = RangeComparatorUtil.ComparatorType.InsideRange;
                break;
            case RangeComparatorUtil.ComparatorType.OutsideRange:
                this.comparatorText.text = "Outside";
                this.operatorType = RangeComparatorUtil.ComparatorType.OutsideRange;
                break;
        }

        this.minValue = (mRNodeData.action_data as RangeComparatorBlockData).from;
        this.maxValue = (mRNodeData.action_data as RangeComparatorBlockData).to;

        this.rangeText.text = $"{this.minValue} ~ {this.maxValue}";
        base.SetMRNodeData(mRNodeData);
    }

}
}