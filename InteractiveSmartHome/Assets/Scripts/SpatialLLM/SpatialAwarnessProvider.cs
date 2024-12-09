using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public static class DirectionUtil 
{
    public enum Direction
    {
        Front,
        Back,
        Up,
        Down,
        Right,
        Left
    }


    public static Dictionary<string, Direction> directionMap = new Dictionary<string, Direction>()
    {
        {"Front", Direction.Front},
        {"Back", Direction.Back},
        {"Up", Direction.Up},
        {"Down", Direction.Down},
        {"Right", Direction.Right},
        {"Left", Direction.Left}
    };

    public static Direction GetDirection(string direction)
    {
        return directionMap[direction];
    }

    public static string GetDirection(Direction direction)
    {
        return direction.ToString();
    }
}


namespace SpatialLLM.Core
{
public class SpatialAwarnessProvider : Singleton<SpatialAwarnessProvider>
{
   
    [SerializeField] private Transform userCameraTransform;
    [SerializeField] private Camera frustalCamera; // 使用するカメラ
    public GameObject parentObject; // 親オブジェクト

    private List<Device> devices; // Deviceコンポーネントを持つオブジェクトのリスト

    




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
   


   public List<Device> GetDevicesInSight(bool isVisible = true)
    {
        Vector3 directionVector = userCameraTransform.transform.forward;
        Quaternion targetRotation = Quaternion.LookRotation(directionVector); 

        frustalCamera.transform.rotation = targetRotation;
        
        List<Device> visibleDevices = GetObjectsInFrustum(isVisible);
        return visibleDevices;
    }


    // 視野内にあるオブジェクトのリストを取得するメソッド
    public List<Device> GetObjectsInFrustum(bool isVisible = true)
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
}
}