using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using Oculus.Interaction.Input.Visuals;
using UnityEditor;
using UnityEngine;

public class MenuSpawner : MonoBehaviour
{
    
   
   [SerializeField] GameObject menuGroup;
   
    public float distanceFromCamera = 1.5f;


private void Update() {


    if(OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.LTouch)){
        OpenCloseMenu(); 
    }
}



private void OpenCloseMenu() 
{
    Debug.Log("Open the Menu");
    if(menuGroup.activeSelf){
        menuGroup.SetActive(false);
        return;
    }

    Transform cameraTransform = Camera.main.transform;
    menuGroup.transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
    menuGroup.transform.LookAt(cameraTransform);
    menuGroup.transform.Rotate(0, 180, 0);
    menuGroup.SetActive(true);

}
    
}
