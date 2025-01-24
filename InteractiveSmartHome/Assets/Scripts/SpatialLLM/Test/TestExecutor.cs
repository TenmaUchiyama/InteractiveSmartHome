using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SpatialLLM.Experiment;
using UnityEngine;

public class TestExecutor : SystemExecutor
{
    private bool isStarted = false;

  

    // Update is called once per frame
    void Update()
    {
        if(!isStarted) return;

        if(Input.GetKeyDown(KeyCode.I))
        {
            this.CompleteOperation();
        }
    }




   
}
