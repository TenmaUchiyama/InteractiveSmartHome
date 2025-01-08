using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif





public class DebugMinimap : MonoBehaviour
{
   public LayerMask layerMask;



     [Header("References")]
    public Transform houseTransform; // 家モデルのTransform
    public GameObject saParentObj;  // デバイスの親オブジェクト
    public RectTransform uiCanvas;  // UIキャンバスのRectTransform
    public Vector2 uiCenterOffset = Vector2.zero; // UI中心のオフセット

    [Header("UI Settings")]
    public GameObject deviceUIPrefab; // デバイス用のUIアイコンPrefab
    public float uiScaleFactor = 1.0f; // デバイスをUI上にマッピングする際のスケール
    public GameObject iconsParent;

#if UNITY_EDITOR
    [ContextMenu("Childのレイヤーを変える")]
    public void Testing() 
    {
        // LayerMaskを単一のレイヤー番号に変換
        int layerNumber = Mathf.FloorToInt(Mathf.Log(layerMask.value, 2));

        foreach (Transform child in this.transform)
        {
            child.gameObject.layer = layerNumber;
        }
    }



    [ContextMenu("クリックして実行")]
    public void MapDevicesToUIInEditor()
    {
        if (houseTransform == null || saParentObj == null || uiCanvas == null || deviceUIPrefab == null)
        {
            Debug.LogError("必要な参照が設定されていません！");
            return;
        }

        // UIキャンバス内に新しいIconsオブジェクトを作成
        CreateIconsParent();

        // 家モデルの中心点を基準にする
        Vector3 housePosition = houseTransform.position;

        // saParentObjの子オブジェクトを取得してループ処理
        foreach (Transform child in saParentObj.transform)
        {
            // デバイスの相対位置を計算（X軸とZ軸ベース）
            Vector3 deviceWorldPosition = child.position;
            Vector3 relativePosition = deviceWorldPosition - housePosition;

            // 相対位置をCanvasのローカル座標にマッピング
            Vector3 localPosition = new Vector3(
                relativePosition.x * uiScaleFactor + uiCenterOffset.x,
                relativePosition.z * uiScaleFactor + uiCenterOffset.y,
                0 // Z軸はCanvasの深さに合わせる（通常0）
            );

            // デバイスUIを生成
            GameObject uiObject = PrefabUtility.InstantiatePrefab(deviceUIPrefab, iconsParent.transform) as GameObject;

            RectTransform uiRectTransform = uiObject.GetComponent<RectTransform>();
            if (uiRectTransform != null)
            {
                // World Spaceの場合、localPositionを直接設定
                uiRectTransform.localPosition = localPosition;
                uiObject.transform.SetParent(iconsParent.transform, false); // Iconsオブジェクトの子に設定
            }
        }

        Debug.Log("デバイスのUIマッピングが完了しました！");
    }

    private void CreateIconsParent()
    {
        // 既存のIconsオブジェクトを削除（重複防止）
        if (iconsParent != null)
        {
            DestroyImmediate(iconsParent);
        }

        // 新しいIconsオブジェクトを作成してUIキャンバス内に配置
        iconsParent = new GameObject("Icons");
        iconsParent.transform.SetParent(uiCanvas, false);

        // 必要ならRectTransformを追加
        var rectTransform = iconsParent.AddComponent<RectTransform>();
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
    }

}
#endif