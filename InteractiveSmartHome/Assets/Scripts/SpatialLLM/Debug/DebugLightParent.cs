using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using Unity.XR.CoreUtils;
using UnityEngine;



namespace SpatialLLM.Debugging
{
[ExecuteAlways]
public class DebugLightParent : MonoBehaviour
{
    [Range(0,100)]
    public float intensity = 1;


    public bool isEnable = true;

    private void Start() {

        
         foreach (Transform child in transform)
        {
            // 子オブジェクトに DebugLight コンポーネントを追加
            if (child.gameObject.GetComponent<DebugLight>() == null)
            {
                DebugLight comp = child.gameObject.AddComponent<DebugLight>();
                    Debug.Log($"Added DebugLight to {child.gameObject.name}");

        
                comp.ExpandBounds(new Vector3(0.1f, 0.3f, 0.1f));
            }else{
                DebugLight comp = child.gameObject.GetComponent<DebugLight>();
               comp.ExpandBounds(new Vector3(0.1f, 0.3f, 0.1f));
            }


        //     if(!child.gameObject.GetComponent<SADevice>())
        //     {
        //         child.gameObject.AddComponent<SADevice>();
        //     }
        }

    }


    void OnValidate()
    {
        Light[] lights =  GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            light.intensity = intensity /  10;
        }  


    
        foreach (Light light in lights)
        {
            light.enabled = isEnable;
        }
      
    } 


    
}
}