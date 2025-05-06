using UnityEngine;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Device;

public class FOVVisualizer : MonoBehaviour
{
    public Transform userCameraTransform; // ユーザーのカメラのTransform
    public List<SADevice> devices; // デバイスのリスト

    public GameObject parentObject;
    // FOVの設定
    public float verticalFOV = 96f;
    public float horizontalFOV = 110f;

    // 視野の可視化距離
    public float fovVisualizationDistance = 100f;

    private void Start()
    {
        if (parentObject != null)
        {
            devices = new List<SADevice>(parentObject.GetComponentsInChildren<SADevice>());
        }
        else
        {
            Debug.LogError("parentObject is not set!");
        }
    }

    void OnDrawGizmos()
    {
        if (userCameraTransform == null)
        {
            Debug.LogError("userCameraTransform is not set!");
            return;
        }

        Vector3 origin = userCameraTransform.position;
        Vector3 forward = userCameraTransform.forward;
        Vector3 up = userCameraTransform.up;
        Vector3 right = userCameraTransform.right;

        // FOVの角度をラジアンに変換
        float halfVerticalFOVRad = Mathf.Deg2Rad * (verticalFOV / 2f);
        float halfHorizontalFOVRad = Mathf.Deg2Rad * (horizontalFOV / 2f);

        // 視野のコーナー方向をローカル空間で計算
        Vector3 topLeft = (forward + (up * Mathf.Tan(halfVerticalFOVRad)) - (right * Mathf.Tan(halfHorizontalFOVRad))).normalized;
        Vector3 topRight = (forward + (up * Mathf.Tan(halfVerticalFOVRad)) + (right * Mathf.Tan(halfHorizontalFOVRad))).normalized;
        Vector3 bottomLeft = (forward - (up * Mathf.Tan(halfVerticalFOVRad)) - (right * Mathf.Tan(halfHorizontalFOVRad))).normalized;
        Vector3 bottomRight = (forward - (up * Mathf.Tan(halfVerticalFOVRad)) + (right * Mathf.Tan(halfHorizontalFOVRad))).normalized;

        // 視野のラインを描画
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + topLeft * fovVisualizationDistance);
        Gizmos.DrawLine(origin, origin + topRight * fovVisualizationDistance);
        Gizmos.DrawLine(origin, origin + bottomLeft * fovVisualizationDistance);
        Gizmos.DrawLine(origin, origin + bottomRight * fovVisualizationDistance);

        // コーナー同士を結ぶ
        Gizmos.DrawLine(origin + topLeft * fovVisualizationDistance, origin + topRight * fovVisualizationDistance);
        Gizmos.DrawLine(origin + topRight * fovVisualizationDistance, origin + bottomRight * fovVisualizationDistance);
        Gizmos.DrawLine(origin + bottomRight * fovVisualizationDistance, origin + bottomLeft * fovVisualizationDistance);
        Gizmos.DrawLine(origin + bottomLeft * fovVisualizationDistance, origin + topLeft * fovVisualizationDistance);

        // 各デバイスのバウンディングボックスを描画
        if (devices != null && devices.Count > 0)
        {
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

                // バウンディングボックスがFOV内にあるかを判定
                bool isInFOV = IsBoundsInFOV(bounds, origin, forward, up, right, verticalFOV, horizontalFOV);

                // 色を設定
                Gizmos.color = isInFOV ? Color.red : Color.white;

                // バウンディングボックスを描画
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }

    /// <summary>
    /// バウンディングボックスがFOV内にあるかどうかを判定します。
    /// </summary>
    /// <param name="bounds">判定対象のバウンディングボックス</param>
    /// <param name="origin">FOVの起点（カメラの位置）</param>
    /// <param name="forward">カメラの前方向</param>
    /// <param name="up">カメラの上方向</param>
    /// <param name="right">カメラの右方向</param>
    /// <param name="verticalFOV">垂直FOV角度</param>
    /// <param name="horizontalFOV">水平FOV角度</param>
    /// <returns>FOV内にある場合はtrue、そうでない場合はfalse</returns>
    bool IsBoundsInFOV(Bounds bounds, Vector3 origin, Vector3 forward, Vector3 up, Vector3 right, float verticalFOV, float horizontalFOV)
    {
        // バウンディングボックスの各コーナーを取得
        Vector3[] corners = GetBoundsCorners(bounds);

        foreach (var corner in corners)
        {
            Vector3 direction = corner - origin;
            float distance = direction.magnitude;
            direction.Normalize();

            // 水平方向の角度を計算
            float horizontalAngle = Vector3.Angle(Vector3.ProjectOnPlane(direction, up), Vector3.ProjectOnPlane(forward, up));

            // 垂直方向の角度を計算
            float verticalAngle = Vector3.Angle(Vector3.ProjectOnPlane(direction, right), Vector3.ProjectOnPlane(forward, right));

            if (horizontalAngle <= horizontalFOV / 2f && verticalAngle <= verticalFOV / 2f)
            {
                return true; // 少なくとも一つのコーナーがFOV内にある
            }
        }

        return false; // 全てのコーナーがFOV外
    }

    /// <summary>
    /// バウンディングボックスの全てのコーナーを取得します。
    /// </summary>
    /// <param name="bounds">バウンディングボックス</param>
    /// <returns>8つのコーナーの配列</returns>
    Vector3[] GetBoundsCorners(Bounds bounds)
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
}
