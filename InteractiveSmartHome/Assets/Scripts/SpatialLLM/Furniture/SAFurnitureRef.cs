using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialLLM.Core;
using UnityEngine;

public class SAFurnitureRef : Singleton<SAFurnitureRef>
{
   

    [SerializeField] GameObject parentPrefab;
    
    private List<SAFurniture> saFurnitures = new List<SAFurniture>();




    void Start()
    {
        saFurnitures = parentPrefab.GetComponentsInChildren<SAFurniture>(false).ToList();
    }



<<<<<<< HEAD

=======
>>>>>>> stack
    public List<SAFurniture> GetAllSAFurnitures()
    {
        return saFurnitures;
    }



    public SAFurniture GetFurnitureByID(string id)
    {
        SAFurniture foundFurniture = saFurnitures.Find(furniture => { 
            
            Debug.Log($"<color=red>Furniture ID: {furniture.GetFurnitureData().id}, Compared_to: {id}</color>");
            return furniture.GetFurnitureData().id == id;
            
            });

        return foundFurniture; 
    }
}
