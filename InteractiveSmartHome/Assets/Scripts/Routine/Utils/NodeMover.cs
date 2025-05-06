using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class NodeMover : MonoBehaviour
{
    private void Update() {

        this.transform.LookAt(Camera.main.transform.position);

        this.transform.Rotate(0, 180, 0); 

    
}




  

    


}
