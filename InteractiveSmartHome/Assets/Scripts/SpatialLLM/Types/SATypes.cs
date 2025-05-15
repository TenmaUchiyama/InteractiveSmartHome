using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;


namespace SpatialLLM.Type{




    public enum SADeviceType 
    {
        Light, 
        Curtain
    }

public static class DirectionUtil 
{
    public enum Direction
    {
        Front,
        Back,
        Up,
        Down,
        Right,
        Left
    }


    public static Dictionary<string, Direction> directionMap = new Dictionary<string, Direction>()
    {
        {"Front", Direction.Front},
        {"Back", Direction.Back},
        {"Up", Direction.Up},
        {"Down", Direction.Down},
        {"Right", Direction.Right},
        {"Left", Direction.Left}
    };

    public static Direction GetDirection(string direction)
    {
        return directionMap[direction];
    }

    public static string GetDirection(Direction direction)
    {
        return direction.ToString();
    }
}





public record DBDeviceData
{
    public string device_id { get; set; }
    public string device_type { get; set; }
    public string device_name { get; set; }

    public string anchor_id { get; set; }
    public string connector_type { get; set; }
    public string connector_topic { get; set; }
    public string description { get; set; } = "";

    public DBDeviceData(
        string device_id = "",
        string device_type = "",
        string device_name= "",
        string anchor_id = "",
        string connector_type = "",
        string connector_topic = "",
        string description =" ")
    {
        this.device_id = device_id;
        this.device_type = device_type;
        this.device_name = device_name;
        this.anchor_id = anchor_id;
        this.connector_type = connector_type;
        this.connector_topic = connector_topic;
        this.description = description;
    }
}



}