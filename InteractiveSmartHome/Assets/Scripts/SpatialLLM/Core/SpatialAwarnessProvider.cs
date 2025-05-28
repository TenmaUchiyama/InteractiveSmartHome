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
    public class SpatialAwarnessProvider : Singleton<SpatialAwarnessProvider>
    {
        [SerializeField] private Camera userCamera;
        public const float verticalFOV = 70f;
        public const float horizontalFOV = 70f;

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

        private Plane[] GetCustomFrustumPlanes()
        {
            GameObject tempCamObj = new GameObject("TempFrustumCam");
            Camera tempCam = tempCamObj.AddComponent<Camera>();

            tempCam.transform.position = userCamera.transform.position;
            tempCam.transform.rotation = userCamera.transform.rotation;

            tempCam.fieldOfView = verticalFOV;
            float halfHRad = horizontalFOV * Mathf.Deg2Rad * 0.5f;
            float halfVRad = verticalFOV * Mathf.Deg2Rad * 0.5f;
            tempCam.aspect = Mathf.Tan(halfHRad) / Mathf.Tan(halfVRad);

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(tempCam);
            GameObject.Destroy(tempCamObj);
            return planes;
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
            Plane[] camPlanes = GetCustomFrustumPlanes();

            return SADeviceRef.Instance.GetAllDevices()
                .Where(device => device != null && device.CompareDeviceType(device_type))
                .Where(device =>
                {
                    Collider col = device.GetComponent<Collider>();
                    Renderer renderer = device.GetComponent<Renderer>();
                    Bounds? bounds = col != null ? col.bounds : renderer?.bounds;
                    if (!bounds.HasValue) return false;

                    Vector3 directionToDevice = (device.transform.position - userCamera.transform.position).normalized;
                    if (Vector3.Dot(userCamera.transform.forward, directionToDevice) < 0f) return false;

                    bool isInFov = GeometryUtility.TestPlanesAABB(camPlanes, bounds.Value);
                    return (getInFov && isInFov) || (!getInFov && !isInFov);
                })
                .ToList();
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
            Debug.Log($"<color=cyan>Found {devices.Count} devices in direction {dir}.</color>");
            float range = directionRequest.range ?? float.MaxValue;
            string order = directionRequest.order;

            return FilterDeviceData(devices, order, range);
        }

        public List<DeviceSpatialData> GetDeviceInFov(string device_type, FOVRequest fovData)
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
                if (posData != null && (range == 0.0f || posData.distance_from_user <= range)) // 0.0の場合は無制限
                    deviceData.Add(posData);
            }
            return SortDevices(deviceData, order);
        }

        private List<DeviceSpatialData> SortDevices(List<DeviceSpatialData> list, string order)
        {
            switch (order.ToLower())
            {
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