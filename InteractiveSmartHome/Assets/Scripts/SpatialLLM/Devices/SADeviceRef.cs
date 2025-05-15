using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialLLM.Device;
using UnityEngine;

public class SADeviceRef : Singleton<SADeviceRef>
{


    [SerializeField] GameObject parentObject;
    
    private List<SADevice> saDevices = new List<SADevice>();




    void Start()
    {
        saDevices = parentObject.GetComponentsInChildren<SADevice>(false).ToList(); 
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            foreach(SADevice device in saDevices)
            {
                device.TurnOff();
            }
        }
    }



    public List<SADevice> GetAllDevices()
    {
        return parentObject.GetComponentsInChildren<SADevice>(false).ToList(); 
    }



    public SADevice GetDeviceById(string id)
    {
        SADevice foundDevice = saDevices.Find(device => { 
            
            
            return device.GetDBDeviceData().device_id == id;});

        return foundDevice; 
    }


    public void AddSADevice(SADevice saDevice)
    {
        saDevice.transform.SetParent(parentObject.transform, worldPositionStays: true);

    }

    public GameObject GetSADeviceParent() 
    {
        return this.parentObject;
    }


    public List<SADevice> GetAllDevicesRealTime()
    {
        return parentObject.GetComponentsInChildren<SADevice>(false).ToList(); ;
    }



}
