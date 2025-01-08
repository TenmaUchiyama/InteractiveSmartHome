using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(LineRenderer))]
public class DrawOnHover : MonoBehaviour
{

    [SerializeField] private InteractableUnityEventWrapper InteractableUnityEventWrapper;
        
      public Color boundingBoxColor = Color.green; // 表示色
    private LineRenderer lineRenderer;


    public Color boxColor = Color.green; // 緑色に設定
    public float lineWidth = 0.01f; // 線の太さを設定

    void Start()
    {
        
       InteractableUnityEventWrapper.WhenHover.AddListener(WhenHovered);
         InteractableUnityEventWrapper.WhenUnhover.AddListener(WhenUnHovered);
        // LineRendererコンポーネントを取得または追加
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.enabled = false;
         lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false; // ループ設定を解除
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // 線の色を設定
        lineRenderer.startColor = boxColor;
        lineRenderer.endColor = boxColor;
        DrawBoundingBox();


        
      
    }
    void OnDestroy()
    {
        InteractableUnityEventWrapper.WhenHover.RemoveListener(WhenHovered);
        InteractableUnityEventWrapper.WhenUnhover.RemoveListener(WhenUnHovered);
    }

    private void WhenHovered()
    {

        Debug.Log("<color=red>Hovered</color>");
         lineRenderer.enabled = true;
    }

    private void WhenUnHovered() 
    {
        Debug.Log("<color=red>UnHovered</color>");
        lineRenderer.enabled = false;
    }
 void DrawBoundingBox()
{
    BoxCollider boxCollider = GetComponent<BoxCollider>();
    if (boxCollider == null)
    {
        Debug.LogWarning("BoxColliderが見つかりません.");
        return;
    }

    // BoxColliderの中心とサイズを取得
    Vector3 center = boxCollider.center;
    Vector3 size = boxCollider.size;

    // ローカル空間でのコーナー座標を計算
    Vector3[] corners = new Vector3[8];
    Transform t = transform;

    corners[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f; // 左下前
    corners[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;  // 右下前
    corners[2] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;   // 右上前
    corners[3] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;  // 左上前
    corners[4] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;  // 左下後
    corners[5] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;   // 右下後
    corners[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;    // 右上後
    corners[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;   // 左上後

    // ワールド座標に変換
    for (int i = 0; i < corners.Length; i++)
    {
        corners[i] = t.TransformPoint(corners[i]);
        Debug.Log($"Corner {i}: {corners[i]}"); // デバッグ用
    }

    // 前面、背面、垂直の辺だけを描画
    Vector3[] linePoints = new Vector3[]
    {
        // 前面
        corners[0], corners[1], corners[2], corners[3], corners[0],
        // 背面
        corners[4], corners[5], corners[6], corners[7], corners[4],
        // 垂直の辺
        corners[0], corners[4], 
        corners[1], corners[5], 
        corners[2], corners[6], 
        corners[3], corners[7]
    };

    lineRenderer.positionCount = 0; // 頂点リセット
    lineRenderer.positionCount = linePoints.Length;
    lineRenderer.SetPositions(linePoints);

    Debug.Log("LineRenderer updated with points.");
}
}
