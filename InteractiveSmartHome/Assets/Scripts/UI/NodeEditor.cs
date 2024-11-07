using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.UI;
using NodeTypes;
using Unity.VisualScripting;
using UnityEngine;

public class NodeEditor : Singleton<NodeEditor>
{
    [SerializeField] private GameObject canvasParent;
    [SerializeField] private GameObject contentParent; 

    [SerializeField] private UIInputComponentSO nodeEditorComponentSO;
    [SerializeField] private GameObject testInputComponent;
    
    

    bool testValue = false;

      public void SetMRNodeData(MRNodeData mrNodeData)
    {
        // UIエディタをインスタンス化し、MRNodeDataをバインド
        EditorTimerNode timerNodeEditor = testInputComponent.GetComponent<EditorTimerNode>();
        timerNodeEditor.SetMRNodeData(mrNodeData);
    }

// public void AddInputComponent<T>(string title, UIInputComponentType inputComponentType, T initialValue, Action<T> setter)
//     {
//         Debug.Log($"Adding Input Component: {title} Type: {inputComponentType} Value: {initialValue}");
//         GameObject nodeEditorInputPrefab = nodeEditorComponentSO.GetUIInputComponentPrefab(inputComponentType);
//         Debug.Log($"<color=red>NodeEditorInputPrefab: {nodeEditorInputPrefab}</color>");
//         NodeEditInputComponent nodeEditInputComponentScript = nodeEditorInputPrefab.GetComponent<NodeEditInputComponent>();
        
//         nodeEditInputComponentScript.SetNodeEditorComponent(title,  newVal =>{ setter.Invoke(newVal); Debug.Log("<color=red>FOIERJOFJEOIJEFIOJ</color>");}, initialValue);
//         Instantiate(nodeEditorInputPrefab, contentParent.transform);
//     }
    // public void SetNodeEditor(GameObject nodeEditInputComponent)
    // {

        
    //     Instantiate(nodeEditInputComponent, contentParent.transform);

    //     canvasParent.SetActive(true);
    // }

    



}
