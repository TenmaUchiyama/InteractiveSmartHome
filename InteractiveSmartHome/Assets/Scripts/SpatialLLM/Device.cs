using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Meta.Voice.NLayer;
using Oculus.Platform.Models;
using UnityEditor;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;




namespace SpatialLLM.Core{




public class Device : MonoBehaviour
{


   

    private DevicePositionalData debugDeviceData;
    public bool debug = false;
    private bool isVisible = false;

    private Color targetColor = Color.red;
    private Color originalColor = new Color(92/255,212/255,255/255,255/255);


    private Renderer renderer; 


    public bool IsVisible
    {
        get { return isVisible; }
    }

    private void Start() {
        renderer = GetComponent<Renderer>();

        debugDeviceData = new DevicePositionalData(Guid.NewGuid().ToString(), gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));
      
    }


    public DevicePositionalData GetDevicePositionalData()
    {
        debugDeviceData.position = new Position(transform.position);
        debugDeviceData.distance_from_user = Vector3.Distance(transform.position, Camera.main.transform.position);
        return debugDeviceData;
    }

    void OnBecomeVisible()
    {
        isVisible = true;
    
       if(debug)
       {
           Debug.Log("Visibleeeeeeeeeeeeeee");
       }
    }


    void OnBecomeInvisible()
    {
        isVisible = false;
    }



    public void ChangeColor() 
    {
        renderer.material.color = Color.red;
    }

    public void ResetColor()
    {
        renderer.material.color = Color.white;
    }
}
}