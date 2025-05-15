using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using UnityEngine;
using Newtonsoft.Json;
using SpatialLLM.Device;
using MRFlow.Network;
using SpatialLLM.Type;

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
        this.modelType = modelType;
        this.modelDeviceId = modelDeviceId;
        this.modelName = modelName;
    }

    public DeviceDataTemporary(string guid, string anchorId, string modelType, string modelDeviceId, string modelName)
    {
        this.deviceId = guid;
        this.anchorId = anchorId;
        this.modelType = modelType;
        this.modelDeviceId = modelDeviceId;
        this.modelName = modelName;
    }

    public string GetSerializedData()
    {
        return JsonConvert.SerializeObject(this);
    }
}

public class DeviceAnchoreManager : MonoBehaviour
{
    [SerializeField] GameObject devicePrefab;
    [SerializeField] ActionServerConnector actionServerConnector;

    private SpatialAnchorCore anchorCore;
    private GameObject latestCreatedObject;

    void Start()
    {
        anchorCore = GetComponent<SpatialAnchorCore>();
        anchorCore.OnAnchorCreateCompleted.AddListener(OnAnchorCreated);
        anchorCore.OnAnchorsLoadCompleted.AddListener(OnAnchorLoaded);
        anchorCore.OnAnchorsEraseAllCompleted.AddListener(OnAllErased);
    }

    void OnDestroy()
    {
        anchorCore.OnAnchorCreateCompleted.RemoveListener(OnAnchorCreated);
        anchorCore.OnAnchorsLoadCompleted.RemoveListener(OnAnchorLoaded);
        anchorCore.OnAnchorsEraseAllCompleted.RemoveListener(OnAllErased);
    }

    private void OnAllErased(OVRSpatialAnchor.OperationResult result)
    {
        Debug.Log($"<color=red> Erased all with result: {result}</color>");
    }

    private void OnAnchorLoaded(List<OVRSpatialAnchor> anchors)
    {
        if (anchors.Count > 0)
        {
            Debug.Log($"<color=yellow> LOADED : {anchors[0].Uuid}</color>");
        }
    }

    private async void OnAnchorCreated(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
    {
      
        if (result == OVRSpatialAnchor.OperationResult.Success && latestCreatedObject != null)
        {
            if (latestCreatedObject.TryGetComponent<SASwitchbot>(out var switchBot))
            {
                switchBot.GenerateDBDeviceData(anchor.Uuid.ToString());
                SADeviceRef.Instance.AddSADevice(switchBot.gameObject.GetComponent<SADevice>());
                await actionServerConnector.AddDevices(new List<DBDeviceData>{switchBot.GetDBDeviceData()});
                Debug.Log($"<color=yellow>Anchor Created & ID set: {anchor.Uuid.ToString()}</color>");
            }

            latestCreatedObject = null; // 使用後クリア
        }
        else
        {
            Debug.LogWarning($"<color=red>Anchor creation failed or no tracked object</color>");
        }
    }

    public async void  CreateAnchorOnPosition(Transform ghostPosition)
    {
    
        //  OVRSpatialAnchor anchor = anchoreObject.AddComponent<OVRSpatialAnchor>();
        //  anchor.transform.position = ghostPosition.position;
        //  anchor.transform.rotation = ghostPosition.rotation;
         anchorCore.InstantiateSpatialAnchor(devicePrefab, ghostPosition.position, ghostPosition.rotation);
         
    }




    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            anchorCore.EraseAllAnchors();
        }
    }

}
