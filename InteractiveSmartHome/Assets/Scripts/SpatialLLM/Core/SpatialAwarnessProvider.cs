using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActionDataTypes;
using SpatialLLM.Device;
using SpatialLLM.Network;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;
using static SpatialLLM.Type.DirectionUtil;

namespace SpatialLLM.Core
{
    
    
    
    

public class FOVDeviceDetectorUtil
{

    private Camera userCamera;

    private float horizontalFOV = 70f;
    private float verticalFOV = 70f;

    // 視線方向のオフセット（度単位）
    private float horizontalAngleOffset = 0f; // 例: 右に5度傾けたい → +5
    private float verticalAngleOffset = 0f;   // 例: 下に10度傾けたい → -10
    private float pValue = 5f; // 丸みの強さ（2: 楕円, ∞: 長方形, 4〜6: 角丸推奨）
    public FOVDeviceDetectorUtil(Camera cam, float hFov = 70f, float vFov = 70f, float hOffset = 0f, float vOffset = 0f, float p = 5f)
    {
        userCamera = cam;
        horizontalFOV = hFov;
        verticalFOV = vFov;
        horizontalAngleOffset = hOffset;
        verticalAngleOffset = vOffset;
        pValue = p;
    }




private Vector3 GetAdjustedForward()
{
    // ユーザーの視線方向を基準に、pitch/yawオフセットを適用
    Quaternion offsetRotation = Quaternion.Euler(verticalAngleOffset, horizontalAngleOffset, 0f);
    return userCamera.transform.rotation * offsetRotation * Vector3.forward;
}


/// <summary>
/// 丸みを持った視野内判定（ローカル空間 + オフセット対応）
/// </summary>
private bool IsWithinRoundedFov(Vector3 targetPos)
{
    Vector3 dirToTarget = (targetPos - userCamera.transform.position).normalized;

    // ローカル空間に変換（カメラ空間基準）
    Vector3 localDir = Quaternion.Inverse(userCamera.transform.rotation) * dirToTarget;

    // オフセットを加味
    Quaternion offsetRotation = Quaternion.Euler(verticalAngleOffset, horizontalAngleOffset, 0f);
    localDir = Quaternion.Inverse(offsetRotation) * localDir;

    float hAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
    float vAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

    float a = horizontalFOV * 0.5f;
    float b = verticalFOV * 0.5f;

    float normalized = Mathf.Pow(Mathf.Abs(hAngle / a), this.pValue) + Mathf.Pow(Mathf.Abs(vAngle / b), this.pValue);

    return normalized <= 1f;
}
    
 /// <summary>
/// 中心からのスコアを計算（視野の中心とのズレ）
/// </summary>
private float GetCentralityScore(Vector3 targetPos)
{
    Vector3 dirToTarget = (targetPos - userCamera.transform.position).normalized;

    // カメラの向きにオフセットを加味した回転
    Quaternion offsetRotation = Quaternion.Euler(verticalAngleOffset, horizontalAngleOffset, 0f);
    Quaternion adjustedRotation = userCamera.transform.rotation * offsetRotation;

    // ワールド空間からオフセット考慮したカメラ空間へ変換
    Vector3 localDir = Quaternion.Inverse(adjustedRotation) * dirToTarget;

    float hAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
    float vAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

    return Mathf.Sqrt(hAngle * hAngle + vAngle * vAngle); // 中心からのズレ（ピクセル距離のような指標）
}




    public bool IsInFov(SADevice device, out float centralityScore)
    {
        centralityScore = float.MaxValue;

        if (device == null) return false;

        var obj = device.gameObject;

        Collider col = obj.GetComponent<Collider>();
        Renderer renderer = obj.GetComponent<Renderer>();
        if (col == null && renderer == null) return false;

        Vector3 position = obj.transform.position;
        Vector3 dirToObj = (position - userCamera.transform.position).normalized;

        // 背面除外（カメラのforwardに対して）
        if (Vector3.Dot(userCamera.transform.forward, dirToObj) < 0f) return false;

        bool isInFov = IsWithinRoundedFov(position);
        if (isInFov)
        {
            centralityScore = GetCentralityScore(position);
        }

        return isInFov;
    }

}


    public class SpatialAwarnessProvider : Singleton<SpatialAwarnessProvider>
    {
        [SerializeField] private Camera userCamera;
        
        private FOVDeviceDetectorUtil fovDetector;






        void Awake()
        {
            fovDetector = new FOVDeviceDetectorUtil(userCamera, hFov: 65, vFov:50, hOffset: 0, vOffset: 10, p:5f);
        }

        private Vector3 GetLocalPosition(Transform target)
        {
            return userCamera.transform.InverseTransformPoint(target.position);
        }

        private bool IsInDirection(Vector3 localPos, Direction direction)
        {
            switch (direction)
            {
                case Direction.Front: return localPos.z > 0;
                case Direction.Back: return localPos.z < 0;
                case Direction.Right: return localPos.x > 0;
                case Direction.Left: return localPos.x < 0;
                case Direction.Up: return localPos.y > 0;
                case Direction.Down: return localPos.y < 0;
                default: return false;
            }
        }

        public List<SADevice> FindDevicesInDirection(Direction direction, string device_type)
        {
            return SADeviceRef.Instance.GetAllDevices()
                .Where(d => d != null && d.CompareDeviceType(device_type))
                .Where(d => IsInDirection(GetLocalPosition(d.transform), direction))
                .ToList();
        }

        public List<SADevice> FindDevicesInFov(string device_type = "", bool getInFov = true)
        {
           

            return SADeviceRef.Instance.GetAllDevices()
                .Where(device => device != null && device.CompareDeviceType(device_type))
                .Where(device =>
                {
                   float centralityScore;
                 
                bool isInFov = fovDetector.IsInFov(device, out centralityScore);
                return getInFov ? isInFov : !isInFov;
                })
                .ToList();
        }


        public float GetFovCentralityScore(SADevice device)
        {
            if (device == null) return float.MaxValue;

            float centralityScore;
            fovDetector.IsInFov(device, out centralityScore);
            return centralityScore;
        }

        public List<DeviceSpatialData> GetAllDevices(string device_type, AllRequest allRequest)
        {
            string order = allRequest.order;
            float range = allRequest.range ?? float.MaxValue;

            List<SADevice> devices = SADeviceRef.Instance.GetAllDevices()
                .Where(device => device.CompareDeviceType(device_type))
                .ToList();

            return FilterDeviceData(devices, order, range);
        }

        public List<DeviceSpatialData> GetDevicesInDirection(string device_type, DirectionRequest directionRequest)
        {
            Direction dir = GetDirection(directionRequest.direction);
            List<SADevice> devices = FindDevicesInDirection(dir, device_type);
            foreach (var device in devices)
            {
                Debug.Log($"<color=cyan>Device: {device.gameObject.name}, Position: {device.transform.position}</color>");
            }
            float range = directionRequest.range ?? float.MaxValue;
            string order = directionRequest.order;

            return FilterDeviceData(devices, order, range);
        }

        public List<DeviceSpatialData>  GetDevicesInFov(string device_type, FOVRequest fovData)
        {
            List<SADevice> devices = FindDevicesInFov(device_type, fovData.isInFov);

            foreach (var device in devices)
            {
                Debug.Log($"<color=cyan>Device: {device.gameObject.name}, Position: {device.transform.position}</color>");
            }
            float range = fovData.range ?? float.MaxValue;
            string order = fovData.order;

            return FilterDeviceData(devices, order, range);
        }

        private List<DeviceSpatialData> FilterDeviceData(List<SADevice> devices, string order, float range)
        {
            List<DeviceSpatialData> deviceData = new List<DeviceSpatialData>();
            foreach (var device in devices)
            {
                var posData = device.GetDevicePositionalRelativeToUser(userCamera.transform);

                Debug.Log($"<color=cyan>Device: {device.gameObject.name}, Position: {posData.position.x} {posData.position.y} {posData.position.z} , Distance: {posData.distance_from_user}</color>");
                if (posData != null && (range == 0.0f || posData.distance_from_user <= range)) // 0.0の場合は無制限
                    deviceData.Add(posData);
            }
            return SortDevices(deviceData, order);
        }

        private List<DeviceSpatialData> SortDevices(List<DeviceSpatialData> list, string order)
        {
            switch (order.ToLower())
            {
                case "centrality": return list.OrderBy(d => d.eye_centrality_score).ToList();
                case "right": return list.OrderByDescending(d => d.position.x).ToList();
                case "left": return list.OrderBy(d => d.position.x).ToList();
                case "high": return list.OrderByDescending(d => d.position.y).ToList();
                case "down": return list.OrderBy(d => d.position.y).ToList();
                case "proximity":
                default: return list.OrderBy(d => d.distance_from_user).ToList();
            }
        }

        public List<SAFurniture> FindFurnitureInDirection(Direction direction, string furniture_type)
        {
            return SAFurnitureRef.Instance.GetAllSAFurnitures()
                .Where(f => f != null && f.CompareFurnitureType(furniture_type))
                .Where(f => IsInDirection(GetLocalPosition(f.transform), direction))
                .ToList();
        }


   

        public SAFurniture FindFurnitureByType(string furniture_type)
        {
            return SAFurnitureRef.Instance.GetAllSAFurnitures()
                .FirstOrDefault(f => f != null && f.CompareFurnitureType(furniture_type));
        }

        public List<DeviceSpatialData> GetDeviceByFurnitureType(string furnitureType, float range = 0f)
        {
            var furniture = FindFurnitureByType(furnitureType);
            if (furniture == null) return new List<DeviceSpatialData>();
            return GetDevicesAroundFurniture(furniture.GetFurnitureData().id, "proximity", range);
        }

        public List<DeviceSpatialData> GetDevicesAroundFurniture(string furnitureID, string order = "proximity", float range = 0f)
        {
            var targetFurniture = SAFurnitureRef.Instance.GetFurnitureByID(furnitureID);
            if (targetFurniture == null) return new List<DeviceSpatialData>();

            Vector3 furnitureLocalPos = userCamera.transform.InverseTransformPoint(targetFurniture.transform.position);
            var devices = SADeviceRef.Instance.GetAllDevices();
            var result = new List<DeviceSpatialData>();

            foreach (var device in devices)
            {
                float distance = Vector3.Distance(targetFurniture.transform.position, device.transform.position);
                if (range > 0 && distance > range) continue;

                Vector3 deviceLocalPos = userCamera.transform.InverseTransformPoint(device.transform.position);
                Vector3 relativePos = deviceLocalPos - furnitureLocalPos;
                var spatialData = device.GenerateFurnitureRelativePositionData(relativePos);
                result.Add(spatialData);
            }
            return SortDevices(result, order);
        }

        public void TEST_FURNITURE()
        {
            var furniture = SAFurnitureRef.Instance.GetAllSAFurnitures();
        }
    }
}