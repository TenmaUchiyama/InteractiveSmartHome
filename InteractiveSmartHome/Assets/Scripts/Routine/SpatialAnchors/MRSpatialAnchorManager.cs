using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;
namespace MRFlow.SpatialAnchors
{
public class MRSpatialAnchorManager : Singleton<MRSpatialAnchorManager>
{
  private SpatialAnchorCoreBuildingBlock spatialAnchorCore;

  private GameObject anchoreObject = new GameObject();
  
  private const string ANCHOR_NAME = "base_anchor";



  private void Start() {
    spatialAnchorCore = GetComponent<SpatialAnchorCoreBuildingBlock>();

    spatialAnchorCore.OnAnchorsEraseAllCompleted.AddListener(OnAnchorsErasedAllCompleted);
    spatialAnchorCore.OnAnchorCreateCompleted.AddListener(OnAnchorCreateCompleted);
    spatialAnchorCore.OnAnchorsLoadCompleted.AddListener(OnAnchorsLoadCompleted);



    
    bool isLoadedSuccessfully = LoadAnchor();
    if (!isLoadedSuccessfully)
    {
      SpawnNewAnchor();
    }

    spatialAnchorCore.EraseAllAnchors();

    Debug.Log($"<color=green>Anchor Start</color>");
  }

    private void OnAnchorsErasedAllCompleted(OVRSpatialAnchor.OperationResult arg0)
    {
        Debug.Log($"<color=green>Anchor erased successfully</color>");
        PlayerPrefs.DeleteKey(ANCHOR_NAME);
    }

    private void OnAnchorsLoadCompleted(List<OVRSpatialAnchor> arg0)
    {
       Debug.Log($"<color=green>Anchor loaded successfully {arg0[0].Uuid}</color>");
    }

    private void OnAnchorCreateCompleted(OVRSpatialAnchor arg0, OVRSpatialAnchor.OperationResult arg1)
    {
        Debug.Log($"<color=green>Anchor created successfully {arg0.Uuid}</color>");
        PlayerPrefs.SetString(ANCHOR_NAME, arg0.Uuid.ToString());
    }


public Vector3 ConvertAnchorPositionToTargetRelativePosition(Vector3 anchorPosition, Transform targetTransform)
{
    if (anchoreObject == null || targetTransform == null)
    {
        Debug.Log("Anchor or target transform is null.");
        return Vector3.zero;
    }

    Vector3 worldPosition = anchoreObject.transform.TransformPoint(anchorPosition);

    Vector3 localPositionRelativeToTarget = targetTransform.InverseTransformPoint(worldPosition);

    return localPositionRelativeToTarget;
}


    public Vector3 ConvertToAnchorRelativePosition(Vector3 targetPosition)
{
    if (anchoreObject == null || targetPosition == null)
    {
        Debug.LogWarning("Anchor or target object is null.");
        return Vector3.zero;
    }

    Vector3 localPositionRelativeToAnchor = anchoreObject.transform.InverseTransformPoint(targetPosition);

  

    return localPositionRelativeToAnchor;
}




    public void SpawnNewAnchor()
  {

    spatialAnchorCore.InstantiateSpatialAnchor(anchoreObject, transform.position, transform.rotation);

  }

  public bool LoadAnchor() 
  {
    string anchorId = PlayerPrefs.GetString(ANCHOR_NAME);
    if (string.IsNullOrEmpty(anchorId))
    {
      Debug.Log("<color=red>Anchor not found</color>");
      return false;
    }
    Debug.Log($"<color=green>Anchor found {anchorId}. Loading</color>");
    Guid anchorUuid = new Guid(anchorId);   
    spatialAnchorCore.LoadAndInstantiateAnchors(anchoreObject, new List<Guid> { anchorUuid });
    return true;
  }
}
}