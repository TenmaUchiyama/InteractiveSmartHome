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
                case DeviceColor.Orange: return new Color(1f, 0.5f, 0f); // オレンジ色
                case DeviceColor.Green: return Color.green;
                case DeviceColor.Purple: return new Color(0.5f, 0f, 0.5f); // 紫色
                case DeviceColor.Pink: return new Color(1f, 0.75f, 0.8f); // ピンク色
                case DeviceColor.Brown: return new Color(0.6f, 0.4f, 0.2f); // 茶色
                case DeviceColor.Black: return Color.black;
                case DeviceColor.Gray: return Color.gray; // グレー色
                case DeviceColor.Cyan: return Color.cyan;
                case DeviceColor.Magenta: return new Color(1f, 0f, 1f); // マゼンタ色
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
