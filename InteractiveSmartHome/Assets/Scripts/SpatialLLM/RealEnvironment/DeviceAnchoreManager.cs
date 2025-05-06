using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Newtonsoft.Json;
using UnityEngine;



[Serializable]
public class DeviceDataTemporary 
{
    public string deviceId; 
    public string anchorId; 
    public string modelType;
    public string modelDeviceId; 
    public string modelName; 


    public DeviceDataTemporary(Guid guid, Guid anchorId, string modelType, string modelDeviceId, string modelName)
    {
        this.deviceId = guid.ToString();
        this.anchorId = anchorId.ToString();
        this.modelName = modelType;
        this.modelDeviceId = modelDeviceId;
        this.modelName = modelName;
            
    }

    public DeviceDataTemporary(string guid, string anchorId, string modelType, string modelDeviceId, string modelName)
    {
        this.deviceId = guid;
        this.anchorId = anchorId;
        this.modelName = modelType;
        this.modelDeviceId = modelDeviceId;
        this.modelName = modelName;
            
    }
    public string GetSerializedData()
    {
        return JsonConvert.SerializeObject(this);
    }

}


[RequireComponent(typeof(SpatialAnchorCoreBuildingBlock))]
public class DeviceAnchoreManager : MonoBehaviour
{

    [SerializeField] GameObject spawnObject;
    SpatialAnchorCoreBuildingBlock anchorCore;
    // Start is called before the first frame update


    void OnDestroy()
    {
        anchorCore.OnAnchorCreateCompleted.RemoveListener(OnAnchorCreated);
        anchorCore.OnAnchorsLoadCompleted.RemoveListener(OnAnchorLoaded);
        anchorCore.OnAnchorsEraseAllCompleted.RemoveListener(OnAllErased);
    }

    void Start()
    {
        anchorCore = GetComponent<SpatialAnchorCoreBuildingBlock>();


        anchorCore.OnAnchorCreateCompleted.AddListener(OnAnchorCreated);
        anchorCore.OnAnchorsLoadCompleted.AddListener(OnAnchorLoaded);
        anchorCore.OnAnchorsEraseAllCompleted.AddListener(OnAllErased);
    }

    private void OnAllErased(OVRSpatialAnchor.OperationResult arg0)
    {
        Debug.Log($"<color=red> Erased all with result: {arg0}</color>");
    }

    private void OnAnchorLoaded(List<OVRSpatialAnchor> arg0)
    {
       Debug.Log($"<color=yellow> LOADED : {arg0[0].Uuid.ToString()}");
    }

    private void OnAnchorCreated(OVRSpatialAnchor arg0, OVRSpatialAnchor.OperationResult arg1)
    {
        Debug.Log($"<color=yellow>{arg0.Uuid.ToString()}</color>");
    }

    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            anchorCore.InstantiateSpatialAnchor(spawnObject, spawnObject.transform.position, spawnObject.transform.rotation);
        }


        if(Input.GetKeyDown(KeyCode.L))
        {
            anchorCore.LoadAndInstantiateAnchors(spawnObject,new List<Guid>{new Guid("3d64aa70-0059-d983-14cf-571a76aa791d")});
        }
    }
}
