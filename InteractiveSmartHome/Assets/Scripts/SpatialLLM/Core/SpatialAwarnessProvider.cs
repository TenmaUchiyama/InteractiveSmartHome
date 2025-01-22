using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialLLM.Device;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;
using static SpatialLLM.Type.DirectionUtil;





namespace SpatialLLM.Core
{
public class SpatialAwarnessProvider : Singleton<SpatialAwarnessProvider>
{
   
    [SerializeField] private Transform userCameraTransform;
    // [SerializeField] private Camera frustalCamera; // 使用するカメラ
    public GameObject parentObject; // 親オブジェクト

    private List<SADevice> allDevices; // Deviceコンポーネントを持つオブジェクトのリスト






   
    
    public const float verticalFOV = 86f;
    public const float horizontalFOV = 100f;






    void Start()
    {

            allDevices = new List<SADevice>(parentObject.GetComponentsInChildren<SADevice>(false));

      

    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Press M");
            GetDevicesInSight("Curtain", true);
        }
    }



    public List<SADevice> GetAllDevices()
    {
        return this.allDevices; 
    }

    public SADevice GetDeviceById(string id)
    {
        return allDevices.Find(device => device.GetDBDeviceData().device_id == id);
    }

 

  public List<SADevice> GetDeviceInDirection(Direction direction, string device_type)
{
    List<SADevice> devicesInDirection = new List<SADevice>();

    foreach (SADevice device in allDevices)
    {

        if (!device.CompareDeviceType(device_type)) continue; 
        // デバイスの位置をユーザーのローカル座標系に変換
        Vector3 localPos = userCameraTransform.InverseTransformPoint(device.transform.position);

        switch (direction)
        {
            case Direction.Front:
                // 自分の前にあるすべてのデバイス
                if (localPos.z > 0)
                {
                    devicesInDirection.Add(device);
                }
                break;

            case Direction.Back:
                // 自分の後ろにあるすべてのデバイス
                if (localPos.z < 0)
                {
                    devicesInDirection.Add(device);
                }
                break;

            case Direction.Right:
                // 自分の右側にあるすべてのデバイス
                if (localPos.x > 0)
                {
                    devicesInDirection.Add(device);
                }
                break;

            case Direction.Left:
                // 自分の左側にあるすべてのデバイス
                if (localPos.x < 0)
                {
                    devicesInDirection.Add(device);
                }
                break;

            case Direction.Up:
                // 自分の上にあるすべてのデバイス
                if (localPos.y > 0)
                {
                    devicesInDirection.Add(device);
                }
                break;

            case Direction.Down:
                // 自分の下にあるすべてのデバイス
                if (localPos.y < 0)
                {
                    devicesInDirection.Add(device);
                }
                break;
        }
    }

    return devicesInDirection;
}
   
public List<SADevice> GetDevicesInSight(string device_type = "", bool getInFov = true)
{
    List<SADevice> returnDevice = new List<SADevice>();  // 条件に合致するデバイスを格納するリスト

    // 垂直方向と水平方向の半分のFOVを計算
    float halfVerticalFOV = verticalFOV / 2f;
    float halfHorizontalFOV = horizontalFOV / 2f;

    foreach (var device in allDevices)
    {
        if (!device.CompareDeviceType(device_type)) continue;
        if (device == null) continue;

        Renderer renderer = device.GetComponent<Renderer>();
        bool isWithinFov = false;

        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Vector3[] corners = GetBoundsCorners(bounds);

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
        }
        else
        {
            // Rendererがない場合はdevice.transform.positionで判定
            Vector3 directionToDevice = device.transform.position - userCameraTransform.position;
            directionToDevice.Normalize();

            Vector3 localDirection = userCameraTransform.InverseTransformDirection(directionToDevice);

            float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;

            if (Mathf.Abs(horizontalAngle) < halfHorizontalFOV && Mathf.Abs(verticalAngle) < halfVerticalFOV)
            {
                isWithinFov = true;
            }
        }

        if (getInFov)
        {
            if (isWithinFov)
            {
                returnDevice.Add(device);
            }
        }
        else
        {
            if (!isWithinFov)
            {
                returnDevice.Add(device);
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






    
     public  List<DeviceSpatialData> AllDevice(string device_type,  string order = "proximity", string rangeStr = "")
{


            
            List<SADevice> devices = this.GetAllDevices().Where(
                device => device.CompareDeviceType(device_type)
            ).ToList();
            
    List<DeviceSpatialData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();

    devicePositionData = FilterDeviceData(devicePositionData, order, rangeStr);
    

       
    return devicePositionData;

}



   public  List<DeviceSpatialData> DirectionFunction( string device_type ="",string direction="Front", string order = "proximity", string rangeStr = "")
{

    Direction dir = DirectionUtil.GetDirection(direction);


            List<SADevice> devices = this.GetDeviceInDirection(dir, device_type);
    

    List<DeviceSpatialData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();


    

    devicePositionData = FilterDeviceData(devicePositionData, order, rangeStr);
    

       
    return devicePositionData;
    
}


public  List<DeviceSpatialData> SightFunction(string device_type, string isWithinFov, string order = "proximity", string rangeStr = "")
{
    
  
    bool withinFov = isWithinFov.ToLower().Trim() == "true";

    List<SADevice> devices = this.GetDevicesInSight(device_type, withinFov);

 
    List<DeviceSpatialData> devicePositionData = devices.Select(device => device.GetDevicePositionalData()).ToList();
    
   
    devicePositionData = FilterDeviceData(devicePositionData, order, rangeStr); 
    

    return devicePositionData;
    
}




private  List<DeviceSpatialData> FilterDeviceData (List<DeviceSpatialData> devicePositionData, string order, string rangeStr)
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
private List<DeviceSpatialData> SortDevices(List<DeviceSpatialData> devices, string order)
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

   

   


}
}