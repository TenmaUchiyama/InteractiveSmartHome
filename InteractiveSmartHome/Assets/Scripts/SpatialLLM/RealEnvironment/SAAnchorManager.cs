using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRFlow.Network;
using SpatialLLM.Type;
using UnityEngine;
using System.Linq;


public class SAAnchorManager : MonoBehaviour
{
    [SerializeField] private AnchorSelector rightHandSelector;
    [SerializeField] private GameObject anchorPrefab; 
    [SerializeField] private ActionServerConnector actionServerConnector;


    private List<OVRSpatialAnchor> _anchors = new();
    private List<Guid> _savedUuids = new();

    private const string PlayerPrefsKey = "AnchorUUIDList";

    async void Start()
    {

        LoadAllAnchorsFromServer();


       
    }





public async void DeleteAllAnchors()
{
    Debug.Log("<color=red>Deleting all anchors</color>");

    List<OVRSpatialAnchor> toRemove = new List<OVRSpatialAnchor>(_anchors);

    foreach (var anchor in toRemove)
    {
        if (anchor != null)
        {
            var result = await anchor.EraseAnchorAsync();
            if (result.Success)
            {
                Debug.Log($"<color=red>Anchor deleted: {anchor.Uuid}</color>");
                Destroy(anchor.gameObject);
            }
            else
            {
                Debug.LogWarning($"Failed to delete anchor: {anchor.Uuid}");
            }
        }
    }

    _anchors.Clear();
    _savedUuids.Clear();
    PlayerPrefs.DeleteKey(PlayerPrefsKey);
    PlayerPrefs.Save();

    Debug.Log("<color=red>All anchors deleted and PlayerPrefs cleared</color>");
}


    public async void TryDeleteSelectedAnchor()
    {
        var target = rightHandSelector.currentCollidedAnchor;
        if(target.TryGetComponent<SASwitchbot>(out var switchbot))
        {
            string device_id = switchbot.GetDBDeviceData().device_id;
            await actionServerConnector.DeleteDevice(device_id); 
        }
        if (target != null)
        {
            Debug.Log($"<color=red>Trying to delete anchor: {target.Uuid}</color>");

            var result = await target.EraseAnchorAsync();
            if (result.Success)
            {
                Destroy(target.gameObject);
                Debug.Log($"<color=red>Anchor deleted: {target.Uuid}</color>");
                // UUIDリストからの削除やPlayerPrefs更新もここで対応可
            }
            else
            {
                Debug.LogWarning("Failed to erase anchor.");
            }
        }
        else
        {
            Debug.Log("No anchor selected to delete.");
        }
    }


    public async Task  CreateAnchorOnPosition(Transform spawnTransform)
{
    
    Debug.Log("<color=cyan>Anchor Creating at custom position</color>");
    var go = Instantiate(anchorPrefab, spawnTransform.position, spawnTransform.rotation,SADeviceRef.Instance.GetSADeviceParent().transform);

    var anchor = go.AddComponent<OVRSpatialAnchor>();

    while (!anchor.Localized) await Task.Yield();
    
    Debug.Log("<color=cyan>Anchore Localized</color>");

    _anchors.Add(anchor);
    _savedUuids.Add(anchor.Uuid);

    await SaveAnchorsAsync();
    Debug.Log("<color=cyan>Anchore Saved</color>");
    SaveUUIDs();

    Debug.Log("<color=cyan>Anchor Created at custom position</color>");

    SASwitchbot sASwitchbot = go.GetComponent<SASwitchbot>();
    string[] temp = {"9C9E6EDCDB72","9C9E6EDE6E06", "9C9E6EDE9D12"};

    if (sASwitchbot == null)
    {
         Debug.LogWarning("SASwitchBot component not found on instantiated anchor.");
    }else{
        sASwitchbot.CreateDeviceData(anchor.Uuid.ToString());
        Debug.Log($"<color=lime>CreateDeviceData called {sASwitchbot.GetDBDeviceData().anchor_id}</color>");
        DBDeviceData switchBotData = sASwitchbot.GetDBDeviceData(); 
        switchBotData.connector_topic = temp[SADeviceRef.Instance.GetAllDevices().Count-1];
        await actionServerConnector.AddDevices(new List<DBDeviceData>(){switchBotData});
    }

   
}

    private async void LoadAllAnchors()
    {
        if (_savedUuids.Count == 0)
        {
            Debug.Log("<color=orange>No Anchors to Load</color>");
            return;
        }

        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(_savedUuids.ToArray(), unboundAnchors);

        if (!result.Success)
        {
            Debug.LogWarning("<color=red>Failed to Load Anchors</color>");
            return;
        }

        foreach (var unbound in unboundAnchors)
        {
            bool localized = unbound.Localized || await unbound.LocalizeAsync();
            if (!localized) continue;

            // Prefabインスタンス化 & アンカーにBind
            Pose pose; 
            unbound.TryGetPose(out pose);
            var go = Instantiate(anchorPrefab, pose.position, pose.rotation);
            var anchor = go.AddComponent<OVRSpatialAnchor>();
            unbound.BindTo(anchor);
            _anchors.Add(anchor);

            Debug.Log($"<color=green>Anchor Restored: {anchor.Uuid}</color>");

            
        }
    }

private async void LoadAllAnchorsFromServer()
{
    Debug.Log("<color=cyan>Loading all anchors from ActionServerConnector...</color>");

    List<DBDeviceData> devices = await actionServerConnector.GetAllDevices();
     Debug.Log($"<color=cyan>{devices.Count}</color>");

    HashSet<Guid> anchorGuids = new();

    foreach (var device in devices)
    {


        Debug.Log($"<color=cyan>ID: {device.anchor_id} </color>");

        if (Guid.TryParse(device.anchor_id, out Guid uuid))
        {
            anchorGuids.Add(uuid);
        }
        else
        {
            Debug.LogWarning($"Invalid anchor_id format: {device.anchor_id}");
        }
    }

    var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
    var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(anchorGuids.ToList(), unboundAnchors);
    

    if (!result.Success)
    {
        Debug.LogError("<color=red>Failed to load anchors from server UUIDs.</color>");
        return;
    }
    Debug.Log($"<color=cyan>{unboundAnchors.Count}</color>");
    foreach (var unbound in unboundAnchors)
    {
        bool localized = unbound.Localized || await unbound.LocalizeAsync();
        if (!localized)
        {
            Debug.LogWarning("Anchor could not be localized.");
            continue;
        }

    // アンカーPrefabをインスタンス化してBind
        Pose pose; 
        unbound.TryGetPose(out pose);
        var go = Instantiate(anchorPrefab, pose.position, pose.rotation, SADeviceRef.Instance.GetSADeviceParent().transform);
        var anchor = go.AddComponent<OVRSpatialAnchor>();
        unbound.BindTo(anchor);

        _anchors.Add(anchor); // 管理リストに追加

        // デバイスを追加（SASwitchbotなどが前提）
        if (go.TryGetComponent<SASwitchbot>(out var switchBot))
        {
            var matchingDevice = devices.Find(d => d.anchor_id == anchor.Uuid.ToString());
            if (matchingDevice != null)
            {
                switchBot.SetDBDeviceData(matchingDevice);
                switchBot.GetComponent<DrawOnHover>().DrawHover();
                Debug.Log($"<color=green>Anchor & device restored: {matchingDevice.device_name}</color>");
            }
        }




        
    }
}


    private async Task SaveAnchorsAsync()
    {
        var result = await OVRSpatialAnchor.SaveAnchorsAsync(_anchors);
        if (!result.Success)
        {
            Debug.LogWarning("<color=red>Failed to Save Anchors</color>");
        }else{
            Debug.Log("<color=cyan>Save Success</color>");
        }
    }

    private void SaveUUIDs()
    {
        var json = JsonUtility.ToJson(new UUIDListWrapper { uuids = _savedUuids.ConvertAll(g => g.ToString()) });
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    private void LoadUUIDs()
    {
        _savedUuids.Clear();

        if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return;

        var json = PlayerPrefs.GetString(PlayerPrefsKey);
        var wrapper = JsonUtility.FromJson<UUIDListWrapper>(json);
        foreach (var uuidStr in wrapper.uuids)
        {
            if (Guid.TryParse(uuidStr, out Guid uuid))
            {
                _savedUuids.Add(uuid);
            }
        }
    }

    [Serializable]
    private class UUIDListWrapper
    {
        public List<string> uuids;
    }
}
