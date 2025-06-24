using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Json;
using MRFlow.Network;
using Oculus.Interaction;
using SpatialLLM.Core;
using SpatialLLM.Network;
using SpatialLLM.Type;
using TMPro;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;

namespace SpatialLLM.Device
{
    public abstract class SADevice : MonoBehaviour
    {
        [SerializeField] ShowLabel showLabel;
        
        protected DBDeviceData deviceData ; 
        protected DeviceSpatialData spatialData;

        protected SADeviceType saDeviceType;

        protected OperatingDeviceData currentOperatingData = new OperatingDeviceData(); 

        [SerializeField] protected bool isDeviceSelected = false;

        protected bool isDeviceOn = false; 

        public bool IsDeviceOn => isDeviceOn;

        public int HandleHover { get; private set; }

        public abstract void OperateDevice(OperatingDeviceData operatingDeviceData);

        public abstract void TurnOnWithColor(Color color);

        public abstract void TurnOff();

        public abstract void Init();

        protected DrawOnHover drawOnHover;


        public void TEMP_DrawHover()
        {

            this.drawOnHover.DrawHover(); 
    
        }


        
        public void TEMP_DrawUnHover()
        {

            this.drawOnHover.ClearDrawing(); 
    
        }
   

        public void DisplayShowLabel(bool showing)
        {
            if (showing)
            {
                this.showLabel.DisplayLabel();
            }
            else
            {
                this.showLabel.HideLabel();
            }
        }

        public DeviceSpatialData GenerateFurnitureRelativePositionData(Vector3 relativePos)
        {
         
            if(this.spatialData ==null)
            {
              
                return null;
            }
         
            Vector3 toObject = (transform.position - Camera.main.transform.position).normalized;
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            Vector3 cameraUp = Camera.main.transform.up;

          
            
            // DeviceSpatialData の生成
            DeviceSpatialData spatialData = new DeviceSpatialData(
                this.spatialData.id,
                this.spatialData.name,
                relativePos,
                Vector3.Distance(transform.position, Camera.main.transform.position),
                SpatialAwarnessProvider.Instance != null ? SpatialAwarnessProvider.Instance.GetFovCentralityScore(this) : (float?)null
            );
            return spatialData;
        }


        public DeviceSpatialDataForFurniture GenerateDeviceSpatialDataForFurniture(Vector3 relativePos,float distance)
        {
             
            if(this.spatialData ==null)
            {
              
                return null;
            }
         
            DeviceSpatialDataForFurniture spatialDataForFurniture = new DeviceSpatialDataForFurniture(
                this.spatialData.id,
                this.spatialData.name,
                relativePos,
              distance
            );

            return spatialDataForFurniture;
        }

 public DeviceSpatialData GetDevicePositionalRelativeToUser(Transform referenceCamera = null)
{
    Debug.Log("[SADevice] Start GetDevicePositionalRelativeToUser");

    try
    {
        Transform camTransform = referenceCamera != null ? referenceCamera : Camera.main.transform;

        // ワールド座標の差分
        Vector3 worldDelta = this.transform.position - camTransform.position;

        // Yaw（Y軸回転）のみ抽出
        Quaternion yawOnlyRotation = Quaternion.Euler(0, camTransform.eulerAngles.y, 0);

        // Yawのみに基づく相対位置（回転を逆適用してローカル化）
        Vector3 relativePosition = Quaternion.Inverse(yawOnlyRotation) * worldDelta;

        // 距離（ベクトルの長さ）そのまま
        float distance_from_user = worldDelta.magnitude;

        Debug.Log("[SADevice] Creating new DeviceSpatialData");
        Debug.Log($"[SADevice] <color=yellow> centrality_score: {(SpatialAwarnessProvider.Instance != null ? SpatialAwarnessProvider.Instance.GetFovCentralityScore(this) : (float?)null)}</color>");
        this.spatialData = new DeviceSpatialData(
            id: this.GetDeviceID(),
            name: this.gameObject.name,
            position: relativePosition, // 傾きの影響を排除した相対位置
            distance_from_user: distance_from_user,
            eye_centrality_score: SpatialAwarnessProvider.Instance != null ? SpatialAwarnessProvider.Instance.GetFovCentralityScore(this) : (float?)null
        );

        return this.spatialData;
    }
    catch (Exception ex)
    {
        Debug.LogError("[SADevice] Error getting positional data for device: " + this.name + " - " + ex.Message + "\n" + ex.StackTrace);
    }

    return null;
}


        public DeviceSpatialData GetDevicePositionalData() 
        {
            this.spatialData.position = new Position(new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z));
            this.spatialData.distance_from_user = Vector3.Distance(transform.position, Camera.main.transform.position);
            return this.spatialData;
        }

        public string GetDeviceID() 
        {
            return this.deviceData.device_id;
        }

        public DBDeviceData GetDBDeviceData() 
        {
            return this.deviceData;
        }


        public void SetDBDeviceData(DBDeviceData deviceData)
        {
            this.deviceData = deviceData;
        }

        public bool IsDeviceSelected()
        {
            return this.isDeviceSelected;
        }

        public void SetIsSelected(bool isSelected)
        {
            this.isDeviceSelected = isSelected;
        }

        public bool CompareDeviceType(string device_type)
        {
            return device_type.Equals(saDeviceType.ToString()) || device_type == "";
        }

        public SADeviceType GetSADeviceType()
        {
            return this.saDeviceType;
        }

        public OperatingDeviceData GetCurrentOperateData()
        {
            return this.currentOperatingData;
        }
    }
}