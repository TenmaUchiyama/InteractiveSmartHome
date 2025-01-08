using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceDebugger : MonoBehaviour
{
    private Bounds bounds;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            bounds = renderer.bounds;
        }
    }

    void Update()
    {
        if (bounds.size != Vector3.zero)
        {
            // 各辺をDebug.DrawLineで描画
            Debug.DrawLine(bounds.min, bounds.max, Color.blue);
        }
    }


     void OnDrawGizmos()
    {
        // シーンビュー用にGizmosで描画
        if (bounds.size != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }



    #if UNITY_EDITOR
    void AttachComponents() 
    {
        // BoxColliderをアタッチ
        //ColliderSurfaceをアタッチ
        
    }
    #endif
}
