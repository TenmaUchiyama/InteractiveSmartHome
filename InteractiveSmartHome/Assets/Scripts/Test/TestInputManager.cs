using System.Collections;
using System.Collections.Generic;
using MRFlow.Core;
using UnityEngine;

public class TestInputManager : MonoBehaviour
{
  

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left Mouse Button Clicked");
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if(hit.transform.TryGetComponent(out NodeHandler nodeHandler))
                {
                    EdgeManager.Instance.SelectHandlerDown(nodeHandler);
                }

                
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Left Mouse Button Released");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            NodeHandler hitHandler = null;

             if(Physics.Raycast(ray, out RaycastHit hit))
            {
                if(hit.transform.TryGetComponent(out NodeHandler nodeHandler))
                {
                    hitHandler = nodeHandler;
                }
                
            }

            EdgeManager.Instance.SelectHandlerUp(hitHandler);
            
        }
    }
}
