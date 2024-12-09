using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MRFlow.Component;
using NodeTypes;
using UnityEngine;
using Newtonsoft.Json;
using MRFlow.Test;
using MRFlow.Core;

public class TestManager : Singleton<TestManager>
{

    [SerializeField] private Button AddButton;
    [SerializeField] private Button SpawnButton; 
    [SerializeField] private Button RemoveButton;



    private void Update() {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddAllNodes();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            LoadAndSpawnNodes();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveAllNodes();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            NodeManager.Instance.SpawnNewNode(NodeType.Logic_Timer, Vector3.zero);
        }
    }
 JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

    public async void AddAllNodes() 
    {

       
        Debug.Log("AddAllNodes");

        MRNode[] nodes = Object.FindObjectsOfType<MRNode>();
        Debug.Log(nodes.Length);
        foreach (MRNode node in nodes)
        {
          MRNodeData mRNodeData = node.GetMRNodeData();
          string jsonify = JsonConvert.SerializeObject(mRNodeData,settings);
          Debug.Log($"<color=yellow>Adding Node: {jsonify}</color>");
         await TestServerConnector.Instance.TestAddNode(mRNodeData);
        };
    }

    
    public void RemoveAllNodes()
    {
        Debug.Log("RemoveAllNodes");

        MRNode[] nodes = Object.FindObjectsOfType<MRNode>();
 

        foreach (MRNode node in nodes)
        {
            NodeManager.Instance.RemoveNode(node);
        };
    }




    public async void LoadAndSpawnNodes() 
    {
        Debug.Log("LoadAndSpawnNodes");
        List<MRNodeData>  mrNodeDatas = await TestServerConnector.Instance.GetMRNodeDatas();
        
        NodeManager.Instance.SpawnNodes(mrNodeDatas);
    }



}
