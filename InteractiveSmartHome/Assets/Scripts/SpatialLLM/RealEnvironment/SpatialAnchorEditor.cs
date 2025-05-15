using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using UnityEngine;

public class SpatialAnchorEditor : MonoBehaviour
{
    [SerializeField] GameObject spawnOffset; 

    [SerializeField] SAAnchorManager saanchorManager;


    [SerializeField] GameObject testSpawnPos;

    
    private bool isVisualized  = true;

    void Start()
    {
        if(saanchorManager == null)saanchorManager = GetComponent<SAAnchorManager>();
        
    }



    public async void TestSpawn() 
    {
        Debug.Log("Creating");
         await saanchorManager.CreateAnchorOnPosition(testSpawnPos.transform);
    }
    
    async void Update()
    {







         if (Input.GetKeyDown(KeyCode.Delete))
        {
            saanchorManager.DeleteAllAnchors();
        }
       if (Input.GetKeyDown(KeyCode.Space))
        {
               Debug.Log("<color=yellow>R Triggered</color>");
            await saanchorManager.CreateAnchorOnPosition(testSpawnPos.transform);
        }


        
        if(OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
         {  
            Debug.Log("<color=yellow>R Triggered</color>");
            await saanchorManager.CreateAnchorOnPosition(spawnOffset.transform);
        }



           if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Debug.Log("B PRESSED");
            saanchorManager.TryDeleteSelectedAnchor();
        }
         if(OVRInput.GetDown(OVRInput.RawButton.X))
         {

            Debug.Log("X PRESSED");
            Debug.Log(SADeviceRef.Instance.GetAllDevicesRealTime().Count);
            foreach(SADevice device in SADeviceRef.Instance.GetAllDevicesRealTime())
            {
                if(device.TryGetComponent<DrawOnHover>(out var drawOnHover));
                {
                    if(drawOnHover.isVisible())
                    {
                        drawOnHover.DrawUnhover();
                        
                    }else{
                        drawOnHover.DrawHover();
                        
                    }
                }
            }
         }





    }
}
