using System;
using System.Collections;
using System.Collections.Generic;
using ActionDataTypes.Logic;

using NodeTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



namespace MRFlow.Editor{
public class EditorGateLogicNode : BaseEditor, INodeEditor
{ 
    
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Image logicImage;

    
    

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


    GateLogicUtil.GateLogicType[] comparatorTypes = (GateLogicUtil.GateLogicType[])System.Enum.GetValues(typeof(GateLogicUtil.GateLogicType));

    List<string> comparatorSymbols = new List<string>();
    foreach (var comparatorType in comparatorTypes)
    {
        string symbol = GateLogicUtil.GetGateLogicSymbol(comparatorType);
        comparatorSymbols.Add(symbol);
    }

    dropdown.ClearOptions();
    dropdown.AddOptions(comparatorSymbols);

    dropdown.onValueChanged.AddListener(OnDropdownChanged);


    
    }

    private void OnDropdownChanged(int index)
    {
       if(mrNode != null)
       {
        string selectedSymbol = dropdown.options[index].text;
        MRNodeData mrNodeData = mrNode.GetMRNodeData();
        (mrNodeData.action_data as GateLogicBlockData).logic_operator = selectedSymbol;
        mrNode.SetMRNodeData(mrNodeData);
        Debug.Log($"<color=yellow>[EditorGateLogicNode]Node Comparator Changed: {selectedSymbol}</color>");
        GateLogicUtil.GateLogicType logicType = GateLogicUtil.GetGateLogicType(selectedSymbol);
        this.logicImage.sprite = gateSpriteMaps.Find(x => x.operatorType == logicType).sprite;

       }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}