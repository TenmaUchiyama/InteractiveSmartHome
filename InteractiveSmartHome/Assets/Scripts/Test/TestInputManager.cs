using System.Collections;
using System.Collections.Generic;
using MRFlow.Core;
using UnityEngine;

public class TestInputManager : MonoBehaviour
{
  

    void Update()
    {
        Ray testRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(testRay.origin, testRay.direction * 10, Color.red); 

        if (Input.GetMouseButtonDown(0))
        {
          
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
             
                if(hit.transform.TryGetComponent(out NodeHandler nodeHandler))
                {

                    EdgeManager.Instance.SetSelectHandler(nodeHandler);
                }

                
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
       

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            NodeHandler hitHandler = null;

             if(Physics.Raycast(ray, out RaycastHit hit))
            {


                if(hit.transform.TryGetComponent(out NodeHandler nodeHandler))
                { 
                    
                    hitHandler = nodeHandler;
                }
                
            }

            EdgeManager.Instance.SetSelectHandler(hitHandler);
            
        }
    }



}
