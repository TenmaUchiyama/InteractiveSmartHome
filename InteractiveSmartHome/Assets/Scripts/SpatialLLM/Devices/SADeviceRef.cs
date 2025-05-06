using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialLLM.Device;
using UnityEngine;

public class SADeviceRef : Singleton<SADeviceRef>
{


    [SerializeField] GameObject parentPrefab;
    
    private List<SADevice> saDevices = new List<SADevice>();




    void Start()
    {
        saDevices = parentPrefab.GetComponentsInChildren<SADevice>(false).ToList(); 
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
        return saDevices;
    }



    public SADevice GetDeviceById(string id)
    {
        SADevice foundDevice = saDevices.Find(device => { 
            
            
            return device.GetDBDeviceData().device_id == id;});

        return foundDevice; 
    }

}
