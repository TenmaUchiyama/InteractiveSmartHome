using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionDataTypes;
using MRFlow.Component;
using MRFlow.Core;
using Newtonsoft.Json;
using NodeTypes;
using Unity.VisualScripting;
using UnityEngine;

public class RoutineManager : Singleton<RoutineManager>
{

    private List<Routine> routines = new List<Routine>(); 




    public void StartRoutine()
    {

        Debug.Log("Starting Routine");
        MRRoutineEdgeData routineEdge = EdgeManager.Instance.GetCurrentRoutineEdge(); 


        List<MRNode> nodes = NodeManager.Instance.NodeList; 

        List<MRNodeData> nodeDatasInRoutineEdge = nodes
            .Where(node => routineEdge.nodes.Contains(node.GetMRNodeData().id))
            .Select(node => node.GetMRNodeData())
            .ToList();

        
        List<Routine> routines = ConvertEdgesToRoutines(routineEdge.edges, nodeDatasInRoutineEdge);

        string json = JsonConvert.SerializeObject(routines, Formatting.Indented);

        Debug.Log(json); 
        
    }



private  List<Routine> ConvertEdgesToRoutines(List<MREdgeData> edges, List<MRNodeData> nodes)
{
      Dictionary<Guid, Guid> nodeIdToActionId = new Dictionary<Guid,Guid>();

        foreach (MRNodeData node in nodes)
        {
            nodeIdToActionId.Add(node.id, node.action_data.id);
        }

        List<Guid> nodeInIDs = edges.Select(edge => edge.node_in).ToList();
        List<Guid> nodeOutIDs = edges.Select(edge => edge.node_out).ToList();
        List<Guid> nodeInIDset = new HashSet<Guid>(nodeInIDs).ToList();
        List<Guid> nodeOutIDset = new HashSet<Guid>(nodeOutIDs).ToList();

        List<Guid> firstNodeIds = nodeInIDset.Where(id => !nodeOutIDset.Contains(id)).ToList();
        List<Guid> lastNodeIds = nodeOutIDset.Where(id => !nodeInIDset.Contains(id)).ToList();

        List<Routine> routines = edges.Select(edge => {
            Guid currentActionid = nodeIdToActionId.ContainsKey(edge.node_in) ? nodeIdToActionId[edge.node_in] : Guid.Empty;
            Guid nextActionid = nodeIdToActionId.ContainsKey(edge.node_out) ? nodeIdToActionId[edge.node_out] : Guid.Empty;

            Routine routine = new Routine(
                currentActionid,
                nextActionid 
            ); 

            if(firstNodeIds.Contains(edge.node_in))
            {
                routine.first = true; 
            };
            return routine; 
        }).ToList();


        foreach (Guid lastNodeId in lastNodeIds)
        {
            Guid currentActionid = nodeIdToActionId.ContainsKey(lastNodeId) ? nodeIdToActionId[lastNodeId] : Guid.Empty;
            Routine existingRoutine = routines.FirstOrDefault(routine => routine.currentBlockId == currentActionid && routine.nextBlockId == Guid.Empty);
            if(existingRoutine != null)
            {
                existingRoutine.last = true; 
            }
            else
            {
                Routine routine = new Routine(
                    currentActionid,
                    Guid.Empty
                ); 
                routine.last = true; 
                routines.Add(routine); 
            }
        }

        return routines.ToList();
}


private void UpdateMrRoutineEdgeDB(Guid edge_id)
{
    // Get Nodes in the Routine Edge
    MRRoutineEdgeData routineEdge = EdgeManager.Instance.GetCurrentRoutineEdge();
    List<MRNode> nodes = NodeManager.Instance.NodeList;
    List<MRNodeData> nodeDatasInRoutineEdge = NodeManager.Instance.GetMRNodeDataFromNodeIds(routineEdge.nodes);

    List<DBNode> dbNodes = NodeManager.Instance.ConvertFromNodeDataToDBData(nodeDatasInRoutineEdge);    


    
    

}
}
