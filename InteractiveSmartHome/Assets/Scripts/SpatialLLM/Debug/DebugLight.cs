using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpatialLLM.Debugging
{
    
[ExecuteAlways]
public class DebugLight : MonoBehaviour
{
    private Bounds bounds;
        public Vector3 boundsPadding = new Vector3(0,0,0); // 境界を大きくする量

        void Start()
        {
            // Rendererを取得してBoundsを取得
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
        }


  


       public void ExpandBounds (Vector3 padding)
       {
              bounds.Expand(padding);
       }



}
}