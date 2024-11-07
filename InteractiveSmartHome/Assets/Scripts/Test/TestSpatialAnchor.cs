using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks;
using UnityEngine;

public class TestSpatialAnchor : MonoBehaviour
{
 

  
  [SerializeField] Transform objectPosition; 
  [SerializeField] GameObject anchorPrefab;
  private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;
  private Dictionary<string, Guid> _nodeToAnchorMap = new Dictionary<string, Guid>();


  private void Start() {
    

    _spatialAnchorCore = GetComponent<SpatialAnchorCoreBuildingBlock>();


    _spatialAnchorCore.OnAnchorCreateCompleted.AddListener(OnAnchorCreateCompleted);
    _spatialAnchorCore.OnAnchorsLoadCompleted.AddListener(OnAnchorsLoadCompleted);


    _spatialAnchorCore.InstantiateSpatialAnchor(anchorPrefab, objectPosition.position, objectPosition.rotation);
  }

  

    private void Update() {
    if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
     
      Debug.Log("<color=red>PrimaryIndexTrigger</color>");
      this.SpawnAnchorObject(); 

    }


    if(OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
    {
      Debug.Log("<color=red>SecondaryHandTrigger</color>");
      this.LoadPrefs();
    }


    if(OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
    {
      Debug.Log("<color=red>PrimaryHandTrigger</color>");
      this.RemoveAllAnchors();
    }
  }

    public void SpawnAnchorObject()
    {
      
        _spatialAnchorCore.InstantiateSpatialAnchor(anchorPrefab, new Vector3(0f,0f,0f), Quaternion.identity);

    }


    

    public void OnAnchorCreateCompleted(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {

        int num =_nodeToAnchorMap.Count + 1; 
        string newId = "uuid" +num.ToString();
        PlayerPrefs.SetInt("Num", num);
        _nodeToAnchorMap[newId] = anchor.Uuid;
        PlayerPrefs.SetString(newId, _nodeToAnchorMap[newId].ToString());
        
        Debug.Log("<color=red>OnAnchorCreateCompleted</color>");
    }


    public void LoadPrefs()
    {

      int num = PlayerPrefs.GetInt("Num");
      Debug.Log($"<color=red>LoadPrefs {num}</color>");
      for (int i = 1; i <= num; i++)
      {
        string key = "uuid" + i.ToString();
        string myUuid = PlayerPrefs.GetString(key);
        Debug.Log($"<color=red>LoadPrefs {myUuid}</color>");
        _spatialAnchorCore.LoadAndInstantiateAnchors(anchorPrefab, new List<Guid>(){Guid.Parse(myUuid)});
      }

 
    }


    public void DeleteAnchor(Guid uuid)
    {
        _spatialAnchorCore.EraseAnchorByUuid(uuid);
    }

    public void RemoveAllAnchors() 
    {
        _spatialAnchorCore.EraseAllAnchors();
    }

    public void OnAnchorEraseCompleted(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
        Debug.Log($"<color=red>OnAnchorEraseCompleted</color>");
    }

    public void OnAnchorsLoadCompleted(List<OVRSpatialAnchor> anchors)
    {
      Debug.Log($"<color=red>OnLoad</color>");
        foreach(var anchor in anchors)
        {
            Debug.Log($"<color=red>OnLoad {anchor.Uuid}</color>");
        }
    }
    



}
