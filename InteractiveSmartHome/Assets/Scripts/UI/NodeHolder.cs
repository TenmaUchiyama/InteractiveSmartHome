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
        
      }

      public void CancelHoldNode()
      {
            holdingNodeType = NodeType.None;
            // rayInteractor.enabled = true;
      }

      public void Test() 
      {
            Debug.Log("<color=red>Test</color>");
      }
      private void Update() {
            if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                
            }
      }



}
