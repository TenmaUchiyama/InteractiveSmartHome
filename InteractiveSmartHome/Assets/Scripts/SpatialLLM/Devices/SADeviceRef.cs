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
<<<<<<< HEAD
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     foreach(SADevice device in saDevices)
        //     {
        //         device.TurnOff();
        //     }
        // }
=======
        if(Input.GetKeyDown(KeyCode.O))
        {
            foreach(SADevice device in saDevices)
            {
                device.TurnOff();
            }
        }
>>>>>>> stack
    }



    public List<SADevice> GetAllDevices()
    {
        return saDevices;
    }



    public SADevice GetDeviceById(string id)
    {
        SADevice foundDevice = saDevices.Find(device => { 
            
            Debug.Log($"<color=red>Device ID: {device.GetDBDeviceData().device_id}, Compared_to: {id}</color>");
            return device.GetDBDeviceData().device_id == id;});

        return foundDevice; 
    }

}
