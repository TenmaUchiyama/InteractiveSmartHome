using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using NodeTypes;
using UnityEngine;
using UnityEngine.UI;




namespace MRFlow.Editor{
public class BaseEditor : MonoBehaviour
{

[SerializeField] protected Button closeButton;
[SerializeField] protected InputField nameInputField; 
[SerializeField] protected InputField descriptionInputField;

protected MRNode mrNode;

    

     
    protected virtual void Start()
    {

        nameInputField.onValueChanged.AddListener(OnNameChanged);
        descriptionInputField.onValueChanged.AddListener(OnDescriptionChanged);

        closeButton.onClick.AddListener(() =>
        {
            NodeEditor.Instance.CloseEditor();
        });
  
    }
    
    
    public virtual void SetMRNode(MRNode newMRNode)
    {
       this.mrNode = newMRNode;

        MRNodeData mrNodeData = mrNode.GetMRNodeData();
        nameInputField.text = mrNodeData.action_data.name;
        descriptionInputField.text = mrNodeData.action_data.description;
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

}
}