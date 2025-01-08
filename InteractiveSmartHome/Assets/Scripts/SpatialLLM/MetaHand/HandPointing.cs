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


        // [SerializeField, Interface(typeof(ISelector))]
        // private UnityEngine.Object _selector;
        private ISelector Selector;   



        bool isStateActivated = false;



         bool isVisibleStateChangedOnce =false;


    private void Awake() {
        // // // Selector = _selector as ISelector;
    }


    void Start()
    {

        // Selector.WhenSelected += HandleSelected;
        // Selector.WhenUnselected += HandleUnselected;


        SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
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

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("<color=green>Space Button Pressed </color>");
            OnVoiceRecognized("あれ取って。");
        }
    }
    // Update is called once per frame
    // void Update()
    // {
        
      
    //     return;
        
    //     bool isRightHandVisible = rightHand.Bones.Count > 0 && isStateActivated; 



    //     if (isRightHandVisible != isVisibleStateChangedOnce)
    //     {
    //         isVisibleStateChangedOnce = isRightHandVisible;
    //         beam.SetActive(isRightHandVisible);
    //     }

    //     if (!isRightHandVisible)
    //     {
    //         return;
    //     }

    //     Vector3 intermediatePhalanxPosition  = rightHand.Bones[7].Transform.position;
    //     Vector3 indexTipPosition  = rightHand.Bones[(int)OVRSkeleton.BoneId.XRHand_IndexTip].Transform.position;


    //     //まずはDirectionを見つけてNormalizeする
    //     Vector3 direction = indexTipPosition - intermediatePhalanxPosition;
    //     float distance = direction.magnitude;
    //     direction.Normalize(); 


    //     float beamLength = beam.transform.localScale.z;

    //     //オブジェクトの位置を更新する。
    //     beam.transform.position = intermediatePhalanxPosition + direction * (beamLength * 0.5f + distance);
    //     beam.transform.rotation = Quaternion.LookRotation(direction);

    //     Ray ray = new Ray(indexTipPosition , direction );

    //     RaycastHit hit; 

    //     if(Physics.Raycast(ray, out hit))
    //     {
    //         // Debug.Log($"Hit: {hit.collider.gameObject.name}");
    //     }
        
    // }
}
