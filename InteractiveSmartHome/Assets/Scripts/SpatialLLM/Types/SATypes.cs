using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public string device_type { get; set;}
    public string device_name { get; set; }
    public string mqtt_topic { get; set; }
    public Vector3 device_position { get; set; }



    public DBDeviceData(string devcie_id,  string device_name, string device_type,string mqtt_topic, Vector3 device_position)
    {
        this.device_id = devcie_id;
        this.device_type = device_type;
        this.device_name = device_name;
        this.mqtt_topic = mqtt_topic;
        this.device_position = device_position;

    }
}



}