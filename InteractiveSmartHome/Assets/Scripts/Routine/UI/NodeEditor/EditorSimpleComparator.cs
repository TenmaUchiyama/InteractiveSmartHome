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
public class EditorSimpleComparator : BaseEditor, INodeEditor
{


    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] InputField numberInputField;

     
   protected override void Start() 
   {
    base.Start(); 


    // ComparatorType を取得
        SimpleComparatorUtil.ComparatorType[] comparatorTypes = (SimpleComparatorUtil.ComparatorType[])Enum.GetValues(typeof(SimpleComparatorUtil.ComparatorType));

        // シンボルのリストを作成
        List<string> comparatorSymbols = new List<string>();
        foreach (var comparatorType in comparatorTypes)
        {
            string symbol = SimpleComparatorUtil.GetSimpleComparatorSymbol(comparatorType);
            comparatorSymbols.Add(symbol);
        }

        // Dropdown のオプションを設定
        dropdown.ClearOptions();
        dropdown.AddOptions(comparatorSymbols);

        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        numberInputField.onValueChanged.AddListener(OnValueChanged);    
   }

        private void OnDropdownChanged(int index)
        {


            if(mrNode != null)
            {
                string selectedSymbol = dropdown.options[index].text;
                 MRNodeData mrNodeData = mrNode.GetMRNodeData();
                (mrNodeData.action_data as SimpleComparatorBlockData).comparator = selectedSymbol;
                mrNode.SetMRNodeData(mrNodeData);
                Debug.Log($"<color=yellow>[EditorSimpleComparator]Node Comparator Changed: {selectedSymbol}</color>");
            }
            

        }

        public override void SetMRNode(MRNode newMRNode)
    {
        base.SetMRNode(newMRNode);
    }


    private void OnValueChanged(string newDuration)
    {
      
        if(mrNode != null && float.TryParse(newDuration, out float value))
        {

            if(newDuration == "")
            {
                value = 0f;
            }
            numberInputField.text = value.ToString();
            MRNodeData mrNodeData = mrNode.GetMRNodeData();
            (mrNodeData.action_data as SimpleComparatorBlockData).value = value;
            mrNode.SetMRNodeData(mrNodeData);
            Debug.Log($"<color=yellow>[EditorTimerNode]Node Duration Changed: {value}</color>");
            
        }
        
      
    }


}
}