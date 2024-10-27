using System.Collections;
using System.Collections.Generic;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;

public class NodeHolder : MonoBehaviour
{
      private NodeType holdingNodeType; 
      
      [SerializeField] private RayInteractor rayInteractor;


      public void SetHoldNodeType(NodeType nodeType)
      {
          holdingNodeType = nodeType;
          rayInteractor.enabled = false;
      }

      public void CancelHoldNode()
      {
            holdingNodeType = NodeType.None;
            rayInteractor.enabled = true;
      }




}
