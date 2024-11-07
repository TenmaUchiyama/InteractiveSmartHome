using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MRFlow.Test 
{public class TestUI : MonoBehaviour
{
    [SerializeField] Button startAllRoutineButton; 


    // Start is called before the first frame update
    void Start()
    {
       
    }

 
private void OnStartAllRoutineClicked(){
    Debug.Log("StartAllRoutine");
}
}}