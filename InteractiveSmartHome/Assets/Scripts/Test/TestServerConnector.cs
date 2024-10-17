
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using ActionDataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodeTypes;
using UnityEngine;

public class TestServerConnector : MonoBehaviour
{
    async void Start()
    {


        
       List<DBNode> nodes = await ActionServerConnector.Instance.GetAllNodes();

         var tasks = nodes.Select(async node =>
        {
            object actionData = await ActionServerConnector.Instance.GetAction(node.data_action_id);
            Node newNode = new Node(id: node.id, type : node.type,action_data: actionData,  node.position);

            return newNode;
        });

       
        Node[] nodeArray = await Task.WhenAll(tasks);
        
        FlowStore.Instance.nodes.Add(nodeArray.ToList());
    }

    

    // public async void StartAllRoutine() 
    // {
    //     const string routineUrl = "/routine/start-all";
    //     await ServerConnector.Instance.StartAllRoutine(routineUrl);
    // }
}
