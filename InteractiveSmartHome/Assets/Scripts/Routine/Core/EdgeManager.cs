using System;
using System.Collections.Generic;
using UnityEngine;
using MRFlow.Component;
using NodeTypes;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace MRFlow.Core
{



/**
このクラスは、全体のRoutineEdgeの管理を行うクラス。RoutineEdgeは、Routineに紐づけられたActionの流れ（Edge）。
EdgeとはどのActionがどのActionに繋がっているかを示す。
*/
public class EdgeManager : Singleton<EdgeManager>
{     


    //　現在のRoutineEdgeのEdgeリスト
    private List<MREdge> edgesInCurrentRoutine = new List<MREdge>();

    [SerializeField] private GameObject edgePrefab;



    //今開いているRoutineEdgeのデータ
    private MRRoutineEdgeData currentMRRoutineEdgeData; 
    


    private NodeHandler holdingHandler = null;
    



        public void SetCurrentMRRoutineEdgeData(MRRoutineEdgeData currentMRRoutineEdgeData)
        {
            this.currentMRRoutineEdgeData = currentMRRoutineEdgeData;
        }
        
        public MRRoutineEdgeData GetCurrentRoutineEdge() 
        {
            return this.currentMRRoutineEdgeData;
        }


     
        // Edgeを作成する。
        public void CreateEdge(NodeHandler selectedNodeOut, NodeHandler selectedNodeIn, MREdgeData mrEdgeData = null)
        {   
            
   
            if(IsEdgeExist(selectedNodeOut, selectedNodeIn)) return;
            
            GameObject edgeObj = Instantiate(edgePrefab, this.transform);
            MREdge newEdge = edgeObj.GetComponent<MREdge>();
            newEdge.setNodes(selectedNodeOut, selectedNodeIn);  
            
            selectedNodeIn.SetConnectedEdge(newEdge);
            selectedNodeOut.SetConnectedEdge(newEdge);
            selectedNodeOut.SetConnectedNode(selectedNodeIn.GetNode());
            selectedNodeIn.SetConnectedNode(selectedNodeOut.GetNode());

          

            UpdateNodeListToCurrentEdge(selectedNodeIn.GetNode()); 
            UpdateNodeListToCurrentEdge(selectedNodeOut.GetNode()); 


            if(mrEdgeData == null)
            {
                mrEdgeData = new MREdgeData(
                Guid.NewGuid().ToString(),
                selectedNodeOut.GetNodeData().id,
                selectedNodeIn.GetNodeData().id 
            );

            
            this.currentMRRoutineEdgeData.edges.Add(mrEdgeData);
            }
            

        }


        public void RemoveEdge(MREdge edge)
        {
            this.currentMRRoutineEdgeData.edges.Remove(edge.GetMREdgeData());
            edge.DestroyEdge();
        }

        // すでに同じActionのペアがEdgeに存在するかどうかを確認する。
        private bool IsEdgeExist(NodeHandler selectedNodeOut, NodeHandler selectedNodeIn)
        {
            
            bool isExists = selectedNodeIn.IsAlreadyConnected(selectedNodeOut.GetNode()) || selectedNodeOut.IsAlreadyConnected(selectedNodeIn.GetNode());
            if(isExists) Debug.LogError("Edge already exists");
       
            return isExists;
        }



        // 現在のRoutineEdgeの中のNodeを更新する。
        public void UpdateNodeListToCurrentEdge(MRNode newNode)
        {

            HashSet<Guid> uniqueNodeIds = new HashSet<Guid>(){newNode.GetMRNodeData().id};

                foreach (var node in this.currentMRRoutineEdgeData.nodes)
            {
                uniqueNodeIds.Add(node);
            }
            this.currentMRRoutineEdgeData.nodes = uniqueNodeIds.ToList();
        }



        public async void CreateRoutineEdgeData()
        {
            // RoutineEdgeからnodeのIDを取得してUpdateする。
            List<Guid> guids = this.currentMRRoutineEdgeData.nodes;
            await NodeManager.Instance.UpdateNodeListDBById(guids);
            // EdgeからRoutineを作成する。
        }
    }


}