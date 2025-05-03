using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using SpatialLLM.Core;
using SpatialLLM.Device;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;

<<<<<<< HEAD



=======
>>>>>>> stack
public class HandPointing : MonoBehaviour
{
    [SerializeField] OVRHand rightHand; 
    // [SerializeField] GameObject beam;
    [SerializeField] RayInteractor rayInteractor;

    private LineRenderer rayRenderer;


    [SerializeField] private float maxRayLength = 10f; // Ray の最大長さ
    [SerializeField] private Color rayColor = Color.green; // Ray の色

    private LineRenderer lineRenderer;
    private PointingDevices pointingDevices;


        // [SerializeField, Interface(typeof(ISelector))]
        // private UnityEngine.Object _selector;
        private ISelector Selector;   



        bool isStateActivated = false;



        bool isPinchedOnce = false;





    void Start()
    {

        // Selector.WhenSelected += HandleSelected;
        // Selector.WhenUnselected += HandleUnselected;


        // SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);

        InitLineRenderer();

        pointingDevices = GetComponent<PointingDevices>();
    }










 private void Update()
    {
        

          DrawPointingLine();
        // ピンチが始まった瞬間を検出
        if (rightHand.IsPressed() && !isPinchedOnce)
        {
            isPinchedOnce= true;
            ToggleSADevice();
            pointingDevices.GetSelected();
        }

        // ピンチが解除されたらフラグをリセット
        if (!rightHand.IsPressed())
        {
            isPinchedOnce = false;
        }



    }

   private void ToggleSADevice()
    {
        // RayInteractor のヒット情報を取得
        SurfaceHit? hit = rayInteractor.CollisionInfo;
        if (!hit.HasValue) return;

        // Raycast の正しい origin と direction を設定
        Vector3 origin = hit.Value.Point + hit.Value.Normal * 0.01f; // 少しオフセットをつける
        Vector3 direction = -hit.Value.Normal;

        if (Physics.Raycast(origin, direction, out RaycastHit raycastHit, Mathf.Infinity))
        {
            string gameObjectName = raycastHit.collider.gameObject.name;
            Debug.Log($"<color=yellow>Hit GameObject Name: {gameObjectName}</color>");

            // 親オブジェクトも含めて SADevice を探す
            SADevice device = raycastHit.collider.GetComponentInParent<SADevice>();
            if (device != null)
            {
                Debug.Log("<color=yellow>SADevice component found on the hit object.</color>");

                // isDeviceSelected の状態を Toggle
                bool currentState = device.IsDeviceSelected();
                device.SetIsSelected(!currentState);
                Debug.Log($"<color=yellow>SADevice selection toggled: {currentState} -> {!currentState}</color>");
            }
            else
            {
                Debug.Log("<color=red>No SADevice component found on the hit object.</color>");
            }
        }
        else
        {
            Debug.Log("<color=red>Raycast did not hit any object.</color>");
        }
    }


    
    
    
    private void HandleUnselected()
    {
        isStateActivated = false;
    }
    private void HandleSelected()
    {
        isStateActivated = true;
    }





    private void InitLineRenderer() 
    {
        rayRenderer = gameObject.AddComponent<LineRenderer>();
        rayRenderer.startWidth = 0.01f;
        rayRenderer.endWidth = 0.01f;
        rayRenderer.material = new Material(Shader.Find("Unlit/Color"));
        rayRenderer.material.color = Color.white;
        rayRenderer.positionCount = 2;
    }


    private void DrawPointingLine() 
    {
        if(!rayRenderer) return;

        Vector3 startPos = rayInteractor.Origin; 
        Vector3 endPos =  rayInteractor.End; 

        rayRenderer.SetPosition(0, startPos);
        rayRenderer.SetPosition(1, endPos);
    }
}

