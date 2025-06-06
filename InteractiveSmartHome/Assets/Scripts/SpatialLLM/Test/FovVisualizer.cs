using UnityEngine;

[ExecuteInEditMode]
public class FovVisualizer : MonoBehaviour
{
    public Camera userCamera;
    public float horizontalFOV = 70f;
    public float verticalFOV = 70f;
    public float viewDistance = 5f;

    public float horizontalAngleOffset = 0f; // yaw
    public float verticalAngleOffset = 0f;   // pitch

    private void OnDrawGizmos()
    {
        if (userCamera == null) return;

        Vector3 origin = userCamera.transform.position;

        // オフセットを加味したforward方向
        Quaternion offsetRotation = Quaternion.Euler(verticalAngleOffset, horizontalAngleOffset, 0f);
        Vector3 adjustedForward = userCamera.transform.rotation * offsetRotation * Vector3.forward;

        // 中心線
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + adjustedForward * viewDistance);

        // 視野端の計算
        float hFOV_half = horizontalFOV * 0.5f;
        float vFOV_half = verticalFOV * 0.5f;

        Vector3 right = Quaternion.Euler(0, hFOV_half, 0) * adjustedForward;
        Vector3 left = Quaternion.Euler(0, -hFOV_half, 0) * adjustedForward;
        Vector3 up = Quaternion.Euler(-vFOV_half, 0, 0) * adjustedForward;
        Vector3 down = Quaternion.Euler(vFOV_half, 0, 0) * adjustedForward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + right.normalized * viewDistance);
        Gizmos.DrawLine(origin, origin + left.normalized * viewDistance);
        Gizmos.DrawLine(origin, origin + up.normalized * viewDistance);
        Gizmos.DrawLine(origin, origin + down.normalized * viewDistance);

        // 視野の枠を仮で結ぶ（おおまかな可視化）
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawLine(origin + right.normalized * viewDistance, origin + up.normalized * viewDistance);
        Gizmos.DrawLine(origin + up.normalized * viewDistance, origin + left.normalized * viewDistance);
        Gizmos.DrawLine(origin + left.normalized * viewDistance, origin + down.normalized * viewDistance);
        Gizmos.DrawLine(origin + down.normalized * viewDistance, origin + right.normalized * viewDistance);
    }
}
