using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MRFlow.Component;
using NodeTypes;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor.Search;

namespace MRFlow.Core
{

public class EdgeManager : Singleton<EdgeManager>
{     
    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private SelectingLine selectingLine;
    [SerializeField] private RayInspector rayInspector;


    private MRRoutineEdgeData currentMRRoutineEdgeData; 
    


    private NodeHandler holdingHandler = null;
    
    

        private void Start() {
            this.currentMRRoutineEdgeData = new MRRoutineEdgeData(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "unity-routine-test",
                new List<Guid>(), 
                 new List<MREdgeData>()
            ); 
        }
   
        public MRRoutineEdgeData GetCurrentRoutineEdge() 
        {
            return this.currentMRRoutineEdgeData;
        }

        public void SetSelectHandler(NodeHandler nodeHandler)
        {
            if(!nodeHandler) {
                ClearHoldingNode();
                return;
            };

            if(!this.holdingHandler) {
                this.holdingHandler = nodeHandler;
                
                selectingLine.SetNodeHandler(nodeHandler);
                return;
            }
            if(this.holdingHandler.GetHandlerType() == nodeHandler.GetHandlerType()){
                Debug.LogError("Cannot connect the same type of node");
                           ClearHoldingNode();
                return;
            }
            
            if(this.holdingHandler.GetHandlerType() == HandlerType.HANDLER_OUT)
            {
                CreateEdge(this.holdingHandler, nodeHandler);
            }else{
                CreateEdge(nodeHandler, this.holdingHandler);
            }
        }

        private void CreateEdge(NodeHandler selectedNodeOut, NodeHandler selectedNodeIn)
        {   

         
            // if the edge already exists, return
            if(IsEdgeExist(selectedNodeOut, selectedNodeIn)) return;


            GameObject edgeObj = Instantiate(edgePrefab, this.transform);
            MREdge newEdge = edgeObj.GetComponent<MREdge>();
            newEdge.setNodes(selectedNodeOut, selectedNodeIn);  

           


            selectedNodeIn.SetConnectedEdge(newEdge);
            selectedNodeOut.SetConnectedEdge(newEdge);
            selectedNodeOut.SetConnectedNode(selectedNodeIn.GetNode());
            selectedNodeIn.SetConnectedNode(selectedNodeOut.GetNode());

            Debug.Log($"<color=yellow>out{selectedNodeOut.GetNodeData() != null} {selectedNodeIn.GetNodeData().id != null}</color>");
            MREdgeData edgeData = new MREdgeData(
                Guid.NewGuid(),
                selectedNodeOut.GetNodeData().id,
                selectedNodeIn.GetNodeData().id 
            );

            UpdateNodeList(selectedNodeIn.GetNode()); 
            UpdateNodeList(selectedNodeOut.GetNode()); 
            
            this.currentMRRoutineEdgeData.edges.Add(edgeData);


        

           ClearHoldingNode();
        }

  
        private bool IsEdgeExist(NodeHandler selectedNodeOut, NodeHandler selectedNodeIn)
        {
            
            bool isExists = selectedNodeIn.IsAlreadyConnected(selectedNodeOut.GetNode()) || selectedNodeOut.IsAlreadyConnected(selectedNodeIn.GetNode());
            if(isExists) Debug.LogError("Edge already exists");
            ClearHoldingNode();
            return isExists;
        }

        private void ClearHoldingNode() 
        {
            this.holdingHandler = null;
            selectingLine.SetNodeHandler(null);

        }
      

        private void UpdateNodeList(MRNode newNode)
        {
            HashSet<Guid> uniqueNodeIds = new HashSet<Guid>(){newNode.GetMRNodeData().id};

            Debug.Log($"<color=yellow>{this.currentMRRoutineEdgeData != null }</color>");
            foreach (var node in this.currentMRRoutineEdgeData.nodes)
            {
                uniqueNodeIds.Add(node);
            }
            this.currentMRRoutineEdgeData.nodes = uniqueNodeIds.ToList();

        }
    
        

    
    }


}