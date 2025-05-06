
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace SpatialLLM.Minimap{

public class MinimapFollower : MonoBehaviour
{
   [SerializeField] private Transform centerEye; 
   [SerializeField] private Transform minimap;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, -0.5f, 0.5f);

    public float rotationLerpSpeed = 5f; 

    Vector3 targetposition;
    Quaternion targetRotation;
  
    private void Start() {
        SetHeadSetTransform();
    }

    private bool isTransitioning = false; // 補間中かどうかのフラグ

private void Update()
{
    if (isTransitioning)
    {
        SetHeadSetTransform();
    }
    else if (IsMinimapOutofThresh())
    {
        StartLerp();
    }
}

private void StartLerp()
{
    isTransitioning = true;
}


    public void SetHeadSetTransform()
    {
    
 
        Vector3 centerEyePos = centerEye.position;
        Vector3 centerEyeForward = centerEye.forward;   

        Vector3 forwardHor = new Vector3(centerEyeForward.x, positionOffset.y, centerEyeForward.z).normalized;

        Vector3 targetPos = centerEyePos + forwardHor * positionOffset.z;

         minimap.position = Vector3.Lerp(minimap.position, targetPos, Time.deltaTime * rotationLerpSpeed);

        Vector3 directionToCenterEye = (centerEyePos - minimap.position).normalized;
         minimap.rotation  = Quaternion.LookRotation(-directionToCenterEye, Vector3.up); 
    if (Vector3.Distance(minimap.position, targetPos) < 0.01f)
    {
        isTransitioning = false;
    }
    }

bool IsMinimapOutofThresh()
{
    // CenterEyeの方向を取得（y成分を無視）
    Vector3 centerEyeForwardXZ = new Vector3(centerEye.forward.x, 0, centerEye.forward.z).normalized;

    // minimapの方向を取得（centerEyeからminimapへのベクトル）
    Vector3 minimapToCenterEyeXZ = new Vector3(minimap.position.x - centerEye.position.x, 0, minimap.position.z - centerEye.position.z);

    // 距離が極端に短い場合はスキップ（誤差回避）
    if (minimapToCenterEyeXZ.magnitude < 0.01f)
    {
        Debug.Log("<color=red>Minimap is too close to CenterEye, skipping angle calculation.</color>");
        return false;
    }

    // ベクトルを正規化
    minimapToCenterEyeXZ.Normalize();

    // centerEyeForwardXZ と minimapToCenterEyeXZ の角度を計算
    float angle = Vector3.Angle(centerEyeForwardXZ, minimapToCenterEyeXZ);

    

    // 角度が45度以上かをチェック
    return angle > 45f;
}


    #if UNITY_EDITOR
   [ContextMenu("クリックして実行")]    
    public void MoveObjectTo()
    {
        SetHeadSetTransform();
    }
    
    #endif

   
}
}