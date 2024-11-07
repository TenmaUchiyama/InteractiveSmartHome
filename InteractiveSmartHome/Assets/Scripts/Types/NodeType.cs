

using System;
using System.Collections.Generic;
using ActionDataTypes;
using UnityEngine;

namespace NodeTypes
{
    public enum ThemeType
    {
        Sensor, 
        Logic, 
        Actuator, 
        Api
    }
    public enum NodeType 
    {
        None,
        Logic_Timer,
        Logic_Condition, 
        Logic_SimpleComparator, 
        Logic_RangeComparator, 
        Logic_GateLogic, 
        Logic_NotGate, 

        Actuator_Light, 
        
        Sensor_ToggleButton, 
        Sensor_Thermometer, 

        Api_Scheduler, 

    }





public record NodeTypeMap{
       private static Dictionary<NodeType, string> nodeTypeMap = new Dictionary<NodeType, string>
    {
        {NodeType.Logic_Timer, "node-timer"},
        {NodeType.Logic_Condition, "node-condition"},
        {NodeType.Logic_SimpleComparator, "node-simple-comparator"},
        {NodeType.Logic_RangeComparator, "node-range-comparator"},
        {NodeType.Logic_GateLogic, "node-gate-logic"},
        {NodeType.Logic_NotGate, "node-not-gate"},

        {NodeType.Actuator_Light, "node-light"},
        
        {NodeType.Sensor_ToggleButton, "node-toggle-button"},
        {NodeType.Sensor_Thermometer, "node-thermo-sensor"},

        {NodeType.Api_Scheduler, "node-scheduler"},
    };


    public static NodeType GetNodeType(string nodeType)
    {
        foreach (KeyValuePair<NodeType, string> entry in nodeTypeMap)
        {
            if (entry.Value == nodeType)
            {
                return entry.Key;
            }
        }
        return NodeType.None;
    }


    public static string GetNodeTypeString(NodeType nodeType)
    {
        return nodeTypeMap[nodeType];
    }
}





    public enum HandlerType
    {
        HANDLER_OUT, 
        HANDLER_IN 
    }

    [Serializable]
    public record MRRoutineEdgeData
    {
        public Guid id {get; set;}
        public Guid  routine_id {get; set;}
        public string routine_name {get; set;}
        public List<Guid> nodes {get; set;}
        public List<MREdgeData> edges {get; set;}

        public MRRoutineEdgeData(Guid id, Guid routine_id, string routine_name, List<Guid> nodes, List<MREdgeData> edges)
        {
            this.id = id;
            this.routine_id = routine_id;
            this.routine_name = routine_name;
            this.nodes = nodes;
            this.edges = edges;
            
        }
    }


    [Serializable]
    public record MREdgeData
    {
        public string id { get; set; }
        public Guid node_out { get; set; }
        public Guid node_in { get; set; }

        public MREdgeData(string id, Guid node_out, Guid node_in)
        {
            this.id = id;
            this.node_out = node_out;
            this.node_in = node_in;

        }
    }



    [Serializable]
    public record MRNodeData
    {
        public Guid id { get; set; }
        public string type { get; set; }
        public IActionBlock action_data { get; set; }
        public Vector3 position { get; set; }



        public  MRNodeData(Guid id, string type,  IActionBlock action_data, Vector3 position)
        {
            this.id = id;
            this.type = type;
            this.action_data = action_data;
            this.position = position;
        }

      
    }



  




  /// <summary>
  /// ここからはデータベースに保存されている形
  /// </summary>
  /// <value></value>
    [Serializable]
    public record DBRoutineEdge
    {
        public string id {get; set;}
        public string associated_routine_id {get; set;}
        public string routine_name {get; set;}
        public DBEdge[] edges {get; set;}
        public string[] nodes {get; set;}

        public DBRoutineEdge(string id, string associatedRoutineId, string routine_name, DBEdge[] edges, string[] nodes)
        {
            this.id = id;
            this.associated_routine_id = associatedRoutineId;
            this.routine_name = routine_name;
            this.edges = edges;
            this.nodes = nodes;

        }
        
    }

    [Serializable]
    public record DBNode 
    {
        public string id { get; set; }
        public string type { get; set; }
        public string data_action_id { get; set; }
        public Vector3 mr_position { get; set; }


       public DBNode(string id, string type, string dataActionId,  Vector3 mr_position)
    {
        this.id = id;
        this.type = type;
        this.data_action_id = dataActionId;
        this.mr_position = mr_position;
    }

        
    }



    [Serializable]
    public record DBEdge
    {
        public string id {get; set;}
        public string source {get; set;}
        public string target {get; set;}

        public DBEdge(string id, string source, string target)
        {
            this.id = id;
            this.source = source;
            this.target = target;
        }
    }

}