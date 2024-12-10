using System;
using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Type;
using UnityEngine;





namespace SpatialLLM.Core
{
public class SpatialAwarnessProvider : Singleton<SpatialAwarnessProvider>
{
   
    [SerializeField] private Transform userCameraTransform;
    [SerializeField] private Camera frustalCamera; // 使用するカメラ
    public GameObject parentObject; // 親オブジェクト

    private List<Device> devices; // Deviceコンポーネントを持つオブジェクトのリスト

    
public const float verticalFOV = 86f;
    public const float horizontalFOV = 100f;



public DirectionUtil.Direction currentDirection = DirectionUtil.Direction.Front;



    void Start()
    {
     
        devices = new List<Device>(parentObject.GetComponentsInChildren<Device>());
    }

 

 public List<Device> GetDeviceInDirection(DirectionUtil.Direction direction)
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

        List<Device> visibleDevices = GetObjectsInFrustum();

     
       return visibleDevices;
    }
   
 public List<Device> GetDevicesInSight(bool getInFov = true)
    {
        List<Device> returnDevice = new List<Device>();  // 条件に合致するデバイスを格納するリスト

     

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

                Debug.Log("<color=red>True</color>");
                if (isWithinFov)
                {
                    returnDevice.Add(device);
                    device.ChangeColor();
                }
                else
                {
                    device.ResetColor(); // 視野外の場合の処理（必要に応じて）
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
    public List<Device> GetObjectsInFrustum()
    {
        List<Device> visibleDevices = new List<Device>();  // 視野内のオブジェクトを格納するリスト

        foreach (var device in devices.ToArray())
        {
            Vector3 viewportPosition = frustalCamera.WorldToViewportPoint(device.transform.position);

            // ビューポート座標が0〜1の範囲内に収まっている場合、視野内
            if (viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
                viewportPosition.y >= 0 && viewportPosition.y <= 1 &&
                viewportPosition.z > 0)  // zが正ならカメラの前にある
            {
                visibleDevices.Add(device);  // 視野内のオブジェクトをリストに追加
                device.ChangeColor(); 
            }else{
                device.ResetColor();
            }
        }

        return visibleDevices;  // 視野内のオブジェクトを返す
    }


    private void  SetFrustalCameraFOV()
{
    if (frustalCamera == null)
    {
        Debug.LogError("FrustalCameraが設定されていません。");
        return;
    }


            
    Quaternion targetRotation = Quaternion.LookRotation(userCameraTransform.transform.forward); 
    frustalCamera.transform.rotation = targetRotation;

    // 実際のFOV値
    float verticalFOV = 96f;
    float horizontalFOV = 110f;

    // カメラの垂直FOVを設定
    frustalCamera.fieldOfView = verticalFOV;

    // アスペクト比を計算して設定
    float aspectRatio = Mathf.Sin(horizontalFOV * 0.5f * Mathf.Deg2Rad) / Mathf.Sin(verticalFOV * 0.5f * Mathf.Deg2Rad);
    frustalCamera.aspect = aspectRatio;

    // クリッピングプレーンの設定（必要に応じて調整）
    frustalCamera.nearClipPlane = 0.1f;
    frustalCamera.farClipPlane = 1000f;
}
}
}