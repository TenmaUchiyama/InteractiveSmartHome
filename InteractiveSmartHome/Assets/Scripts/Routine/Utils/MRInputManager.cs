using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using MRFlow.Core;
using Newtonsoft.Json.Schema;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.Surfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MRInputManager : Singleton<MRInputManager>
{

    [SerializeField] GameObject hmd;
    [SerializeField] RayInteractor  leftRayInteractor;
    [SerializeField] RayInteractor  rightRayInteractor;
    private  ControllerRef  rightController;
    private ControllerRef leftController;
    public UnityEvent OnPrimaryButtonPressed;
 
        private float distanceToObject; // オブジェクトとの距離を保存
    private Transform targetObject; // ヒットしたオブジェクトを保存



    private void Start() {
        rightController = rightRayInteractor.transform.GetComponent<ControllerRef>();
        leftController = leftRayInteractor.transform.GetComponent<ControllerRef>();
        
    }


    public GameObject GetHmdObject()
    {
        return hmd;
    }
    // private void Update() {

    // return;         
    // if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
    // {
    // if (!rightRayInteractor.enabled)
    // {
    //     EdgeManager.Instance.SetSelectHandler(null);
    //     NodeManager.Instance.SetSelectedNodeUI(null);
    //     return;
    // }

    // Ray ray = rightRayInteractor.Ray;
    // OnPrimaryButtonPressed.Invoke();

    // // すべてのヒットを取得
    // RaycastHit[] hits = Physics.RaycastAll(ray);


    // foreach (RaycastHit hit in hits)
    // {
    //  Debug.Log($"<color=red>{hit.transform.name}</color>");
    //     if (hit.transform.TryGetComponent(out NodeHandler nodeHandler))
    //     {
    //        Debug.Log("<color=red>NodeHandler</color>");
    //         EdgeManager.Instance.SetSelectHandler(nodeHandler);
    //         return; 
    //     }

    //     // まだMRNodeUIが見つかっていない場合、チェックする
    //     if (hit.transform.TryGetComponent(out MRNodeUI mrNodeUI))
    //     {
    //         Debug.Log("MRNodeUI"); 
    //         NodeManager.Instance.SetSelectedNodeUI(mrNodeUI);
    //         return;
    //     }

       
    //     }
    // }
            

    

    // }


}
