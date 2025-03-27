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
            Transform camTransfrom = referenceCamera != null ? referenceCamera : Camera.main.transform; 

            Vector3 relativePosition = camTransfrom.InverseTransformPoint(this.transform.position);
            this.furnitureData.position = new Position(new Vector3(relativePosition.x, relativePosition.y, relativePosition.z));
            this.furnitureData.distance_from_user = Vector3.Distance(transform.position, camTransfrom.position);
            return this.furnitureData;
        }


        public FurnitureData GetFurnitureData()
        {
            return furnitureData;
        }

        public bool CompareFurnitureType(string furniture_type)
        {
            return furniture_type.Equals(this.furnitureData.GetFurnitureTypeInString()) || furniture_type == "";
        }



    }
}