using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MRFlow.Test
{public class TestSimple : MonoBehaviour
{
    [SerializeField] GameObject gameObjectPrefab; 

      private Vector3[] positions = new Vector3[]
    {
        new Vector3(0, 0, 0),    // 位置1
        new Vector3(2, 0, 0),    // 位置2
        new Vector3(4, 0, 0)     // 位置3
    };

    void Start()
    {
        // 3つのオブジェクトをそれぞれの位置に生成
        for (int i = 0; i < positions.Length; i++)
        {
            Instantiate(gameObjectPrefab, positions[i], Quaternion.identity);
        }
    }



}
}