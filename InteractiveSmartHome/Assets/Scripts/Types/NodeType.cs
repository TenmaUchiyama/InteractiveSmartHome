

using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace NodeTypes
{

    public enum NodeType 
    {
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

    public enum HandlerType
    {
        HANDLER_OUT, 
        HANDLER_IN 
    }



    [Serializable]
    public record MRRoutineEdgeData
    {
        public Guid id {get; set;}
        public string  routine_id {get; set;}

        public string[] nodes {get; set;}
        public MREdgeData[] edges {get; set;}

        public MRRoutineEdgeData(Guid id, string routine_id, string[] nodes, MREdgeData[] edges)
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
        public object action_data { get; set; }
        public Vector3 position { get; set; }

    


        public  MRNodeData(Guid id, string type, object action_data, Vector3 position)
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
        public string id { get; set; }
        public string type { get; set; }
        public string data_action_id { get; set; }
        public NodePosition position { get; set; }


       public DBNode(string id, string type, string dataActionId, NodePosition position)
    {
        this.id = id;
        this.type = type;
        this.data_action_id = dataActionId;
        this.position = position;
    }

        
    }

    [Serializable]
    public record  Node
    {
        public string id { get; set; }
        public string type { get; set; }
        public object action_data { get; set; }
        public NodePosition position { get; set; }

        public Node(string id, string type, object action_data, NodePosition position)
        {
            this.id = id;
            this.type = type;
            this.action_data = action_data;
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