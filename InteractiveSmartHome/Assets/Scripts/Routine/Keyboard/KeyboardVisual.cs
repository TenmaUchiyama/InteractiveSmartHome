using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class KeyboardVisual : MonoBehaviour
{
   [SerializeField]
    private LineRenderer leftLinePointer;

    [SerializeField]
    private LineRenderer rightLinePointer;


    [SerializeField]
    private RayInteractor rightRayInteractor; 

    [SerializeField]
    private RayInteractor leftRayInteractor;

    private float linePointerInitialWidth_;

    // コントローラーの最大レイ距離
    private const float RAY_MAX_DISTANCE = 100.0f;


    private bool isKeyboardOpen = false;

    // 仮想キーボードのコライダー（必要に応じてアサイン）
    [SerializeField]
    private Collider virtualKeyboardCollider;

    private void Start()
    {
        this.ToggleKeyboardVisual(false);
        linePointerInitialWidth_ = Mathf.Max(leftLinePointer.startWidth, rightLinePointer.startWidth);
    }


  
    private void Update()
    {
        // 毎フレーム、ラインレンダラーを更新
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        // 左コントローラーのラインレンダラーを更新
        if (leftLinePointer != null)
        {
            leftLinePointer.enabled = false;
            UpdateLineRendererFromController(OVRInput.Controller.LTouch, leftLinePointer);
        }

        // 右コントローラーのラインレンダラーを更新
        if (rightLinePointer != null)
        {
            rightLinePointer.enabled = false;
            UpdateLineRendererFromController(OVRInput.Controller.RTouch, rightLinePointer);
        }
    }

    private void UpdateLineRendererFromController(OVRInput.Controller controller, LineRenderer linePointer)
    {
        // コントローラーが接続されているか確認
        if (!OVRInput.IsControllerConnected(controller) || !isKeyboardOpen)
        {
            return;
        }

        // コントローラーのポジションとローテーションを取得
        // OVRInput.GetLocalControllerPosition/Rotation はローカル空間の情報なので、ワールド空間に変換
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(controller);
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(controller);

        // コントローラーの Transform をワールド空間で取得
        Transform controllerTransform = new GameObject().transform;
        controllerTransform.position = controllerPosition;
        controllerTransform.rotation = controllerRotation;
        controllerTransform.SetParent(transform, false); // 親を設定してワールド空間に

        // ラインレンダラーの設定
        linePointer.startWidth = linePointerInitialWidth_;
        linePointer.endWidth = linePointer.startWidth;
        linePointer.enabled = true;

        // ラインの始点と終点を設定
        Vector3 startPoint = controllerTransform.position;
        Vector3 direction = controllerTransform.forward;
        Vector3 endPoint = startPoint + direction * 2.5f;

        linePointer.SetPosition(0, startPoint);

        // レイキャストによる終点の調整
        Ray ray = new Ray(startPoint, direction);
        RaycastHit hit;

        if (virtualKeyboardCollider != null && virtualKeyboardCollider.Raycast(ray, out hit, RAY_MAX_DISTANCE))
        {
            // 仮想キーボードのコライダーにヒットした場合
            linePointer.SetPosition(1, hit.point);
        }
        else
        {
            // ヒットしなかった場合、一定距離先を終点に設定
            linePointer.SetPosition(1, endPoint);
        }

        // 一時的な Transform を破棄
        Destroy(controllerTransform.gameObject);
    }



      public void ToggleKeyboardVisual(bool isShow)
    {

        isKeyboardOpen = isShow;
        leftLinePointer.enabled = isShow;
        rightLinePointer.enabled = isShow;

        rightRayInteractor.enabled = !isShow;
        leftRayInteractor.enabled = !isShow;
    }


}
