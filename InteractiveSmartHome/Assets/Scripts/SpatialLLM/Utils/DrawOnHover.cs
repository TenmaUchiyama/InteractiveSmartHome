using System.Collections.Generic;
using Oculus.Interaction;
using SpatialLLM.Device;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class DrawOnHover : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private InteractableUnityEventWrapper InteractableUnityEventWrapper;

    [Header("Line Settings")]
    public Color hoverColor    = Color.white;  // Hover時の色
    public Color selectedColor = Color.green;  // Select時の色
    public float lineWidth     = 0.01f;        // 線の太さ

    [Header("VR Camera (Center Eye)")]
    [SerializeField] private Camera vrCamera;   // Inspectorで割り当て or CenterEyeAnchorから自動取得

    private LineRenderer lineRenderer;
    private SADevice     saDevice;
    private Renderer     targetRenderer;

    void Awake()
    {
        // VRカメラ探し
        if (vrCamera == null)
        {
            var go = GameObject.Find("CenterEyeAnchor");
            if (go != null) vrCamera = go.GetComponent<Camera>();
        }
        if (vrCamera == null)
            Debug.LogError("DrawOnHover: VR Camera が見つかりません。CenterEyeAnchor をアサインしてください。");

        lineRenderer    = GetComponent<LineRenderer>();
        saDevice        = GetComponent<SADevice>();
        targetRenderer  = GetComponent<Renderer>();
    }

  

    void Start()
    {
        InitLineRenderer();
        DrawBoundingBox();  // 初期形状を一度だけセット
        InteractableUnityEventWrapper.WhenHover.AddListener(WhenHovered);
        InteractableUnityEventWrapper.WhenUnhover.AddListener(WhenUnHovered);
        InteractableUnityEventWrapper.WhenSelect.AddListener(WhenSelected);
    }

    void OnDestroy()
    {
        InteractableUnityEventWrapper.WhenHover.RemoveListener(WhenHovered);
        InteractableUnityEventWrapper.WhenUnhover.RemoveListener(WhenUnHovered);
        InteractableUnityEventWrapper.WhenSelect.RemoveListener(WhenSelected);
    }

    /// <summary>
    /// FOV内かどうかを判定
    /// </summary>
    private bool IsWithinFov()
    {
        if (vrCamera == null || targetRenderer == null) return false;
        var planes = GeometryUtility.CalculateFrustumPlanes(vrCamera);
        return GeometryUtility.TestPlanesAABB(planes, targetRenderer.bounds);
    }

    private void WhenHovered()
    {
        if (!IsWithinFov()) return;

        DrawHover();
    }

    private void WhenUnHovered()
    {
        // 元の挙動そのまま
        DrawUnhover();
    }

    private void WhenSelected()
    {
        if (!IsWithinFov()) return;

        saDevice.SetIsSelected(!saDevice.IsDeviceSelected());
        DrawSelect();
    }



    /// <summary>
    /// ラインの初期設定
    /// </summary>
    private void InitLineRenderer()
    {
        lineRenderer.material      = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useWorldSpace = false;  // VRではWorldSpace一択
        lineRenderer.loop          = false;
        lineRenderer.startWidth    = lineWidth;
        lineRenderer.endWidth      = lineWidth;
        lineRenderer.startColor    = hoverColor;
        lineRenderer.endColor      = hoverColor;
        lineRenderer.enabled       = false;
    }

    /// <summary>
    /// 選択トグルに応じて色・表示を切り替え
    /// </summary>
    public void DrawSelect()
    {
        if (saDevice.IsDeviceSelected())
        {
            lineRenderer.startColor = selectedColor;
            lineRenderer.endColor   = selectedColor;
            lineRenderer.enabled    = true;
        }
        else
        {
            lineRenderer.startColor = hoverColor;
            lineRenderer.endColor   = hoverColor;
            lineRenderer.enabled    = false;
        }
    }

    /// <summary>
    /// Hover時は常に白ライン表示
    /// </summary>
    public void DrawHover()
    {
        lineRenderer.startColor = hoverColor;
        lineRenderer.endColor   = hoverColor;
        lineRenderer.enabled    = true;
    }

    /// <summary>
    /// 元の DrawUnhover 名義はそのまま残し
    /// </summary>
    public void DrawUnhover()
    {
        if (!saDevice.IsDeviceSelected())
        {
            lineRenderer.enabled = false;
        }
        else
        {
            lineRenderer.startColor = selectedColor;
            lineRenderer.endColor   = selectedColor;
            lineRenderer.enabled    = true;
        }
    }

    /// <summary>
    /// ClearDrawing もオリジナル通り
    /// </summary>
    public void ClearDrawing()
    {
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// 特定色でラインを強調
    /// </summary>
    public void VisualizeTargetDevice(Color targetColor)
    {
        lineRenderer.startColor = targetColor;
        lineRenderer.endColor   = targetColor;
        lineRenderer.enabled    = true;
    }

    /// <summary>
    /// 可視状態チェック
    /// </summary>
    public bool isVisible()
    {
        return lineRenderer.enabled;
    }

    /// <summary>
    /// MeshRenderer.bounds を使ってワイヤーフレーム描画頂点を生成
    /// </summary>
public void DrawBoundingBox()
{
    BoxCollider box = GetComponent<BoxCollider>();
    if (box == null)
    {
        Debug.LogWarning("BoxCollider が見つかりません。");
        return;
    }

    Vector3 center = box.center;
    Vector3 size   = box.size;
    Vector3 e = size * 0.5f;

    Vector3[] corners = new Vector3[]
    {
        center + new Vector3(-e.x, -e.y, -e.z),
        center + new Vector3( e.x, -e.y, -e.z),
        center + new Vector3( e.x,  e.y, -e.z),
        center + new Vector3(-e.x,  e.y, -e.z),
        center + new Vector3(-e.x, -e.y,  e.z),
        center + new Vector3( e.x, -e.y,  e.z),
        center + new Vector3( e.x,  e.y,  e.z),
        center + new Vector3(-e.x,  e.y,  e.z),
    };

    int[] edges = new int[]
    {
        0,1, 1,2, 2,3, 3,0,
        4,5, 5,6, 6,7, 7,4,
        0,4, 1,5, 2,6, 3,7
    };

    List<Vector3> points = new();
    for (int i = 0; i < edges.Length; i += 2)
    {
        points.Add(corners[edges[i]]);
        points.Add(corners[edges[i + 1]]);
        points.Add(corners[edges[i + 1]]);
    }

    lineRenderer.useWorldSpace = false;
    lineRenderer.positionCount = points.Count;
    lineRenderer.SetPositions(points.ToArray());
    lineRenderer.enabled = true;
}

}
