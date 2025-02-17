using System;
using UnityEngine;
using System.Collections.Generic;
using SpatialLLM.Device;

namespace SpatialLLM.Experiment
{
    public enum SpatialType
    {
        ViewpointReference,
        PositionReference,
        DistanceReference,
        ObjectReference,
        HeightReference
    }

    public enum DeviceColor
    {
        White,
        Red,
        Blue,
        Yellow,
        Custom // カスタム色を選べるオプション
    }

    [Serializable]
    public class DeviceColorPair
    {
        public SADevice device;

        public DeviceColor color = DeviceColor.White;

        public Color GetFinalColor()
        {
            switch (color)
            {
                case DeviceColor.White: return Color.white;
                case DeviceColor.Red: return Color.red;
                case DeviceColor.Blue: return Color.blue;
                case DeviceColor.Yellow: return Color.yellow;
                default: return Color.white;
            }
        }
    }



    [Serializable]
    public class DeviceColorPairSerializable
    {
        public string deviceName;
        public string colorName;
    }
}
