using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;
using static SpatialLLM.Network.NetworkDataType;


namespace SpatialLLM.Core
{
    public enum FurnitureType
    {
        TABLE,
        TV,
        SHELF

    }

    [Serializable]
    public class FurnitureData
    {
        public string id { get; private set; }
        public string name { get; private set; }


         [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public FurnitureType FurnitureType { get; private set; }

        public Position position; 
        
        public float distance_from_user;

        public FurnitureData(string id, string name, FurnitureType furnitureType)
        {
            this.id = id;
            this.name = name;
            FurnitureType = furnitureType;
        }

        public string ToStringRepresentation()
        {
            return $"Id: {id}, Name: {name}, FurnitureType: {FurnitureType}";
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        

        public string GetFurnitureTypeInString() 
        {
            return FurnitureData.FurnitureTypeEnumToString(this.FurnitureType);
        }

        public static FurnitureData FromJson(string json)
        {
            return JsonConvert.DeserializeObject<FurnitureData>(json);
        }


        public static string FurnitureTypeEnumToString(FurnitureType furnitureType)
        {
            return furnitureType.ToString();
        }

        public static FurnitureType FurnitureTypeStringToEnum(string furnitureType)
        {
            return (FurnitureType)Enum.Parse(typeof(FurnitureType), furnitureType);
        }
    }

    public class SAFurniture : MonoBehaviour
    {
        [SerializeField] FurnitureType furnitureType; 
        private FurnitureData furnitureData;

        void Awake()
        {
            furnitureData = new FurnitureData(Guid.NewGuid().ToString(), this.gameObject.name, furnitureType);

        }

        void Start()
        {
            Debug.Log($"<color=yellow>Furniture Data: {furnitureData.ToStringRepresentation()}, Json: {furnitureData.ToJson()}</color>");
        }

        void Update()
        {
        
        }
     public FurnitureData GetFurniturePositionalRelativeToUser(Transform referenceCamera = null)
{

    Debug.Log("<color=green>=========================================================================================</color>");
    Transform camTransform = referenceCamera != null ? referenceCamera : Camera.main.transform;


    Vector3 toFurniture = camTransform.position - this.transform.position;

    
    Vector3 userRight = camTransform.right;     
    Vector3 userUp = camTransform.up;          
    Vector3 userForward = camTransform.forward;


    float relativeX = Vector3.Dot(toFurniture, userRight);    
    float relativeY = Vector3.Dot(toFurniture, userUp);      
    float relativeZ = Vector3.Dot(toFurniture, userForward);   

    Vector3 relativePos = new Vector3(relativeX, relativeY, relativeZ);


    this.furnitureData.position = new Position(relativePos);
    this.furnitureData.distance_from_user = Vector3.Distance(transform.position, camTransform.position);

    return this.furnitureData;
}



        public FurnitureData GetFurnitureData()
        {
            return furnitureData;
        }

        public bool CompareFurnitureType(string furniture_type)
        {
            return furniture_type.Equals(this.furnitureData.GetFurnitureTypeInString());
        }



    }
}