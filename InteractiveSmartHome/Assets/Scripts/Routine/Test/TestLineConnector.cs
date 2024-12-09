using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineConnector : MonoBehaviour
{
    public GameObject objectTo;  // 線を引く元のオブジェクト
    public GameObject objectFrom;  // 線を引く先のオブジェクト
    private LineRenderer lineRenderer;
     void Start()
    {
        // LineRendererコンポーネントを取得
        lineRenderer = GetComponent<LineRenderer>();

        // 線の幅を設定 (必要に応じて変更)
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // 頂点の数を設定 (2つの点)
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // ObjectAとObjectBの位置に基づいて線を引く
        lineRenderer.SetPosition(0, objectTo.transform.position);  // 始点 (ObjectA)
        lineRenderer.SetPosition(1, objectFrom.transform.position);  // 終点 (ObjectB)
    }
}
