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
    // [SerializeField] private Camera frustalCamera; // 使用するカメラ

    public const float verticalFOV = 96f;
    public const float horizontalFOV = 106f;

    private Camera cam;
    private Plane[] camPlanes;




        void Start()
        {

       

    }



   
 


    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 extents = bounds.extents;

        corners[0] = bounds.center + new Vector3(extents.x, extents.y, extents.z);
        corners[1] = bounds.center + new Vector3(-extents.x, extents.y, extents.z);
        corners[2] = bounds.center + new Vector3(extents.x, -extents.y, extents.z);
        corners[3] = bounds.center + new Vector3(-extents.x, -extents.y, extents.z);
        corners[4] = bounds.center + new Vector3(extents.x, extents.y, -extents.z);
        corners[5] = bounds.center + new Vector3(-extents.x, extents.y, -extents.z);
        corners[6] = bounds.center + new Vector3(extents.x, -extents.y, -extents.z);
        corners[7] = bounds.center + new Vector3(-extents.x, -extents.y, -extents.z);

        return corners;
    }



// ユーザーのカメラ（またはTransform）を基準に対象のローカル座標を取得
private Vector3 GetLocalPosition(Transform target)
{
    return userCamera.transform.InverseTransformPoint(target.position);
}

// 指定方向（Front, Back, Right, Left, Up, Down）に対象があるかを判定
private bool IsInDirection(Vector3 localPos, Direction direction)
{
    switch (direction)
    {
        case Direction.Front:
            return localPos.z > 0;
        case Direction.Back:
            return localPos.z < 0;
        case Direction.Right:
            return localPos.x > 0;
        case Direction.Left:
            return localPos.x < 0;
        case Direction.Up:
            return localPos.y > 0;
        case Direction.Down:
            return localPos.y < 0;
        default:
            return false;
    }
}

// 対象がユーザーのFOV内にあるかを判定
// 対象がユーザーのFOV内にあるかを、詳細デバッグ付きで判定
private void InitCamera()
{
    cam = Camera.main;
    camPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
}

private bool IsWithinFov(Renderer renderer)
{
    if (cam == null || renderer == null) return false;
    return GeometryUtility.TestPlanesAABB(camPlanes, renderer.bounds);
}





    // // 視野内にあるオブジェクトのリストを取得するメソッド
    // public List<SADevice> GetObjectsInFrustum()
    // {
    //         List<SADevice> visibleDevices = new List<SADevice>();  // 視野内のオブジェクトを格納するリスト

    //     foreach (var device in allDevices.ToArray())
    //     {
    //         Vector3 viewportPosition = frustalCamera.WorldToViewportPoint(device.transform.position);

    //         // ビューポート座標が0〜1の範囲内に収まっている場合、視野内
    //         if (viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
    //             viewportPosition.y >= 0 && viewportPosition.y <= 1 &&
    //             viewportPosition.z > 0)  // zが正ならカメラの前にある
    //         {
    //             visibleDevices.Add(device);  // 視野内のオブジェクトをリストに追加
              
    //         }else{
                
    //         }
    //     }

    //     return visibleDevices;  // 視野内のオブジェクトを返す
    // }




public List<SADevice> FindDevicesInDirection(Direction direction, string device_type)
{
    List<SADevice> allDevices = SADeviceRef.Instance.GetAllDevices();
    List<SADevice> devicesInDirection = new List<SADevice>();

    foreach (SADevice device in allDevices)
    {
        if (!device.CompareDeviceType(device_type))
            continue;

        Vector3 localPos = GetLocalPosition(device.transform);
        if (IsInDirection(localPos, direction))
        {
            devicesInDirection.Add(device);
        }
    }
    return devicesInDirection;
}

public List<SADevice> FindDevicesInFov(string device_type = "", bool getInFov = true)
{


    List<SADevice> returnDevices = new List<SADevice>();
    List<SADevice> allDevices = SADeviceRef.Instance.GetAllDevices();

    foreach (var device in allDevices)
    {
        if (device == null) continue;
        if (!device.CompareDeviceType(device_type)) continue;

        // 優先：Collider → Fallback：Renderer
        Bounds? bounds = null;

        Collider col = device.GetComponent<Collider>();
        if (col != null)
        {
            bounds = col.bounds;
        }
        else
        {
            Renderer renderer = device.GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
        }

        if (!bounds.HasValue) continue;

        // カメラの後ろにあるかチェック
        Vector3 directionToDevice = (device.transform.position - userCamera.transform.position).normalized;
        float dot = Vector3.Dot(userCamera.transform.forward, directionToDevice);
        if (dot < 0f) continue;

        // FOVチェック
        bool isInFov = GeometryUtility.TestPlanesAABB(camPlanes, bounds.Value);
        
        if ((getInFov && isInFov) || (!getInFov && !isInFov))
        {
            returnDevices.Add(device);
        }
    }

    return returnDevices;
}

// public List<SAFurniture> FindFurnitureInFov(string furniture_type = "", bool getInFov = true)
// {
//     List<SAFurniture> returnFurniture = new List<SAFurniture>();
//     List<SAFurniture> allFurniture = SAFurnitureRef.Instance.GetAllSAFurnitures();

//     float halfVerticalFOV = verticalFOV / 2f;
//     float halfHorizontalFOV = horizontalFOV / 2f;

//     foreach (var furniture in allFurniture)
//     {
//         if (furniture == null)
//             continue;

//         if (!furniture.CompareFurnitureType(furniture_type))
//             continue;

//         bool isWithinFov = IsWithinFov(furniture.transform);

//         if (getInFov && isWithinFov)
//         {
//             returnFurniture.Add(furniture);
//         }
//         else if (!getInFov && !isWithinFov)
//         {
//             returnFurniture.Add(furniture);
//         }
//     }

//     return returnFurniture;
// }


        public List<SAFurniture> FindFurnitureInDirection(Direction direction, string furniture_type)
        {
            List<SAFurniture> allFurniture = SAFurnitureRef.Instance.GetAllSAFurnitures();
            List<SAFurniture> furnitureInDirection = new List<SAFurniture>();

            foreach (SAFurniture furniture in allFurniture)
            {
                if (furniture == null)
                    continue;

                if (!furniture.CompareFurnitureType(furniture_type))
                    continue;

                Vector3 localPos = GetLocalPosition(furniture.transform);
                if (IsInDirection(localPos, direction))
                {
                    furnitureInDirection.Add(furniture);
                }
            }

            return furnitureInDirection;
        }

public SAFurniture FindFurnitureByType(string furniture_type)
{
    List<SAFurniture> allFurniture = SAFurnitureRef.Instance.GetAllSAFurnitures();

    foreach (SAFurniture furniture in allFurniture)
    {
        if (furniture == null)
            continue;

        if (furniture.CompareFurnitureType(furniture_type))
        {
            return furniture;
        }
    }

    return null;
}


    
public List<DeviceSpatialData> GetAllDevices(string device_type, AllRequest allRequest)
{
    string order = allRequest.order;
    float range = allRequest.range ?? 0f;

    List<SADevice> devices = SADeviceRef.Instance.GetAllDevices()
        .Where(device => device.CompareDeviceType(device_type))
        .ToList();

    if (devices == null)
    {
        Debug.LogError("Device list is null.");
        return new List<DeviceSpatialData>();
    }

    List<DeviceSpatialData> devicePositionData = new List<DeviceSpatialData>();
    foreach (var device in devices)
    {
        if (device == null)
        {
            Debug.LogError("Device is null.");
            continue;
        }

        try
        {
            var positionalData = device.GetDevicePositionalRelativeToUser();
            if (positionalData == null)
            {
                Debug.LogError("Device positional data is null for device: " + device.name);
                continue;
            }
            devicePositionData.Add(positionalData);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error getting positional data for device: " + device.name + " - " + ex.Message);
        }
    }

    devicePositionData = FilterDeviceData(devicePositionData, order, range);

    return devicePositionData;
}

public List<DeviceSpatialData> GetDevicesInDirection(string device_type, DirectionRequest directionRequest)
{
    string direction = directionRequest.direction;
    string order = directionRequest.order;
    float range = directionRequest.range ?? 0f;

    Direction dir = DirectionUtil.GetDirection(direction);
    Debug.Log("DIRECTION: " + direction); 
    List<SADevice> devices = this.FindDevicesInDirection(dir, device_type);

    if (devices == null)
    {
        Debug.LogError("Device list is null.");
        return new List<DeviceSpatialData>();
    }

    List<DeviceSpatialData> devicePositionData = new List<DeviceSpatialData>();
    foreach (var device in devices)
    {
        if (device == null)
        {
            Debug.LogError("Device is null.");
            continue;
        }

        try
        {
            var positionalData = device.GetDevicePositionalRelativeToUser();
            if (positionalData == null)
            {
                Debug.LogError("Device positional data is null for device: " + device.name);
                continue;
            }
            devicePositionData.Add(positionalData);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error getting positional data for device: " + device.name + " - " + ex.Message);
        }
    }

    devicePositionData = FilterDeviceData(devicePositionData, order, range);

    return devicePositionData;
}

public List<DeviceSpatialData> GetDeviceInFov(string device_type, FOVRequest fovData)
{
    bool withinFov = fovData.isInFov;
    string order = fovData.order;
    float range = fovData.range ?? 0f;

    List<SADevice> devices = this.FindDevicesInFov(device_type, withinFov);
    Debug.Log("Device Position Data: " + devices.Count);

    if (devices == null)
    {
        Debug.LogError("Device list is null.");
        return new List<DeviceSpatialData>();
    }

    List<DeviceSpatialData> devicePositionData = new List<DeviceSpatialData>();
    foreach (var device in devices)
    {
        if (device == null)
        {
            Debug.LogError("Device is null.");
            continue;
        }

        try
        {

            var positionalData = device.GetDevicePositionalRelativeToUser(userCamera.transform);
            if (positionalData == null)
            {
                Debug.LogError("Device positional data is null for device: " + device.name);
                continue;
            }
            devicePositionData.Add(positionalData);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error getting positional data for device: " + device.name + " - " + ex.Message);
        }
    }

    devicePositionData = FilterDeviceData(devicePositionData, order, range);

    return devicePositionData;
}

private List<DeviceSpatialData> FilterDeviceData (List<DeviceSpatialData> devicePositionData, string order, float range)
{
    if (range <= 0)
    {
        range = float.MaxValue;
    }
    // Filter based on range and convert the result to a List
    devicePositionData = devicePositionData
        .Where(device => device.distance_from_user < range)
        .ToList();

    devicePositionData = SortDevices(devicePositionData, order);

    return devicePositionData;
}
public List<FurnitureData> FilterFurnitureData(List<FurnitureData> data, string order, float range)
{
    IEnumerable<FurnitureData> filteredData = data;

    if (range > 0f)
    {
        filteredData = filteredData.Where(d => d.distance_from_user <= range);
    }

    switch (order)
    {
        case "proximity":
            filteredData = filteredData.OrderBy(d => d.distance_from_user);
            break;
        case "height":
            filteredData = filteredData.OrderByDescending(d => d.position.y);
            break;
        case "right":
            filteredData = filteredData.OrderByDescending(d => Vector3.Dot(Camera.main.transform.right, new Vector3(d.position.x, d.position.y, d.position.z).normalized));
            break;
    }

    return filteredData.ToList();
}

    private float ComputeAngle(Vector3 devicePosition)
    {
        // デバイスの相対位置を計算（ユーザーから見たデバイスの位置）
        Vector3 directionToDevice = devicePosition - userCamera.transform.position;
        directionToDevice.y = 0; // 水平面での角度を計算するためにY軸を無視

        // ユーザーの前方方向を取得
        Vector3 forward = userCamera.transform.forward;
        forward.y = 0; // 水平面での方向

        // ユーザーの前方からデバイスへの方向の角度を計算
        float angle = Vector3.SignedAngle(forward, directionToDevice, Vector3.up);

        // 角度を -180〜180 度の範囲に保持
        return angle;
    }
private List<DeviceSpatialData> SortDevices(List<DeviceSpatialData> devices, string order)
{
    // ソートロジック
    switch (order.ToLower())
    {
        case "right":
            // 右から左へ（x値が大きい順）
            devices = devices.OrderByDescending(d => d.position.x).ToList();
            break;
        case "left":
            // 左から右へ（x値が小さい順）
            devices = devices.OrderBy(d => d.position.x).ToList();
            break;
        case "down":
            // 高さの低い順（y値が小さい順）
            devices = devices.OrderBy(d => d.position.y).ToList();
            break;
        case "high":
            // 高さの高い順（y値が大きい順）
            devices = devices.OrderByDescending(d => d.position.y).ToList();
            break;
        case "proximity":
        default:
            // 距離の近い順（デフォルト）
            devices = devices.OrderBy(d => d.distance_from_user).ToList();
            break;
    }

    return devices;
}
   
public List<FurnitureData> GetFurnitureInDirection(DirFurnitureRequest request)
{
    string furnitureTypeStr = request.furnitureType;
    string directionStr = request.direction;
    string order = request.order;
    float range = request.range ?? 0f;

    // Direction enum に変換（"left" → Direction.Left など）
    Direction directionEnum;
    try
    {
        directionEnum = (Direction)Enum.Parse(typeof(Direction), directionStr, true);
    }
    catch
    {
        Debug.LogError($"Invalid direction: {directionStr}");
        return new List<FurnitureData>();
    }

    List<SAFurniture> furnitures = this.FindFurnitureInDirection(directionEnum, furnitureTypeStr);
    Debug.Log("Furniture count in direction: " + furnitures.Count);

    if (furnitures == null || furnitures.Count == 0)
    {
        Debug.LogWarning("No furniture found in that direction.");
        return new List<FurnitureData>();
    }

    List<FurnitureData> furnitureDataList = new List<FurnitureData>();

    foreach (var furniture in furnitures)
    {
        if (furniture == null)
        {
            Debug.LogError("Furniture is null.");
            continue;
        }

        try
        {
            var data = furniture.GetFurniturePositionalRelativeToUser();
            if (data == null)
            {
                Debug.LogError("Furniture data is null for furniture: " + furniture.name);
                continue;
            }
            furnitureDataList.Add(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting furniture data for {furniture.name}: {ex.Message}");
        }
    }

    furnitureDataList = FilterFurnitureData(furnitureDataList, order, range);

    return furnitureDataList;
}


// public List<FurnitureData> GetFurnitureInFov(FOVFurnitureRequest furnitureRequest)
// {
//     string furnitureTypeStr = furnitureRequest.furnitureType;
//     bool withinFov = furnitureRequest.isInFov;
//     string order = furnitureRequest.order;
//     float range = furnitureRequest.range ?? 0f;

//     List<SAFurniture> furnitures = this.FindFurnitureInFov(furnitureTypeStr, withinFov);
//     Debug.Log("Furniture count: " + furnitures.Count);

//     if (furnitures == null || furnitures.Count == 0)
//     {
//         Debug.LogWarning("Furniture list is empty.");
//         return new List<FurnitureData>();
//     }

//     List<FurnitureData> furnitureDataList = new List<FurnitureData>();

//     foreach (var furniture in furnitures)
//     {
//         if (furniture == null)
//         {
//             Debug.LogError("Furniture is null.");
//             continue;
//         }

//         try
//         {
//             var data = furniture.GetFurniturePositionalRelativeToUser();
//             if (data == null)
//             {
//                 Debug.LogError("Furniture data is null for furniture: " + furniture.name);
//                 continue;
//             }
//             furnitureDataList.Add(data);
//         }
//         catch (Exception ex)
//         {
//             Debug.LogError($"Error getting furniture data for {furniture.name}: {ex.Message}");
//         }
//     }

//     furnitureDataList = FilterFurnitureData(furnitureDataList, order, range);

//     return furnitureDataList;
// }



public List<DeviceSpatialData> GetDeviceByFurnitureType(string saFurnitureType, float range = 0f)
{


    SAFurniture saFurniture  = this.FindFurnitureByType(saFurnitureType);
    List<DeviceSpatialData> deviceDatas = this.GetDevicesAroundFurniture(saFurniture.GetFurnitureData().id, "proximity", range);

    if (deviceDatas == null || deviceDatas.Count == 0)
    {
        return new List<DeviceSpatialData>();
    }

    return deviceDatas;
}

public  List<DeviceSpatialData> GetDevicesAroundFurniture(string furnitureID, string order="proximity", float range = 0f)
{
    List<DeviceSpatialData> deviceRelativePositions = new List<DeviceSpatialData>();

    
    // 指定したIDのFurnitureを取得
    SAFurniture targetFurniture = SAFurnitureRef.Instance.GetFurnitureByID(furnitureID);
    if (targetFurniture == null)
    {
        Debug.LogWarning("指定されたIDのFurnitureが見つかりません: " + furnitureID);
        return deviceRelativePositions;
    }

    // ユーザーのローカル座標系でのFurnitureの位置を取得
    Vector3 furnitureLocalPos = userCamera.transform.InverseTransformPoint(targetFurniture.transform.position);

    // 全SADeviceを取得し、範囲内にあるものを調べる
    List<SADevice> allDevices = SADeviceRef.Instance.GetAllDevices();
    
    foreach (SADevice device in allDevices)
    {
        // Furnitureとdevice間の距離をワールド座標上で計算
        float distance = Vector3.Distance(targetFurniture.transform.position, device.transform.position);
        if (range == 0 || distance <= range)
        {
            // ユーザーのローカル座標系でのdeviceの位置を取得
            Vector3 deviceLocalPos = userCamera.transform.InverseTransformPoint(device.transform.position);
            // Furnitureを基準とした相対位置を算出
            Vector3 relativePos = deviceLocalPos - furnitureLocalPos;

            DeviceSpatialData deviceSpatialData = device.GenerateFurnitureRelativePositionData(relativePos);
            deviceRelativePositions.Add(deviceSpatialData);
        }
    }


    deviceRelativePositions = FilterDeviceData(deviceRelativePositions, order,range);

    return deviceRelativePositions;
}






public void TEST_FURNITURE() 
{
    List<SAFurniture> furniture = SAFurnitureRef.Instance.GetAllSAFurnitures();
    

    // List<SADevice> devices = this.GetDevicesAroundFurniture(furniture[0].GetFurnitureData().id, 2f).Keys.ToList();
    // Debug.Log($"<color=red>Received Device: {devices[0].gameObject.name}</color>");
}





}
}