using System;
using System.Collections;
using System.Collections.Generic;
using NodeTypes;
using UnityEngine;




namespace MRFlow.Component{
public class MRNode : MonoBehaviour
{


    private MRNodeData _mrNodeData;
    public MRNodeData mrNodeData => _mrNodeData;

    private void Awake() {
        this._mrNodeData = new MRNodeData(
            id: Guid.NewGuid(),
            type: "test",
            action_data : new {name = "Undefined"}, 
            position : this.transform.position
        );
        
        Debug.Log (this._mrNodeData);
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

}