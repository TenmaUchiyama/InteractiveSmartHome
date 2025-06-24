using UnityEngine;
using TMPro;
using UnityEngine.UI;


[RequireComponent(typeof(LineRenderer))]
public class UIButtonLabel : MonoBehaviour
{

    [Header("表示ラベル・背景色")]
    [SerializeField] private TextMeshProUGUI labelText;
    
    [SerializeField] Image backgroundImage;

    [Header("対象（例：Yボタン）")]
    [SerializeField] private Transform targetTransform;


    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = true;
        lineRenderer.useWorldSpace = true;

        // ラインの基本設定
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.001f;
        lineRenderer.endWidth = 0.001f;

        // 色をデフォルトで白に
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    void LateUpdate()
    {
        if (targetTransform == null) return;

        // ラベルの位置
        Vector3 labelPos = transform.position;
        // ターゲットの位置
        Vector3 targetPos = targetTransform.position;

        // 線を描画
        lineRenderer.SetPosition(0, labelPos);
        lineRenderer.SetPosition(1, targetPos);
    }

    /// <summary>
    /// ラベルの文字列を外部から設定
    /// </summary>
    public void SetLabel(string text)
    {
        if (labelText != null)
            labelText.text = text;
    }


    public void SetLabelColor(Color color)
    {
        if (labelText != null)
            labelText.color = color;
    }

    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
            backgroundImage.color = color;


    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

}
