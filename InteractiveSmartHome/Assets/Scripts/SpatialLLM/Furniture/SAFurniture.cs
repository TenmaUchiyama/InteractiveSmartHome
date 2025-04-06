using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;
using static SpatialLLM.Network.NetworkDataType;
using Oculus.Interaction.Samples;
using JetBrains.Annotations;


namespace SpatialLLM.Core
{
    public enum FurnitureType
    {
        TABLE,
        TV,

    }


    [Serializable]
    public class FurnitureShape 
    {
        public float width {get; set;}
        public float height {get; set;}
        public float depth {get; set;}


        public FurnitureShape(float x, float y, float z)
        {
            width = x;
            height = y;
            depth = z;
        }


    }

    [Serializable]
    public class FurnitureData
    {
        public string id { get; private set; }
        public string name { get; private set; }


         [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public FurnitureType FurnitureType { get; private set; }

        public Position position; 

        public FurnitureShape furnitureShape;
        
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
            furnitureData.furnitureShape = this.GetBoundingBoxDimentions(); 
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

    public Vector3 GetBoundingBoxDimentions_Vector()
    {
        // 子オブジェクトも含めた全Rendererコンポーネントを取得
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Rendererが見つからなければ、サイズはゼロとする
        if (renderers.Length == 0)
        {
            return Vector3.zero;
        }

        // 最初のRendererのboundsを初期値とする
        Bounds combinedBounds = renderers[0].bounds;

        // 残りのRendererのboundsを統合する
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        // combinedBounds.sizeには、幅(x)、高さ(y)、奥行き(z)が格納される
        return combinedBounds.size;
    }

    public FurnitureShape GetBoundingBoxDimentions()
    {
        Vector3 boundingBox = this.GetBoundingBoxDimentions_Vector();
    
        return new FurnitureShape(boundingBox.x, boundingBox.y, boundingBox.z); 
    }


    }
}