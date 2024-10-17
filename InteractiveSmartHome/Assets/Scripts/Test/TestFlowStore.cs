using System;
using UniRx;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using NodeTypes;
public class TestFlowStore : MonoBehaviour
{

    private IDisposable _subscription;
    
    void Start()
    {
        var flowStore = FlowStore.Instance; 

         _subscription = flowStore.nodes.ObserveAdd().Subscribe(x => {
            List<Node> nodes = FlowStore.GetStore(FlowStore.Instance.nodes);
            Debug.Log(JsonConvert.SerializeObject(nodes));
         });
    }


    

   private void OnDestroy()
    {
        // オブジェクトが破棄されるときに購読を解除
        _subscription?.Dispose();
    }
}
