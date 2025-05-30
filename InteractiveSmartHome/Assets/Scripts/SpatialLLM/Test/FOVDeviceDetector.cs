using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SpatialLLM.Device;





public class FOVDeviceDetectorUtil
{
    private Camera userCamera;

    private float horizontalFOV = 70f;
    private float verticalFOV = 70f;

    // 視線方向のオフセット（度単位）
    private float horizontalAngleOffset = 0f; // 例: 右に5度傾けたい → +5
    private float verticalAngleOffset = 0f;   // 例: 下に10度傾けたい → -10
    private float pitchOffsetAngle = 0f; // カメラの下向きオフセット（度単位）
    private float pValue = 5f; // 丸みの強さ（2: 楕円, ∞: 長方形, 4〜6: 角丸推奨）
    public FOVDeviceDetectorUtil(Camera cam, float hFov = 70f, float vFov = 70f, float hOffset = 0f, float vOffset = 0f, float pitchOffset = 0f, float p = 5f)
    {
        userCamera = cam;
        horizontalFOV = hFov;
        verticalFOV = vFov;
        horizontalAngleOffset = hOffset;
        verticalAngleOffset = vOffset;
        pitchOffsetAngle = pitchOffset;
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
private bool IsWithinRoundedFov(Vector3 targetPos, float p = 5f)
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

    float normalized = Mathf.Pow(Mathf.Abs(hAngle / a), p) + Mathf.Pow(Mathf.Abs(vAngle / b), p);

    return normalized <= 1f;
}
    
 /// <summary>
/// 中心からのスコアを計算（視野の中心とのズレ）
/// </summary>
private float GetCentralityScore(Vector3 targetPos)
{
    Vector3 dirToTarget = (targetPos - userCamera.transform.position).normalized;

    // カメラ空間 + オフセット考慮
    Vector3 localDir = Quaternion.Inverse(userCamera.transform.rotation) * dirToTarget;
    Quaternion offsetRotation = Quaternion.Euler(verticalAngleOffset, horizontalAngleOffset, 0f);
    localDir = Quaternion.Inverse(offsetRotation) * localDir;

    float hAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
    float vAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

    return Mathf.Sqrt(hAngle * hAngle + vAngle * vAngle);
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

[ExecuteInEditMode]
public class FOVDeviceDetector : MonoBehaviour
{
    [Header("検出用カメラ (未設定ならこのオブジェクトの Camera を使います)")]
    public Camera userCamera;

    [Header("FOV 設定")]
    [Tooltip("垂直方向の画角 (degrees)")]
    public float verticalFOV = 70f;
    [Tooltip("水平方向の画角 (degrees)")]
    public float horizontalFOV = 70f;

    [Header("Gizmo 表示設定")]
    [Tooltip("視錐台を描画する距離")]
    public float viewDistance = 5f;
    [Tooltip("Sceneビューに視錐台を描画するか")]
    public bool drawGizmo = true;

    [Header("カメラ下向きオフセット")]
    [Tooltip("正の値で下を向く (degrees)")]
    public float verticalOffsetAngle = 0f;
    [Tooltip("水平方向のオフセット (degrees)")]
    public float horizontalAngleOffset = 0f;

    [Header("FOV 丸みの強さ")]
    [Tooltip("2: 楕円, ∞: 長方形, 4〜6: 角丸推奨")]
    public float roundness = 5f;



    private FOVDeviceDetectorUtil fovUtil;

    void Start()
    {
        fovUtil = new FOVDeviceDetectorUtil(userCamera, horizontalFOV, verticalFOV, horizontalAngleOffset, verticalOffsetAngle, 0f, roundness);
    }
    void Update()
    {

      
        

        foreach (SADevice device in SADeviceRef.Instance.GetAllDevices())
        {
            if (device == null) continue;

            float centralityScore;
            bool isInFov =fovUtil.IsInFov(device, out centralityScore);
            if (isInFov)
            {
                device.TEMP_DrawHover();
            }
            else
            {

                device.TEMP_DrawUnHover();
            }

        }


    }
//     private void OnDrawGizmos()
// {
//     if (!drawGizmo || userCamera == null) return;

//     // パラメータ設定
//     float radius = viewDistance;
//     float hFovRad = horizontalFOV * 0.5f * Mathf.Deg2Rad;
//     float vFovRad = verticalFOV * 0.5f * Mathf.Deg2Rad;
//     float width = Mathf.Tan(hFovRad) * radius * 2f;
//     float height = Mathf.Tan(vFovRad) * radius * 2f;
//     float p = 5f;  // 丸みの強さ（2: 楕円, ∞: 長方形, 4〜6: 角丸推奨）

//     Vector3 adjustedForward = Quaternion.Euler(-ver, 0, 0) * userCamera.transform.forward;
//     Quaternion ellipseRotation = Quaternion.LookRotation(adjustedForward, userCamera.transform.up);
//     Vector3 center = userCamera.transform.position + adjustedForward * radius;

//     int segments = 128;
//     Vector3 prevPoint = Vector3.zero;

//     Gizmos.color = Color.green;

//     for (int i = 0; i <= segments; i++)
//     {
//         float t = (float)i / segments * Mathf.PI * 2f;
//         float cos = Mathf.Cos(t);
//         float sin = Mathf.Sin(t);

//         float x = Mathf.Sign(cos) * Mathf.Pow(Mathf.Abs(cos), 2f / p) * width * 0.5f;
//         float y = Mathf.Sign(sin) * Mathf.Pow(Mathf.Abs(sin), 2f / p) * height * 0.5f;

//         Vector3 local = new Vector3(x, y, 0f);
//         Vector3 world = ellipseRotation * local + center;

//         if (i > 0)
//         {
//             Gizmos.DrawLine(prevPoint, world);
//         }

//         prevPoint = world;
//     }

//     // 視線補助線
//     Gizmos.color = Color.cyan;
//     Gizmos.DrawLine(userCamera.transform.position, center);
// }


 }