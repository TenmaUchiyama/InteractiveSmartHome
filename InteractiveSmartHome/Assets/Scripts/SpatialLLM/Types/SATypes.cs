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
    public string device_type { get; set;}
    public string device_name { get; set; }
    public string description { get; set; }
    public string product_topic { get; set; }
    public Position device_position { get; set; }



    public DBDeviceData(string devcie_id,  string device_name, string device_type, string description, string mqtt_topic, Vector3 device_position)
    {
        this.device_id = devcie_id;
        this.device_type = device_type;
        this.device_name = device_name;
        this.description =description;
        this.product_topic = mqtt_topic;
        this.device_position = new Position(device_position);

    }



}


/// <summary>
/// Switchbotやmqtt等多数のデバイスを扱うためのデータ構造
/// </summary>
[Serializable]
public record DeviceDataForDB
{
    public string device_id { get; set; }
    public string anchor_id {get; set;}
    public string connection_type { get; set; }
    public string topic { get; set; }
    public string device_type { get; set;}
    public string device_name { get; set; }
    public string description { get; set; }
    public Position device_position { get; set; }



    public DeviceDataForDB(string device_id, string anchor_id, string device_model, string topic, string device_type, string device_name, string description, Vector3 device_position)
    {
        this.device_id = device_id;
        this.anchor_id = anchor_id;
        this.connection_type = device_model;
        this.topic = topic;
        this.device_type = device_type;
        this.device_name = device_name;
        this.description = description;
        this.device_position = new Position(device_position);
    }


}


}


