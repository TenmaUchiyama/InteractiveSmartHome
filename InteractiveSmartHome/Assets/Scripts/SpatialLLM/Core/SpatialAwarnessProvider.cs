using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;
using static SpatialLLM.Type.DirectionUtil;





namespace SpatialLLM.Core
{
public class SpatialAwarnessProvider : Singleton<SpatialAwarnessProvider>
{
   
    [SerializeField] private Transform userCameraTransform;
    [SerializeField] private Camera frustalCamera; // 使用するカメラ
    [SerializeField] private GameObject aFurniture;
    public GameObject parentObject; // 親オブジェクト

    private List<TestDevice> devices; // Deviceコンポーネントを持つオブジェクトのリスト

    
public const float verticalFOV = 86f;
    public const float horizontalFOV = 100f;



public DirectionUtil.Direction currentDirection = DirectionUtil.Direction.Front;



    void Start()
    {
     
        devices = new List<TestDevice>(parentObject.GetComponentsInChildren<TestDevice>());


        List<TestDevice> devicePositionalDatas =  GetDevicesAroundFurniture(currentDirection);





         

        foreach (var device in devicePositionalDatas)
        {
           DevicePositionalData data = device.GetDevicePositionalData(); 

           Debug.Log($"<color=red>{data.name}</color>");
        }
    }



    public List<TestDevice> GetAllDevices()
    {
        return this.devices; 
    }

 

 public List<TestDevice> GetDeviceInDirection(DirectionUtil.Direction direction)
    {
        Vector3 directionVector = Vector3.zero;

        // Enumに応じた方向を設定
        switch (direction)
        {
            case DirectionUtil.Direction.Front:
                directionVector = userCameraTransform.transform.forward;
                break;
            case DirectionUtil.Direction.Back:
                directionVector = -userCameraTransform.transform.forward;
                break;
            case DirectionUtil.Direction.Up:
                directionVector = userCameraTransform.transform.up;
                break;
            case DirectionUtil.Direction.Down:
                directionVector = -userCameraTransform.transform.up;
                break;
            case DirectionUtil.Direction.Right:
                directionVector = userCameraTransform.transform.right;
                break;
            case DirectionUtil.Direction.Left:
                directionVector = -userCameraTransform.transform.right;
                break;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionVector); 

        frustalCamera.transform.rotation = targetRotation;

        List<TestDevice> visibleDevices = GetObjectsInFrustum();

     
       return visibleDevices;
    }
   
 public List<TestDevice> GetDevicesInSight(bool getInFov = true)
    {
        List<TestDevice> returnDevice = new List<TestDevice>();  // 条件に合致するデバイスを格納するリスト

     

        // 垂直方向と水平方向の半分のFOVを計算
        float halfVerticalFOV = verticalFOV / 2f;
        float halfHorizontalFOV = horizontalFOV / 2f;

        foreach (var device in devices)
        {
            if (device == null) continue;

            Renderer renderer = device.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"Device {device.name} does not have a Renderer component.");
                continue;
            }

            Bounds bounds = renderer.bounds;
            Vector3[] corners = GetBoundsCorners(bounds);

            bool isWithinFov = false;


            
            int outOfFovCount = 0;  // 範囲外の角のカウント
            int halfCornersCount = corners.Length / 2;  // 半分の数

            foreach (var corner in corners)
            {
                Vector3 directionToCorner = corner - userCameraTransform.position;
                directionToCorner.Normalize();

                Vector3 localDirection = userCameraTransform.InverseTransformDirection(directionToCorner);

                float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
                float verticalAngle = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;

                // FOV内にない場合、カウント
                if (Mathf.Abs(horizontalAngle) >= halfHorizontalFOV || Mathf.Abs(verticalAngle) >= halfVerticalFOV)
                {
                    outOfFovCount++;
                }

                // 範囲外の角が半分以上だったら終了
                if (outOfFovCount >= halfCornersCount)
                {
                    isWithinFov = false;
                    break;
                }
            }

            // 範囲内の角が半分以上ならtrue、それ以外はfalse
            if (outOfFovCount < halfCornersCount)
            {
                isWithinFov = true;
            }

            if (getInFov)
            {

                if (isWithinFov)
                {
                    returnDevice.Add(device);
               
                }
                else
                {
                    
                }
            }
            else
            {
            Debug.Log("<color=green>False</color>");

                if (!isWithinFov)
                {
                    returnDevice.Add(device);
                    device.ChangeColor();
                }
                else
                {
                    device.ResetColor(); // 視野内の場合の処理（必要に応じて）
                }
            }
        }

        return returnDevice;  // 条件に合致するデバイスを返す
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


    // 視野内にあるオブジェクトのリストを取得するメソッド
    public List<TestDevice> GetObjectsInFrustum()
    {
        List<TestDevice> visibleDevices = new List<TestDevice>();  // 視野内のオブジェクトを格納するリスト

        foreach (var device in devices.ToArray())
        {
            Vector3 viewportPosition = frustalCamera.WorldToViewportPoint(device.transform.position);

            // ビューポート座標が0〜1の範囲内に収まっている場合、視野内
            if (viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                viewportPosition.y >= 0 && viewportPosition.y <= 1 &&
                viewportPosition.z > 0)  // zが正ならカメラの前にある
            {
                visibleDevices.Add(device);  // 視野内のオブジェクトをリストに追加
              
            }else{
                
            }
        }

        return visibleDevices;  // 視野内のオブジェクトを返す
    }






    
     public  List<DevicePositionalData> AllDevice(string direction, string order = "proximity", string rangeStr = "")
{

            

    List<TestDevice> devices = SpatialAwarnessProvider.Instance.GetAllDevices();
    List<DevicePositionalData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();

    devicePositionData = FilterDeviceData(devicePositionData, order, rangeStr);
    

       
    return devicePositionData;

}



   public  List<DevicePositionalData> DirectionFunction(string direction, string order = "proximity", string rangeStr = "")
{

    Direction dir = DirectionUtil.GetDirection(direction);
    

    List<TestDevice> devices = SpatialAwarnessProvider.Instance.GetDeviceInDirection(dir);
    

    List<DevicePositionalData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();


    

    devicePositionData = FilterDeviceData(devicePositionData, order, rangeStr);
    

       
    return devicePositionData;
    
}


public  List<DevicePositionalData> SightFunction(string isWithinFov, string order = "proximity", string rangeStr = "")
{
  
    bool withinFov = isWithinFov.ToLower().Trim() == "true";
    
 
    List<TestDevice> devices = SpatialAwarnessProvider.Instance.GetDevicesInSight(withinFov);
    

    List<DevicePositionalData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();
    

    devicePositionData = FilterDeviceData(devicePositionData, order, rangeStr); 
    

    return devicePositionData;
    
}




private  List<DevicePositionalData> FilterDeviceData (List<DevicePositionalData> devicePositionData, string order, string rangeStr)
{
     
      if (float.TryParse(rangeStr, out float range))
    {
        // Filter based on range and convert the result to a List
        devicePositionData = devicePositionData
            .Where(device => device.distance_from_user < range)
            .ToList();
    }

    devicePositionData = SortDevices(devicePositionData, order);

    return devicePositionData;
}

    private float ComputeAngle(Vector3 devicePosition)
    {
        // デバイスの相対位置を計算（ユーザーから見たデバイスの位置）
        Vector3 directionToDevice = devicePosition - userCameraTransform.position;
        directionToDevice.y = 0; // 水平面での角度を計算するためにY軸を無視

        // ユーザーの前方方向を取得
        Vector3 forward = userCameraTransform.forward;
        forward.y = 0; // 水平面での方向

        // ユーザーの前方からデバイスへの方向の角度を計算
        float angle = Vector3.SignedAngle(forward, directionToDevice, Vector3.up);

        // 角度を -180〜180 度の範囲に保持
        return angle;
    }
private List<DevicePositionalData> SortDevices(List<DevicePositionalData> devices, string order)
{
    // 各デバイスに角度を計算して追加
    foreach (var device in devices)
    {
        Vector3 devicePos = new Vector3(device.position.x, device.position.y, device.position.z);
        device.angle = ComputeAngle(devicePos);
    }

    // ソートロジック
    switch (order.ToLower())
    {
        case "right":
            // 右から左へ（角度が小さい順）
            devices = devices.OrderBy(d => d.angle).ToList();
            break;
        case "left":
            // 左から右へ（角度が大きい順）
            devices = devices.OrderByDescending(d => d.angle).ToList();
            break;
        case "down":
            // 高さの低い順
            devices = devices.OrderBy(d => d.position.y).ToList();
            break;
        case "high":
            // 高さの高い順
            devices = devices.OrderByDescending(d => d.position.y).ToList();
            break;
        case "angle":
            // 角度順
            devices = devices.OrderBy(d => d.angle).ToList();
            break;
        case "proximity":
        default:
            // 距離の近い順（デフォルト）
            devices = devices.OrderBy(d => d.distance_from_user).ToList();
            break;
    }

    return devices;
}

   



public List<TestDevice> GetDevicesAroundFurniture(Direction userDirection)
{
    List<TestDevice> devicesInDirection = new List<TestDevice>();

    if (aFurniture == null)
    {
        Debug.LogWarning("aFurniture is not assigned.");
        return devicesInDirection;
    }




    foreach (var device in devices)
    {
        if (device == null) continue;

        Vector3 devicePosition = device.transform.position;
        Vector3 furniturePosition = aFurniture.transform.position;

        // aFurniture からデバイスへのベクトル
        Vector3 directionToDevice = devicePosition - furniturePosition;

        // ユーザーのカメラ空間に変換
        Vector3 localDirection = userCameraTransform.InverseTransformDirection(directionToDevice);

        // 水平および垂直の角度を計算
        float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float verticalAngle = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;

        // 指定された方向に基づいてフィルタリング
        switch (userDirection)
        {
            case Direction.Front:
                if (Mathf.Abs(horizontalAngle) < 45 && localDirection.z > 0)
                    devicesInDirection.Add(device);
                break;
            case Direction.Back:
                if (Mathf.Abs(horizontalAngle) < 45 && localDirection.z < 0)
                    devicesInDirection.Add(device);
                break;
            case Direction.Left:
                if (Mathf.Abs(horizontalAngle - (-90)) < 45)
                    devicesInDirection.Add(device);
                break;
            case Direction.Right:
                if (Mathf.Abs(horizontalAngle - 90) < 45)
                    devicesInDirection.Add(device);
                break;
            case Direction.Up:
                if (verticalAngle > 45)
                    devicesInDirection.Add(device);
                break;
            case Direction.Down:
                if (verticalAngle < -45)
                    devicesInDirection.Add(device);
                break;
        }
    }

    return devicesInDirection;
}


}
}