using System.Collections;
using System.Collections.Generic;
using ActionDataTypes.Logic;
using MRFlow.Component;
using NodeTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;




namespace MRFlow.Editor{
public class EditorTimerNode : BaseEditor, INodeEditor
{
  
  [SerializeField] private InputField durationInputField; 



    private MRNode mrNode;


    protected override void Start()
    {


        base.Start();
       
        durationInputField.onValueChanged.AddListener(OnDurationChanged);


       
    }

    public override void SetMRNode(MRNode newMRNode)
    {   
        base.SetMRNode(newMRNode);
    }


    private void OnDurationChanged(string newDuration)
    {
      
        if(mrNode != null && float.TryParse(newDuration, out float duration))
        {

            if(newDuration == "")
            {
                duration = 0;
            }
            durationInputField.text = duration.ToString();
            MRNodeData mrNodeData = mrNode.GetMRNodeData();
            (mrNodeData.action_data as TimerBlockData).waitTime = duration;
            mrNode.SetMRNodeData(mrNodeData);
            Debug.Log($"<color=yellow>[EditorTimerNode]Node Duration Changed: {duration}</color>");
            
        }
        
      
    }


}
}