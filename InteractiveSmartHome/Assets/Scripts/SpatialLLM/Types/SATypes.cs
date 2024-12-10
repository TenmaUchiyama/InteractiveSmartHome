using System.Collections;
using System.Collections.Generic;
using UnityEngine;


  namespace SpatialLLM.Type{

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
}