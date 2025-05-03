using System.Collections.Generic;
using UnityEngine;

public class MouseCircleDrawer : MonoBehaviour
{
     [Header("描画設定")]
    public int mouseButton = 0;               // 左クリック
    public float drawDistance = 1f;           // カメラ前方 1m
    public float minSegmentDistance = 0.01f;  // 点を追加する最小距離
    public LineRenderer linePrefab;           // LineRenderer プレハブ

    [Header("選択対象")]
    public string selectableTag = "Selectable";

    Camera cam;
    Plane drawPlane;
    LineRenderer currentLine;
    List<Vector3> worldPoints = new List<Vector3>();
    private LineRenderer previousLine = null;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // ① 描画用平面を常に更新
        Vector3 planeCenter = cam.transform.position + cam.transform.forward * drawDistance;
        drawPlane = new Plane(-cam.transform.forward, planeCenter);

        // ② マウス操作で描画開始・継続・終了
        if (Input.GetMouseButtonDown(mouseButton))
        {
            StartLine();
            AddPoint();
        }
        else if (currentLine != null && Input.GetMouseButton(mouseButton))
        {
            AddPoint();
        }
        else if (currentLine != null && Input.GetMouseButtonUp(mouseButton))
        {
            FinishLine();
            SelectObjectsInScreenCircle();
           
        }
    }

    // 新しい LineRenderer を生成
    void StartLine()
    {
        worldPoints.Clear();

          if (currentLine != null)
        {
            Destroy(currentLine.gameObject);
        }
        currentLine = Instantiate(linePrefab);
        currentLine.positionCount = 0;
        currentLine.loop = false;

    }

    // 平面上に新しい点を追加
    void AddPoint()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (drawPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPos = ray.GetPoint(enter);
            if (worldPoints.Count == 0
                || Vector3.Distance(worldPoints[worldPoints.Count - 1], worldPos) > minSegmentDistance)
            {
                worldPoints.Add(worldPos);
                currentLine.positionCount = worldPoints.Count;
                currentLine.SetPosition(worldPoints.Count - 1, worldPos);
            }
        }
    }

    // 線を閉じ、見た目上円にする
    void FinishLine()
    {
        if (worldPoints.Count > 1)
            currentLine.loop = true;
    }

    // スクリーン座標ベースの円選択
    void SelectObjectsInScreenCircle()
    {
        // 1) worldPoints をスクリーン座標に変換
        var screenPoints = new List<Vector2>();
        foreach (var wp in worldPoints)
        {
            Vector3 sp = cam.WorldToScreenPoint(wp);
            screenPoints.Add(new Vector2(sp.x, sp.y));
        }

        // 2) 中心 (重心) と半径 (最大距離) を算出
        Vector2 center = Vector2.zero;
        foreach (var p in screenPoints) center += p;
        center /= screenPoints.Count;

        float radius = 0f;
        foreach (var p in screenPoints)
            radius = Mathf.Max(radius, Vector2.Distance(center, p));

        // 3) タグ "Selectable" の全オブジェクトを走査
        var candidates = GameObject.FindGameObjectsWithTag(selectableTag);
        foreach (var go in candidates)
        {
            Vector3 sp = cam.WorldToScreenPoint(go.transform.position);
            // z>0 でカメラ前方、かつ円内に含まれるか判定
            if (sp.z > 0 && Vector2.Distance(new Vector2(sp.x, sp.y), center) <= radius)
            {
                Debug.Log($"Selected: {go.name}");
                // 任意の処理例：色を変える
                var rend = go.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = Color.red;
            }
        }
    }

}
