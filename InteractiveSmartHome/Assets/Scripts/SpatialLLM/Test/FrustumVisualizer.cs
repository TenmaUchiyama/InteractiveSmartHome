using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(LineRenderer))]
public class FrustumVisualizer : MonoBehaviour
{
    [Header("FOV Settings")]
    [Range(1f, 179f)]
    [SerializeField] private float verticalFOV = 60f;
    [Range(1f, 179f)]
    [SerializeField] private float horizontalFOV = 90f;

    [Header("Clipping Planes")]
    [SerializeField] private float nearClipPlane = 0.1f;
    [SerializeField] private float farClipPlane = 1000f;

    [Header("Visualization Settings")]
    [SerializeField] private Color frustumColor = Color.yellow;

    private Camera frustumCamera;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        frustumCamera = GetComponent<Camera>();
        if (frustumCamera == null)
        {
            Debug.LogError("FrustumCameraが設定されていません。");
            enabled = false;
            return;
        }

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 16; // 8 for near plane + 8 for far plane
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = frustumColor };
    }

    private void Update()
    {
        SetFrustumCameraFOV();
        DrawFrustum();
    }

    private void SetFrustumCameraFOV()
    {
        frustumCamera.fieldOfView = verticalFOV;
        float aspectRatio = Mathf.Sin(horizontalFOV * 0.5f * Mathf.Deg2Rad) / Mathf.Sin(verticalFOV * 0.5f * Mathf.Deg2Rad);
        frustumCamera.aspect = aspectRatio;
        frustumCamera.nearClipPlane = nearClipPlane;
        frustumCamera.farClipPlane = farClipPlane;
    }

    private void DrawFrustum()
    {
        Vector3[] corners = CalculateFrustumCorners();
        lineRenderer.positionCount = corners.Length;

        for (int i = 0; i < corners.Length; i++)
        {
            lineRenderer.SetPosition(i, corners[i]);
        }
    }

    private Vector3[] CalculateFrustumCorners()
    {
        Vector3[] corners = new Vector3[16];
        Vector3 camPos = frustumCamera.transform.position;
        Vector3 camForward = frustumCamera.transform.forward;
        Vector3 camRight = frustumCamera.transform.right;
        Vector3 camUp = frustumCamera.transform.up;

        float near = frustumCamera.nearClipPlane;
        float far = frustumCamera.farClipPlane;

        float vFOV = verticalFOV * Mathf.Deg2Rad;
        float hFOV = horizontalFOV * Mathf.Deg2Rad;

        float nearHeight = 2f * Mathf.Tan(vFOV / 2f) * near;
        float nearWidth = 2f * Mathf.Tan(hFOV / 2f) * near;

        float farHeight = 2f * Mathf.Tan(vFOV / 2f) * far;
        float farWidth = 2f * Mathf.Tan(hFOV / 2f) * far;

        // 近クリッププレーンの四隅
        Vector3 nearTopLeft = camPos + camForward * near + (camUp * nearHeight / 2f) - (camRight * nearWidth / 2f);
        Vector3 nearTopRight = camPos + camForward * near + (camUp * nearHeight / 2f) + (camRight * nearWidth / 2f);
        Vector3 nearBottomLeft = camPos + camForward * near - (camUp * nearHeight / 2f) - (camRight * nearWidth / 2f);
        Vector3 nearBottomRight = camPos + camForward * near - (camUp * nearHeight / 2f) + (camRight * nearWidth / 2f);

        // 遠クリッププレーンの四隅
        Vector3 farTopLeft = camPos + camForward * far + (camUp * farHeight / 2f) - (camRight * farWidth / 2f);
        Vector3 farTopRight = camPos + camForward * far + (camUp * farHeight / 2f) + (camRight * farWidth / 2f);
        Vector3 farBottomLeft = camPos + camForward * far - (camUp * farHeight / 2f) - (camRight * farWidth / 2f);
        Vector3 farBottomRight = camPos + camForward * far - (camUp * farHeight / 2f) + (camRight * farWidth / 2f);

        // 近クリッププレーンの四隅を追加
        corners[0] = nearTopLeft;
        corners[1] = nearTopRight;
        corners[2] = nearBottomRight;
        corners[3] = nearBottomLeft;
        corners[4] = nearTopLeft;

        // 遠クリッププレーンの四隅を追加
        corners[5] = farTopLeft;
        corners[6] = farTopRight;
        corners[7] = farBottomRight;
        corners[8] = farBottomLeft;
        corners[9] = farTopLeft;

        // 近クリッププレーンから遠クリッププレーンへの線
        corners[10] = nearTopLeft;
        corners[11] = farTopLeft;
        corners[12] = nearTopRight;
        corners[13] = farTopRight;
        corners[14] = nearBottomRight;
        corners[15] = farBottomRight;
        corners = corners.Take(16).ToArray(); // Ensure array length

        return corners;
    }
}