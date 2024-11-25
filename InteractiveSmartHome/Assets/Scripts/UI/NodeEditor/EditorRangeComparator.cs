using System;
using System.Collections;
using System.Collections.Generic;
using ActionDataTypes.Logic;
using MRFlow.Component;
using NodeTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MRFlow.Editor{
public class EditorRangeComparator :  BaseEditor, INodeEditor
{
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] InputField numberFrom; 
    [SerializeField] InputField numberTo;


    private RangeComparatorUtil.ComparatorType operatorType = RangeComparatorUtil.ComparatorType.InsideRange;
    private float fromValue = 0.0f;
    private float toValue = 10.0f;


    private string[] options = { "Inside", "Outside" };
    protected override void Start() 
   {
    
    base.Start(); 


    


        // Dropdown のオプションを設定
         dropdown.ClearOptions();

        // 新しいオプションをリストに変換して追加
        dropdown.AddOptions(new List<string>(options));

        // 初期選択の設定（必要に応じて）
        dropdown.value = 0; // 最初のオプションを選択

        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        numberFrom.onValueChanged.AddListener(OnValueFromChanged);
        numberTo.onValueChanged.AddListener(OnValueToChanged);
   }

        private void OnValueToChanged(string toValue)
        {
            this.toValue = float.Parse(toValue);
            SetData();
        }

        private void OnValueFromChanged(string fromValue)
        {
            this.fromValue = float.Parse(fromValue);
            SetData();
        }

        private void OnDropdownChanged(int selectedIndex)
        { 
            RangeComparatorUtil.ComparatorType selectedType = (RangeComparatorUtil.ComparatorType)selectedIndex;
            this.operatorType = selectedType;

            SetData();
        }

        private void SetData()
        {
            MRNodeData mrNodeData = mrNode.GetMRNodeData();
            RangeComparatorUtil.RangeComparatorTempData rangeComparatorBlock = RangeComparatorUtil.GetRangeComparatorData(this.operatorType, this.fromValue, this.toValue);
            (mrNodeData.action_data as RangeComparatorBlockData).operatorFrom = rangeComparatorBlock.comperatorFrom;
            (mrNodeData.action_data as RangeComparatorBlockData).operatorTo = rangeComparatorBlock.comperatorTo;
            (mrNodeData.action_data as RangeComparatorBlockData).from = rangeComparatorBlock.from;
            (mrNodeData.action_data as RangeComparatorBlockData).to = rangeComparatorBlock.to;

            mrNode.SetMRNodeData(mrNodeData);
        }


    public override void SetMRNode(MRNode newMRNode)
    {
        base.SetMRNode(newMRNode);
    }

    }
}