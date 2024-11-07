using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Attributes;
using MRFlow.Core;
using NodeTypes;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;

public class MRControllerInput : MonoBehaviour
{
    [SerializeField , Interface(typeof(IController))]
    private UnityEngine.Object _controller;
    private IController Controller; 

    [SerializeField] RayInteractor rayInteractor;

    [SerializeField] GameObject nodeVisual;
    [SerializeField] SelectingLine selectingLine;
    [SerializeField] Transform nodeSpawnPosition;



    private NodeType currentSelectedType = NodeType.None; 
    private NodeHandler currentSelectedHandler = null; 


    protected virtual void Awake()
    {
        Controller = _controller as IController;
    }


    private void Update() {
        
        

        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            if (!rayInteractor.enabled)
            {
                OnNullHit();
                return;
            }
            Ray ray = rayInteractor.Ray;
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                bool isHit = OnHitRay(hit);
                if(isHit) return;
            }

            OnNullHit();

        }
    }



private bool OnHitRay(RaycastHit hit) 
{
    
    if(hit.transform.TryGetComponent(out MRNodeUI nodeUI))
    {
            OnHitNode(nodeUI);
            return true;
    }

    if(hit.transform.TryGetComponent(out NodeHandler nodeHandler))
    {

        SetSelectHandler(nodeHandler);
        return true;
    }

    return false;
}

private void OnHitNode(MRNodeUI nodeUI)
{
    NodeType nodeType = nodeUI.GetNodeType();
    currentSelectedType = nodeType;
    nodeVisual.SetActive(true);
    rayInteractor.enabled = false;
}

private void OnNullHit()
{
    Debug.Log($"<color=red>Null Hit {currentSelectedType}</color>");
    if(currentSelectedType != NodeType.None)
    {

        NodeManager.Instance.SpawnNewNode(currentSelectedType, nodeSpawnPosition.position);
        this.ClearNode();
    }

}

 public void SetSelectHandler(NodeHandler nodeHandler)
        {
            if(!nodeHandler) {
                ClearHoldingNode();
                return;
            };

            if(!this.currentSelectedHandler) {
                this.currentSelectedHandler = nodeHandler;
                
                selectingLine.SetNodeHandler(nodeHandler);
                return;
            }
            if(this.currentSelectedHandler.GetHandlerType() == nodeHandler.GetHandlerType()){
                Debug.LogError("Cannot connect the same type of node");
                           ClearHoldingNode();
                return;
            }
            
            if(this.currentSelectedHandler.GetHandlerType() == HandlerType.HANDLER_OUT)
            {

                EdgeManager.Instance.CreateEdge(this.currentSelectedHandler, nodeHandler);
            }else{
                 EdgeManager.Instance.CreateEdge(nodeHandler, this.currentSelectedHandler);
            }

            ClearHoldingNode();
        }

    private void ClearHoldingNode()
    {
       this.currentSelectedHandler = null;
        selectingLine.SetNodeHandler(null);
        
    }

    private void ClearNode() 
    {
        currentSelectedType = NodeType.None;
        nodeVisual.SetActive(false);
        this.CleanControllerState();    
    }

    private void CleanControllerState()
    {
        rayInteractor.enabled = true;
    }



}
