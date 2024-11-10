using System.Collections;
using System.Collections.Generic;
using ActionDataTypes.Logic;
using MRFlow.Component;
using NodeTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorTimerNode : MonoBehaviour, INodeEditor
{
  [SerializeField] private Button closeButton;
  [SerializeField] private InputField nameInputField; 
  [SerializeField] private InputField descriptionInputField;
  [SerializeField] private InputField durationInputField; 



    private MRNode mrNode;


    void Start()
    {


        Debug.Log($"<color=yellow>[EditorTimerNode] Start {nameInputField != null}</color>");
        Debug.Log($"<color=yellow>[EditorTimerNode] Start {descriptionInputField != null}</color>");
        Debug.Log($"<color=yellow>[EditorTimerNode] Start {durationInputField != null}</color>");
        nameInputField.onValueChanged.AddListener(OnNameChanged);
        descriptionInputField.onValueChanged.AddListener(OnDescriptionChanged);
        durationInputField.onValueChanged.AddListener(OnDurationChanged);


        closeButton.onClick.AddListener(() =>
        {
            NodeEditor.Instance.CloseEditor();
        });
    }

    public void SetMRNode(MRNode newMRNode)
    {
        this.mrNode = newMRNode;
    }

    private void OnNameChanged(string newName)
    {
        if (mrNode != null)
        {
            MRNodeData mrNodeData = mrNode.GetMRNodeData();
            mrNodeData.action_data.name = newName;
            mrNode.SetMRNodeData(mrNodeData);
            Debug.Log($"<color=yellow>[EditorTimerNode]Node Name Changed: {newName}</color>");

        }
    }

    private void OnDescriptionChanged(string newDescription)
    {
        if (mrNode != null)
        {
            MRNodeData mrNodeData = mrNode.GetMRNodeData();
            mrNodeData.action_data.description = newDescription;
            mrNode.SetMRNodeData(mrNodeData);
            Debug.Log($"<color=yellow>[EditorTimerNode]Node Description Changed: {newDescription}</color>");
        }
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
