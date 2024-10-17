using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.FullSerializer;
public class TestUI : MonoBehaviour
{
    [SerializeField] Button startAllRoutineButton; 


    // Start is called before the first frame update
    void Start()
    {
       
    }

 
private void OnStartAllRoutineClicked(){
    Debug.Log("StartAllRoutine");
}
}