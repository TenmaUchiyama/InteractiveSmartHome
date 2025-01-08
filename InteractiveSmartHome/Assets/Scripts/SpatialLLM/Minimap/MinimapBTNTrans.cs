using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class MinimapBTNTrans : MonoBehaviour
{
   [SerializeField] OVRSkeleton skeleton; 
   [SerializeField] GameObject minimapBtn; 
   [SerializeField] Vector3 offset = new Vector3(0f, -0.05f, -0.1f);
   [SerializeField] Quaternion rotationOffset = Quaternion.Euler(0, 180, 0);



    [SerializeField] GameObject minimapCanvas;
   [SerializeField] ActiveStateUnityEventWrapper activeStateUnityEventWrapper;

   [SerializeField] InteractableUnityEventWrapper interactableUnityEventWrapper;
   
    

    private bool isActivated = false;





    private void Start() {
        activeStateUnityEventWrapper.WhenActivated.AddListener(()=>{isActivated = true; Debug.Log("Activated");});
        activeStateUnityEventWrapper.WhenDeactivated.AddListener(()=>{isActivated = false; Debug.Log("Deactivated");}); 

        interactableUnityEventWrapper.WhenSelect.AddListener(()=>{minimapCanvas.SetActive(!minimapCanvas.activeSelf);});
    }
   private void Update() {


      if(!(skeleton.Bones.Count > 0)){
            if(this.minimapBtn.activeSelf){this.minimapBtn.SetActive(false);}
            return;
      }


      if (this.minimapBtn.activeSelf != isActivated)
    {
        this.minimapBtn.SetActive(isActivated);
    }


      Transform wristTrans = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform;
      Vector3 wristPosition = wristTrans.position;
      Quaternion wristRotation = wristTrans.rotation;

    Vector3 targetPos = wristPosition + wristTrans.up * offset.y + wristTrans.forward * offset.z;

    this.minimapBtn.transform.position = targetPos;
    this.minimapBtn.transform.rotation = wristTrans.rotation * rotationOffset;

    
   }
}
