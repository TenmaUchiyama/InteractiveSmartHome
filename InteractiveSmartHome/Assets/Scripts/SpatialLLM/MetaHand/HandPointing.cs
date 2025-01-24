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

public class HandPointing : MonoBehaviour
{
    [SerializeField] OVRHand rightHand; 
    // [SerializeField] GameObject beam;
    [SerializeField] RayInteractor rayInteractor;

    private LineRenderer rayRenderer;


    [SerializeField] private float maxRayLength = 10f; // Ray の最大長さ
    [SerializeField] private Color rayColor = Color.green; // Ray の色

    private LineRenderer lineRenderer;


        // [SerializeField, Interface(typeof(ISelector))]
        // private UnityEngine.Object _selector;
        private ISelector Selector;   



        bool isStateActivated = false;



         bool isVisibleStateChangedOnce =false;





    void Start()
    {

        // Selector.WhenSelected += HandleSelected;
        // Selector.WhenUnselected += HandleUnselected;


        SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);

        InitLineRenderer();
    }


    private void Update() {

        VisualizeRay();
    }




    void OnDestroy()
    {
        // Selector.WhenSelected -= HandleSelected;
        // Selector.WhenUnselected -= HandleUnselected;


        SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
    }

  private async void OnVoiceRecognized(string detected)
{
    try
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
                DBDeviceData dBDeviceData = device.GetDBDeviceData();
                Debug.Log($"<color=yellow>dBDeviceData: {dBDeviceData}</color>");

                PointingQueryDataType pointingQuery = new PointingQueryDataType
                {
                    user_message = detected,
                    device = dBDeviceData
                };

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                string json = JsonConvert.SerializeObject(pointingQuery, settings);
                Debug.Log($"<color=yellow>Send Query: {json}</color>");

                // LLM クエリ送信時のエラーハンドリング
                try
                {
                    await LLMQueryRequest.Instance.SendQuery(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"LLMQueryRequest failed: {e.Message}");
                }
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
    catch (Exception e)
    {
        Debug.LogError($"OnVoiceRecognized encountered an error: {e.Message}");
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



    private void VisualizeRay()
    {
        if (rayInteractor == null || lineRenderer == null )
            return;

        // Ray の始点と方向を取得
        Vector3 origin = rayInteractor.Origin;
        Vector3 direction = rayInteractor.Forward;
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

