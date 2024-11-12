using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using MRFlow.UI;
using NodeTypes;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NodeEditor : Singleton<NodeEditor>
{
   
    [SerializeField] private GameObject editorParent;
    [SerializeField] private NodeEditorMapSO nodeEditorMapSO;
    private GameObject currentlyEditingNode;
    


 

      public void OpenEditor(MRNode mrNode)
    {

        if(currentlyEditingNode != null)
        {
            CloseEditor();
        }
        NodeType nodeType = mrNode.GetNodeType(); 
        GameObject nodeEditor = nodeEditorMapSO.GetNodeEditor(nodeType);
        currentlyEditingNode = Instantiate(nodeEditor, editorParent.transform);
        
        INodeEditor componentEditor = currentlyEditingNode.GetComponent<INodeEditor>();
        componentEditor.SetMRNode(mrNode);
        Transform cameraTransform = Camera.main.transform;
        currentlyEditingNode.transform.position = cameraTransform.position + cameraTransform.forward * 1.5f;
        currentlyEditingNode.transform.LookAt(cameraTransform);
        currentlyEditingNode.transform.Rotate(0, 180, 0);
        editorParent.SetActive(true);
    
    }


    public void CloseEditor() 
    {
        editorParent.SetActive(false);
        Destroy(currentlyEditingNode);
        this.currentlyEditingNode = null;
    }



    public void OpenAndCloseEditor()
    {
        if (currentlyEditingNode.activeSelf)
        {
            currentlyEditingNode.SetActive(false);
        }
        else
        {
            currentlyEditingNode.SetActive(true);
        }
    }





}
