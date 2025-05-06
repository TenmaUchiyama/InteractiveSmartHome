using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using UnityEngine;

public class ControllerPointer : MonoBehaviour
{
    [SerializeField] GameObject drawerOffset;
    [SerializeField] float laserLength = 1.5f;
    [SerializeField] LineRenderer lineRenderer;

    void Start()
    {

        GenerateLine();
        // if (lineRenderer == null)
        // {
        //     lineRenderer = gameObject.AddComponent<LineRenderer>();
        // }

        // lineRenderer.positionCount = 2;
        // lineRenderer.startWidth = 0.01f;
        // lineRenderer.endWidth = 0.01f;
        // lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        // lineRenderer.material.color = Color.red;
    }

    // void Update()
    // {
    //      return;

    //     Vector3 start = drawerOffset.transform.position;
    //     Vector3 direction = drawerOffset.transform.forward;
    //     Vector3 end = start + direction * laserLength;

    //     // レーザーの表示
    //     lineRenderer.SetPosition(0, start);
    //     lineRenderer.SetPosition(1, end);

    //     // Raycastを飛ばしてSADeviceを取得
    //     if (Physics.Raycast(start, direction, out RaycastHit hit, laserLength))
    //     {
    //         SADevice device = hit.collider.GetComponent<SADevice>();
    //         if (device != null)
    //         {
    //             Debug.Log("SADevice hit: " + device.name);
    //             // ここで必要な処理を呼び出す（例: device.OnHit();）
    //         }
    //     }
    // }


    private GameObject laserTipCylinder;

private void GenerateLine()
{
    // すでに存在していたら破棄して再生成
    if (laserTipCylinder != null)
    {
        Destroy(laserTipCylinder);
    }

    // 1. Cylinder生成
    laserTipCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    laserTipCylinder.name = "LaserTipTriggerCylinder";

    // 2. サイズと形状（細長いレーザー風）
    float radius = 0.01f;
    float length = 0.1f;
    laserTipCylinder.transform.localScale = new Vector3(radius, length * 0.5f, radius); // Y=高さの半分

    // 3. 初期位置と回転（とりあえず前方1.5m）
    Vector3 origin = transform.position + transform.forward * 1.5f;
    laserTipCylinder.transform.position = origin;
    laserTipCylinder.transform.rotation = Quaternion.LookRotation(transform.forward);

    // 4. Trigger設定
    Collider col = laserTipCylinder.GetComponent<Collider>();
    col.isTrigger = true;

    // 5. Rigidbody追加（Trigger発火に必要）
    Rigidbody rb = laserTipCylinder.AddComponent<Rigidbody>();
    rb.isKinematic = true;

    // 6. 視覚設定（必要に応じて）
    var renderer = laserTipCylinder.GetComponent<MeshRenderer>();
    if (renderer != null)
    {
        renderer.material.color = Color.yellow;
        renderer.enabled = false; // trueにすれば見える
    }

    // 7. 必要なら親を設定（コントローラと一緒に動かす）
    laserTipCylinder.transform.SetParent(this.transform, worldPositionStays: true);
}


}
