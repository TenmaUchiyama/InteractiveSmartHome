using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Platform;
using SpatialLLM.Device;
using SpatialLLM.Network;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class DrawOnHover : MonoBehaviour
{
    [SerializeField] private InteractableUnityEventWrapper InteractableUnityEventWrapper;

    public Color hoverColor = Color.white; // Hover時の色
    public Color selectedColor = Color.green; // Select時の色
    public float lineWidth = 0.01f; // 線の太さを設定

    private LineRenderer lineRenderer;
    private SADevice saDevice;

    void  OnEnable()
    {
     LLMQueryRequest.Instance.OnReceiveResponseFromLLM.AddListener((string msg) => {
            Debug.Log($"<color=red>Received: {msg}</color>");
            ClearDrawing();

            saDevice.SetIsSelected(false);
        });
    }
    void Start()
    {
        saDevice = GetComponent<SADevice>();
        InteractableUnityEventWrapper.WhenHover.AddListener(WhenHovered);
        InteractableUnityEventWrapper.WhenUnhover.AddListener(WhenUnHovered);
        // InteractableUnityEventWrapper.WhenSelect.AddListener(WhenSelected);


        InitLineRenderer();
        DrawBoundingBox();


       
    }

    void OnDestroy()
    {
        InteractableUnityEventWrapper.WhenHover.RemoveListener(WhenHovered);
        InteractableUnityEventWrapper.WhenUnhover.RemoveListener(WhenUnHovered);
        InteractableUnityEventWrapper.WhenSelect.RemoveListener(WhenSelected);
    }

    private void WhenSelected()
    {
        Debug.Log($"<color=red>Selected: {this.saDevice.gameObject.name}</color>");
        saDevice.SetIsSelected(!saDevice.IsDeviceSelected());
        DrawSelect();
    
      
    }

    private void WhenHovered()
    {
        Debug.Log("<color=red>Hovered</color>");

        // Hover時の色を設定
  
        DrawHover();
    }

    private void WhenUnHovered()
    {
        Debug.Log("<color=red>UnHovered</color>");

        // UnHover時に選択されていない場合は非表示
        if (!saDevice.IsDeviceSelected())
        {
            lineRenderer.enabled = false;
        }else{
            lineRenderer.startColor = selectedColor;
        }
    }




    private void InitLineRenderer()
    {
          // LineRendererコンポーネントを取得または追加
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // シェーダーを変更
        lineRenderer.enabled = false;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false; // ループ設定を解除
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // 初期色を設定
        lineRenderer.startColor = hoverColor;
        lineRenderer.endColor = hoverColor;

    }

    public void DrawSelect() 
    {
        if (saDevice.IsDeviceSelected())
        {
            lineRenderer.startColor = selectedColor;
            lineRenderer.endColor = selectedColor;
        }
        else
        {
            lineRenderer.startColor = hoverColor;
            lineRenderer.endColor = hoverColor;
        }

          lineRenderer.enabled = saDevice.IsDeviceSelected();
    }

    public void DrawHover() 
    {
        lineRenderer.startColor = hoverColor;
        lineRenderer.endColor = hoverColor;
        lineRenderer.enabled = true;
    }

    public void ClearDrawing() 
    {
        lineRenderer.enabled = false;
    }

    void DrawBoundingBox(Color color = default(Color))
    {
        if(color != default(Color))
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
        
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
    }
}
