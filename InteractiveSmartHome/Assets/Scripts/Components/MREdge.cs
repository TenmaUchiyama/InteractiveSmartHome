using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NodeTypes;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;



namespace MRFlow.Component 
{
public class MREdge : MonoBehaviour
{
    
    private MREdgeData _mrEdgeData; 

    public MREdgeData mrEdgeData => _mrEdgeData;
    
    MRNode nodeIn;
    MRNode nodeOut;

    RectTransform nodeInRect; 
    RectTransform nodeOutRect;

    private LineRenderer lineRenderer;

    
    private void Start() {
      this.lineRenderer = this.GetComponent<LineRenderer>();
      
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.positionCount = 2;
    }
    public void setNodes(NodeHandler nodeIn, NodeHandler nodeOut)
    {

        this.nodeIn = nodeIn.GetNode();
        this.nodeOut = nodeOut.GetNode();

        this.nodeInRect = this.nodeIn.GetCanvasRect();
        this.nodeOutRect = this.nodeOut.GetCanvasRect();
 
    }

private void Update()
{
    if (!lineRenderer || !nodeIn || !nodeOut) return;

    // ノードのワールド座標を取得
    Vector3 posIn = nodeIn.transform.position;
    Vector3 posOut = nodeOut.transform.position;

    Vector3 direction = (posOut - posIn).normalized;


    Vector3 scaleIn = nodeInRect.lossyScale;
    Vector3 scaleOut = nodeOutRect.lossyScale;


    Vector2 sizeInWorld = Vector2.Scale(nodeInRect.rect.size, new Vector2(scaleIn.x, scaleIn.y));
    Vector2 sizeOutWorld = Vector2.Scale(nodeOutRect.rect.size, new Vector2(scaleOut.x, scaleOut.y));

    Vector2 halfSizeInWorld = sizeInWorld * 0.5f;
    Vector2 halfSizeOutWorld = sizeOutWorld * 0.5f;

    // 各ノードから線をどれだけ短くするか計算
    // X と Y の絶対値を掛け合わせてオフセットを計算
    float offsetIn = halfSizeInWorld.x * Mathf.Abs(direction.x) + halfSizeInWorld.y * Mathf.Abs(direction.y) + 0.05f;
    float offsetOut = halfSizeOutWorld.x * Mathf.Abs(direction.x) + halfSizeOutWorld.y * Mathf.Abs(direction.y) + 0.05f;

    // ノードの中心からオフセットした位置を計算
    Vector3 nodeInPosition = posIn + direction * offsetIn;
    Vector3 nodeOutPosition = posOut - direction * offsetOut;

    // ラインレンダラーの位置を設定
    lineRenderer.SetPosition(0, nodeInPosition);
    lineRenderer.SetPosition(1, nodeOutPosition);


}


    public void DestroyEdge() 
    {
        Destroy(this.gameObject);
    }

    public MREdgeData GetMREdgeData()
    {
        return this._mrEdgeData;

    }

}
}



