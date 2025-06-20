using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using UnityEngine;


public class VRCircleSelector : MonoBehaviour
{
    [SerializeField] GameObject selector;
    public Color selectorColor = new Color(1f, 0f, 0f);

    public float drawDistance = 1f;           // カメラ前方 1m
    public float minSegmentDistance = 0.01f;  // 点を追加する最小距離
    public LineRenderer linePrefab;           // LineRenderer プレハブ
    [SerializeField] Material surfaceMaterial; // 面表示用マテリアル

    private LineRenderer currentLine;
    private LineRenderer previousLine = null;
    private GameObject previousSurface = null;
    private List<Vector3> drawPoints = new List<Vector3>();
    private Camera cam;


    private bool isSelecting = false;


    [Header("選択対象")]
    public string selectableTag = "Selectable";


    public void SetSelectionStarted(bool started)
    {
        isSelecting = started;
        if (isSelecting)
        {
            Debug.Log("[VRCircleSelector] Selection started.");
        }
        else
        {
            Debug.Log("[VRCircleSelector] Selection stopped.");
        }
    }
    void Start()
    {
        selector.GetComponent<MeshRenderer>().material.color = selectorColor;
        cam = Camera.main;
       
    }

    void Update()
    {



        if (!isSelecting)
        {
            return;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
           
            StartLine();
            AddPoint();
        }
        else if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            AddPoint();
        }

        if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger))
        {
            FinishLine();
            currentLine = null;
        }

        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     var candidates = GameObject.FindGameObjectsWithTag(selectableTag);
        //     foreach (var candidate in candidates)
        //         candidate.GetComponent<MeshRenderer>().material.color = Color.white;
        // }
    }

    void StartLine()
    {


        Debug.Log("[StartLine] Drawing started.");
        drawPoints.Clear();

        if (previousLine != null)
            Destroy(previousLine.gameObject);
        if (previousSurface != null)
            Destroy(previousSurface);

        currentLine = Instantiate(linePrefab);
        currentLine.startColor = selectorColor;
        currentLine.endColor = selectorColor;
        currentLine.positionCount = 0;
        currentLine.loop = false;

        previousLine = currentLine;
    }

    void AddPoint()
    {
        Vector3 drawingPoint = selector.transform.position;
        if (drawPoints.Count == 0 || Vector3.Distance(drawPoints[^1], drawingPoint) > minSegmentDistance)
        {
            drawPoints.Add(drawingPoint);
            currentLine.positionCount = drawPoints.Count;
            currentLine.SetPosition(drawPoints.Count - 1, drawingPoint);

            // デバッグログ: Drawing中のポイント
            Debug.Log($"[Drawing] Added Point: {drawingPoint}");
        }
    }

    void FinishLine()
    {
        if (drawPoints.Count > 1)
        {
            currentLine.loop = true;

            // デバッグログ: Drawingが終了したとき
            Debug.Log($"[FinishLine] Drawing completed with {drawPoints.Count} points.");
        }

        ScreenSpaceSelect();
        if (previousLine != null)
            Destroy(previousLine.gameObject);
        if (previousSurface != null)
            Destroy(previousSurface);
    }



    /// <summary>
    /// スクリーン上の多角形（円を投影した形）内にあるオブジェクトを選択
    /// </summary>
    void ScreenSpaceSelect()
    {
        // 1. スクリーン多角形を構成する点を取得（カメラ後方を除外）
        List<Vector2> screenPoly = new List<Vector2>();
        foreach (var wp in drawPoints)
        {
            Vector3 sp = cam.WorldToScreenPoint(wp);

            if (sp.z <= 0f)
                continue; // カメラの背後にある点は無効

            screenPoly.Add(new Vector2(sp.x, sp.y));
        }

        // 有効な多角形でなければスキップ
        if (screenPoly.Count < 3)
        {
            Debug.LogWarning("[ScreenSpaceSelect] Invalid polygon: Less than 3 points.");
            return;
        }

        Debug.Log($"[ScreenSpaceSelect] Valid polygon with {screenPoly.Count} points.");

        foreach (var device in SADeviceRef.Instance.GetAllDevices())
        {
            // 2. 各デバイスの位置をスクリーン座標に変換
            {
                Vector3 sp = cam.WorldToScreenPoint(device.transform.position);

                // カメラの背後にあるものは無視
                if (sp.z <= 0f)
                    continue;

                Vector2 screenPos = new Vector2(sp.x, sp.y);

                // 3. スクリーン多角形内にあるかチェック
                if (!IsPointInPolygon(screenPoly, screenPos))
                    continue;

                // デバッグログ: 選択されたデバイス
                Debug.Log($"[ScreenSpaceSelect] Selected Device: {device.name}");

                // 5. 選択されたオブジェクトを色付け
                device.SetIsSelected(true);
                device.GetComponent<DrawOnHover>().DrawSelect();
            }
        }
        /// <summary>
        /// スクリーン座標多角形内判定（レイキャスト法）
        /// </summary>
        bool IsPointInPolygon(List<Vector2> poly, Vector2 p)
        {
            bool inside = false;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            {
                Vector2 pi = poly[i], pj = poly[j];
                bool intersect = ((pi.y > p.y) != (pj.y > p.y)) &&
                                 (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x);
                if (intersect)
                    inside = !inside;
            }
            return inside;
        }
    }


// ビームを伸ばす最大距離（必要に応じて調整）
public float beamMaxDistance = 10f;


}
