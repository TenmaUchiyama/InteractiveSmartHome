

using System;
using System.Collections.Generic;
using ActionDataTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

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



//       Timer = "node-timer",
//   SimpleComparator = "node-simple-comparator",
//   RangeComparator = "node-range-comparator",
//   GateLogic = "node-gate-logic",
//   NotGate = "node-not-gate",

//   Light = "node-light",
//   ToggleButton = "node-toggle-button",
//   Thermometer = "node-thermo-sensor",

//   Scheduler = "node-scheduler",

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


    public static string GetNodeType(NodeType nodeType)
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
            this.nodes = nodes;
            this.edges = edges;
            
        }
    }


    [Serializable]
    public record MREdgeData
    {
        public Guid id { get; set; }
        public Guid node_out { get; set; }
        public Guid node_in { get; set; }

        public MREdgeData(Guid id, Guid node_out, Guid node_in)
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
        public ActionBlock action_data { get; set; }
        public Vector3 position { get; set; }

        public Guid mrAnchorId { get; set; }


        public  MRNodeData(Guid id, string type,  ActionBlock action_data, Vector3 position)
        {
            this.id = id;
            this.type = type;
            this.action_data = action_data;
            this.position = position;
        }

      
    }

    [Serializable]
    public record DBNode 
    {
        public Guid id { get; set; }
        public string type { get; set; }
        public string data_action_id { get; set; }
        public Guid mrAnchorId { get; set; }
        public Vector3 position { get; set; }


       public DBNode(Guid id, string type, string dataActionId, Guid mrAnchorId , Vector3 position)
    {
        this.id = id;
        this.type = type;
        this.data_action_id = dataActionId;
        this.mrAnchorId = mrAnchorId;
        this.position = position;
    }

        
    }



    [Serializable]
    public record NodePosition
    {
        public float x { get; set; }
        public float y { get; set; }

        public NodePosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

    }


    // [Serializable]
    // public record Edge
    // {
    //     public string id { get; set; }
    //     public string source { get; set; }
    //     public string target { get; set; }
    //     public string type { get; set; }

    //     public Edge(string id, string source, string target, string type)
    //     {
    //         this.id = id;
    //         this.source = source;
    //         this.target = target;
    //         this.type = type;
            
    //     }
    // }

    [Serializable]
    public record RoutineEdge
    {
        public string id {get; set;}
        public string associated_routine_id {get; set;}
        public Edge[] edges {get; set;}
        public string[] nodes {get; set;}

        public RoutineEdge(string id, string associatedRoutineId, Edge[] edges, string[] nodes)
        {
            this.id = id;
            this.associated_routine_id = associatedRoutineId;
            this.edges = edges;
            this.nodes = nodes;

        }
        
    }
}