using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NodeTypes;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;



namespace MRFlow.Component 
{

  public class MREdge : MonoBehaviour
  {

      private MREdgeData _mrEdgeData; 

    public MREdgeData mrEdgeData => _mrEdgeData;
    
    MRNode nodeIn;
    MRNode nodeOut;

    Transform inHandlerTransform;
    Transform outHandlerTransform;

    float offsetIn = 0.1f;
    float offsetOut = 0.1f;
     public void setNodes(NodeHandler nodeIn, NodeHandler nodeOut)
    {


        this.nodeIn = nodeIn.GetNode();
        this.nodeOut = nodeOut.GetNode();
        this.inHandlerTransform = this.nodeIn.transform;
        this.outHandlerTransform = this.nodeOut.transform; 

        if(this.nodeIn.transform.TryGetComponent(out RectTransform rectTransformIn))
        {
            offsetIn = rectTransformIn.rect.width * 0.001f;
        }

        if(this.nodeOut.transform.TryGetComponent(out RectTransform rectTransformOut))
        {
            offsetOut = rectTransformOut.rect.width * 0.001f;
        }
 
    }
private void Update() 
{
    if (!nodeIn || !nodeOut) return;

    Vector3 direction = outHandlerTransform.position - inHandlerTransform.position;
    float distance = direction.magnitude;

    

    Vector3 startPoint = inHandlerTransform.position + direction.normalized * offsetIn;
    Vector3 endPoint = outHandlerTransform.position - direction.normalized * offsetOut;

    Vector3 newDirection = endPoint - startPoint;
    float newDistance = newDirection.magnitude;

    Vector3 midPoint = (startPoint + endPoint) / 2.0f;
    transform.position = midPoint;

    transform.rotation = Quaternion.FromToRotation(Vector3.up, newDirection);

    transform.localScale = new Vector3(transform.localScale.x, newDistance / 2.0f, transform.localScale.z);
}
  }
// public class MREdge : MonoBehaviour
// {
    
//     private MREdgeData _mrEdgeData; 

//     public MREdgeData mrEdgeData => _mrEdgeData;
    
//     MRNode nodeIn;
//     MRNode nodeOut;

//     Transform inHandlerTransform;
//     Transform outHandlerTransform;

//     private LineRenderer lineRenderer;

    
//     private void Start() {
//       this.lineRenderer = this.GetComponent<LineRenderer>();
      
//         lineRenderer.startWidth = 0.1f;
//         lineRenderer.endWidth = 0.1f;

//         lineRenderer.positionCount = 2;
//     }
//     public void setNodes(NodeHandler nodeIn, NodeHandler nodeOut)
//     {


//         this.nodeIn = nodeIn.GetNode();
//         this.nodeOut = nodeOut.GetNode();
//         this.inHandlerTransform = nodeIn.transform;
//         this.outHandlerTransform = nodeOut.transform;
 
//     }

//   private void Update() {
  
//     if(!lineRenderer || !nodeIn || !nodeOut) return;
    
//     lineRenderer.SetPosition(0, outHandlerTransform.position);
//     lineRenderer.SetPosition(1, inHandlerTransform.position);

//   }


// }


}



