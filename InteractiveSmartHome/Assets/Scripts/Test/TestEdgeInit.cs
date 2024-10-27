using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using MRFlow.Core;
using UnityEngine;

public class TestEdgeInit : MonoBehaviour
{
    [SerializeField] NodeHandler btnNodeOut; 
    

    [SerializeField] NodeHandler timerNodeIn; 

    [SerializeField] NodeHandler timerNodeOut; 

    [SerializeField] NodeHandler lightIn; 


    private void Start() {
       
        EdgeManager.Instance.SetSelectHandler(btnNodeOut); 
        EdgeManager.Instance.SetSelectHandler(timerNodeIn);

        EdgeManager.Instance.SetSelectHandler(timerNodeOut);
        EdgeManager.Instance.SetSelectHandler(lightIn);
    }
}
