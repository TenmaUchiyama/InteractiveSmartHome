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
    [SerializeField] OVRSkeleton rightHand; 
    // [SerializeField] GameObject beam;
    [SerializeField] RayInteractor rayInteractor;


    [SerializeField] private float maxRayLength = 10f; // Ray の最大長さ
    [SerializeField] private Color rayColor = Color.green; // Ray の色

    private LineRenderer lineRenderer;


        // [SerializeField, Interface(typeof(ISelector))]
        // private UnityEngine.Object _selector;
        private ISelector Selector;   



        bool isStateActivated = false;



         bool isVisibleStateChangedOnce =false;


    private void Awake() {
        // // // Selector = _selector as ISelector;
        CreateLineRenderer(); 
    }


    void Start()
    {

        // Selector.WhenSelected += HandleSelected;
        // Selector.WhenUnselected += HandleUnselected;


        SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
    }


    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("<color=green>Space Button Pressed </color>");
            OnVoiceRecognized("あれ取って。");
        }

        Debug.Log($"<color=yellow>{rightHand.Bones.Count}</color>");
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
        SurfaceHit? hit = rayInteractor.CollisionInfo;
        


        if (!hit.HasValue) return; 
          // Raycastの結果を直接利用する場合
            Ray ray = new Ray(hit.Value.Point - hit.Value.Normal * hit.Value.Distance, hit.Value.Normal);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {


                // ヒットしたオブジェクトの名前を取得
                string gameObjectName = raycastHit.collider.gameObject.name;
                Debug.Log($"<color=yellow>Hit GameObject Name: {gameObjectName}</color>");

                // SADeviceコンポーネントを取得
                SADevice device = raycastHit.collider.GetComponent<SADevice>();
                if (device != null)
                {
                    Debug.Log("<color=yellow>SADevice component found on the hit object.</color>");
                DBDeviceData dBDeviceData = device.GetDBDeviceData();
                Debug.Log($"<color=yellow>dBDeviceData: {dBDeviceData}</color>");

          
                  
                PointingQueryDataType pointingQuery = new PointingQueryDataType(); 
                pointingQuery.user_message = detected; 
                pointingQuery.device = dBDeviceData; 

                JsonSerializerSettings settings = new JsonSerializerSettings(){
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                   string json = JsonConvert.SerializeObject(pointingQuery, settings);
                     Debug.Log($"<color=yellow>Send Query: {json}</color>");
                   await LLMQueryRequest.Instance.SendQuery(json);

                }
                else
                {
                    Debug.Log("No SADevice component found on the hit object.");
                }
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




      private void CreateLineRenderer()
    {
        // GameObject に LineRenderer を追加
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // LineRenderer の基本設定
        lineRenderer.positionCount = 2; // 始点と終点
        lineRenderer.startWidth = 0.005f; // 始点の幅
        lineRenderer.endWidth = 0.005f;   // 終点の幅
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // シンプルなマテリアル
        lineRenderer.startColor = rayColor; // 始点の色
        lineRenderer.endColor = rayColor;   // 終点の色
        lineRenderer.useWorldSpace = true;  // ワールド座標で描画
    }

    private void VisualizeRay()
    {
        if (rayInteractor == null || lineRenderer == null )
            return;

        // Ray の始点と方向を取得
        Vector3 origin = rayInteractor.Origin;
        Vector3 direction = rayInteractor.Forward;

        // LineRenderer の始点と終点を設定
        lineRenderer.SetPosition(0, origin); // 始点を設定
        lineRenderer.SetPosition(1, origin + direction * maxRayLength); // 終点を設定
    }
}
