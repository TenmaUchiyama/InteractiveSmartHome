using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class NodeMover : MonoBehaviour
{
  private GameObject hmd; 
  public float minScale = 0.5f; // 最小スケール
    public float maxScale = 2.0f; // 最大スケール
    public float minDistance = 1.0f; // 最小距離
    public float maxDistance = 10.0f; // 最大距離



  private void Start() {
    hmd = MRInputManager.Instance.GetHmdObject();
  }


  private void Update() {
       Vector3 direction = hmd.transform.position - this.transform.position;

    this.transform.rotation = Quaternion.LookRotation(-direction);


     float distance = Vector3.Distance(this.transform.position, hmd.transform.position);

        float scale = Mathf.Lerp(minScale, maxScale, (distance - minDistance) / (maxDistance - minDistance));

        scale = Mathf.Clamp(scale, minScale, maxScale);

        this.transform.localScale = new Vector3(scale, scale, scale);

  }
}
