using System;
using UnityEngine;
using System.Collections.Generic;
using SpatialLLM.Device;

namespace SpatialLLM.Experiment
{
    public enum SpatialType
    {
        ViewpointReference,
        DirectionalReference,
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
        Orange, 
        Green, 
        Purple, // 新しい色を追加
        Pink, // 新しい色を追加
        Brown, // 新しい色を追加
        Black, // 新しい色を追加
        Gray, // 新しい色を追加
        Cyan, 
        Magenta, // 新しい色を追加
        
        Custom // カスタム色を選べるオプション
    }

    [Serializable]
    public class DeviceColorPair
    {
        public SADevice device;

        public DeviceColor color = DeviceColor.White;

        public Color customColor = new Color(1f, 1f, 1f, 1f); // カスタム色を指定するためのフィールド

        public Color GetUnityColor()
        {
            switch (color)
            {
                case DeviceColor.White: return Color.white;
                case DeviceColor.Red: return Color.red;
                case DeviceColor.Blue: return Color.blue;
                case DeviceColor.Yellow: return Color.yellow;
                case DeviceColor.Custom:
                    // カスタム色の場合は、デバイスの色を返す
                    return customColor;
                default: return Color.white;
            }
        }
    }



    [Serializable]
    public class DeviceColorPairSerializable
    {
        public string deviceName;
        public string colorName;
        public string deviceId;
    }
}
