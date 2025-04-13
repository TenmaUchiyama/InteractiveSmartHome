using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialLLM.Core;
using SpatialLLM.Type;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Internal;




namespace SpatialLLM.Network
{
public static class NetworkDataType 
{

   



[Serializable]
  public class DeviceSpatialData
    {
        public string id;
        public string name; 
    
     
        public Position position; 
        public float distance_from_user;



        public DeviceSpatialData(string id , string name, Vector3 position, float distance_from_user, float angle = 0)
        {
            this.id = id;
            this.name = name;
            this.position = new Position(position);
            this.distance_from_user = distance_from_user;
      
        }
   

    }




[Serializable]
    public class Position
{
    public float x;
    public float y;
    public float z;

    public Position(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }
}

public enum RequestType
    {
        LlmAgent,
        Device,
        Function
    }


     public class Message
    {
        public RequestType RequestType { get; set; }
        public object Data { get; set; }  // Dataは任意の型を持つことができる

        public Message(RequestType requestType, object data)
        {
            RequestType = requestType;
            Data = data;
        }
    }



   



     public class LLMServerDataUtil
    {
        public enum LLMServerAction
        {
            RespondDeviceData,
            LLMQuery
        }


        public static Dictionary<string, LLMServerAction> actionMap = new Dictionary<string, LLMServerAction>()
        {
            {"device", LLMServerAction.RespondDeviceData},
            {"llm_agent", LLMServerAction.LLMQuery}
        };

        public static LLMServerAction GetLLMServerActionType(string action)
        {
            return actionMap[action];
        }
        public static string GetLLMServerActionStr(LLMServerAction action)
        {
            // actionMap の中から value が action と一致する最初のキーを取得
            var key = actionMap.FirstOrDefault(x => x.Value == action).Key;
            
            // キーが見つからなかった場合の処理（必要に応じて例外を投げるなど）
            if (key == null)
            {
                throw new ArgumentException($"Action '{action}' に対応するキーが見つかりません。");
            }

            return key;
        }







    public enum FunctionType
    {
        Direction, 
        Sight
    }



    public record FunctionMsgType
    {
     public string function; 
     public List<string> args;

     public FunctionMsgType(string function, List<string> args)
     {
         this.function = function;
         this.args = args;
     }

    }


    public static Dictionary<string, FunctionType> functionMap = new Dictionary<string, FunctionType>()
    {
        {"direction", FunctionType.Direction},
        {"sight", FunctionType.Sight}
    };


    public static FunctionType GetFunctionType(string function)
    {
        return functionMap[function];
    }


    public static string GetFunctionTypeStr(FunctionType function)
    {
        var key = functionMap.FirstOrDefault(x => x.Value == function).Key;
        if (key == null)
        {
            throw new ArgumentException($"Function '{function}' に対応するキーが見つかりません。");
        }

        return key;
    }






    }




[Serializable]
public record ColorData
{
    public int r { get; set; }
    public int g { get; set; }
    public int b { get; set; }
}


    public record OperatingDeviceData
    {
        public string id;
        public bool state;
        public int? intensity;
        public ColorData? color;
    }





    [Serializable]
    public record  PointingQueryDataType
    {   
        public string user_message; 
        public DBDeviceData device;
    }


    [Serializable]
    public record DeviceLabel
    {
        public string id; 
        public string name;
        public string type;
    }
    
    [Serializable]
    public record LabelQueryDataType 
    {
        public string user_message;
        public List<DeviceLabel> devices;
    }




}
}