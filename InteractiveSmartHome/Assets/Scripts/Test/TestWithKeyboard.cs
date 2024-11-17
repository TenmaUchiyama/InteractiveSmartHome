using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MRFlow.Component;
using MRFlow.Core;
using MRFlow.Network;
using Newtonsoft.Json;
using NodeTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MRFlow.Test
{

public class TestWithKeyboard : MonoBehaviour
{
   
    [SerializeField] private Button AddButton;
    [SerializeField] private Button SpawnButton; 
    [SerializeField] private Button RemoveButton;



    [SerializeField] MRNode node1; 
    [SerializeField] MRNode node2;
    [SerializeField] MRNode node3;






    



    public void Start()
    {
        
        
        //     AddButton.onClick.AddListener(AddAllNodes);
        // SpawnButton.onClick.AddListener(LoadAndSpawnNodes);
        // RemoveButton.onClick.AddListener(RemoveAllNodes);
    }



    private  async void Update() {
       

        if(Input.GetKeyDown(KeyCode.T)) { 
            Debug.Log("<color=yellow>Test</color>");
            AddAllNodes();
        }


        if(Input.GetKeyDown(KeyCode.R)) { 
            RemoveAllNodes();
        }

        if(Input.GetKeyDown(KeyCode.U)) { 
            Debug.Log("UpdateAllNodes");
           UpdateAllNodes();
        }
        

        if(Input.GetKeyDown(KeyCode.Q)) { 
            Debug.Log("LoadAndSpawnNodes");
            LoadAndSpawnNodes();
        }




        if(Input.GetKeyDown(KeyCode.S)) { 
           await RoutineEdgeManager.Instance.UpdateRoutine(EdgeManager.Instance.GetCurrentRoutineEdge());
        }


        if(Input.GetKeyDown(KeyCode.C)) { 

      
        NodeManager.Instance.AddNode(node1);
        NodeManager.Instance.AddNode(node2);
        NodeManager.Instance.AddNode(node3);

        
             
        NodeHandler nodeHandler1Out = node1.GetNodeHandler(HandlerType.HANDLER_OUT);
        NodeHandler nodeHandler2In = node2.GetNodeHandler(HandlerType.HANDLER_IN);
        NodeHandler nodeHandler2Out = node2.GetNodeHandler(HandlerType.HANDLER_OUT);
        NodeHandler nodeHandler3In = node3.GetNodeHandler(HandlerType.HANDLER_IN);


        

        EdgeManager.Instance.CreateEdge(nodeHandler1Out, nodeHandler2In);
        EdgeManager.Instance.CreateEdge(nodeHandler2Out, nodeHandler3In);

        RoutineEdgeManager.Instance.TestUpdateRoutineEdge();



        }
    }


 JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };


    public async void UpdateAllNodes() 
    {
        Debug.Log("UpdateAllNodes");

        MRNode[] nodes = GameObject.FindObjectsOfType<MRNode>();
        List<MRNodeData> nodeDatas = nodes.Select(node => node.GetMRNodeData()).ToList();

        foreach (MRNodeData nodeData in nodeDatas)
        {
            string jsonify = JsonConvert.SerializeObject(nodeData,settings);
            Debug.Log($"<color=yellow>Updating Node: {jsonify}</color>");
        };

        await NodeManager.Instance.UpdateNodeListDBByData(nodeDatas);


    }

    public async void AddAllNodes() 
    {

       
        Debug.Log("AddAllNodes");
        
        MRNode[] nodes = GameObject.FindObjectsOfType<MRNode>();
      
        List<MRNodeData> nodeDatas = nodes.Select(node => {
            node.InitNewNode();
            return node.GetMRNodeData();}).ToList();
        Debug.Log(nodeDatas.Count);
        await ActionServerController.Instance.AddNodes(nodeDatas);
    }

    
    public void RemoveAllNodes()
    {
        Debug.Log("RemoveAllNodes");

        MRNode[] nodes = GameObject.FindObjectsOfType<MRNode>();
 

        foreach (MRNode node in nodes)
        {
            NodeManager.Instance.RemoveNode(node);
        };
    }




    public async void LoadAndSpawnNodes() 
    {
        Debug.Log("LoadAndSpawnNodes");
        List<MRNodeData>  mrNodeDatas = await ActionServerController.Instance.GetMRNodeDatas();
        
        NodeManager.Instance.SpawnNodes(mrNodeDatas);
    }
}
}