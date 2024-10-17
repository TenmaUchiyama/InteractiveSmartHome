using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MRFlow.Component;
using NodeTypes;
using Unity.VisualScripting;
using System.Linq;

namespace MRFlow.Core
{

public class EdgeManager : Singleton<EdgeManager>
{     
    [SerializeField] private GameObject edgePrefab;
    private NodeHandler holdingHandler = null;
    private List<MREdgeData> mrEdgeDatas = new List<MREdgeData>();
    
        public void SelectHandlerDown(NodeHandler nodeHandler)
        {
            if(!this.holdingHandler) this.holdingHandler = nodeHandler;
                    
        }

        public void SelectHandlerUp(NodeHandler nodeHandler)
        {
           
            if(!nodeHandler) {
                this.holdingHandler = null;
                return;
            }
            if(!holdingHandler) return; 
            if(this.holdingHandler.GetHandlerType() == nodeHandler.GetHandlerType()){
                this.holdingHandler= null; 
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

            selectedNodeOut.SetConnectedEdge(newEdge);
            selectedNodeIn.SetConnectedEdge(newEdge);


            MREdgeData edgeData = new MREdgeData(
                Guid.NewGuid(),
                selectedNodeOut.GetNodeData().id,
                selectedNodeIn.GetNodeData().id 
            );

            
            this.mrEdgeDatas.Append(edgeData);
            

            this.holdingHandler = null;
        }

        private bool IsEdgeExist(NodeHandler selectedNodeOut, NodeHandler selectedNodeIn)
        {
            
            bool isExists =this.mrEdgeDatas.Any(edge => edge.node_out == selectedNodeOut.GetNodeData().id && edge.node_in == selectedNodeIn.GetNodeData().id);
            if(isExists) Debug.LogError("Edge already exists");
            return isExists;
        }


        private void Update() {
            
            // Debug.Log(this.holdingHandler);
        }
    }
}