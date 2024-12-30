using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using UnityEngine;
using UnityEngine.Events;

public class DebugInput : MonoBehaviour
{
    public UnityEvent  OnSpacePressed; 

    public UnityEvent OnIndexPressed;
    public UnityEvent OnIndexReleased;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.Space))
      {
        OnSpacePressed.Invoke();
      }

     if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {
            Debug.Log("[ControllerInput] Pressed");
            OnIndexPressed.Invoke();
            SASpeechRecognizer.Instance.ActivateVoice();
        }


        if(OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
             Debug.Log("[ControllerInput] Released");
            OnIndexReleased.Invoke();
            SASpeechRecognizer.Instance.DeactivateVoice();
        }


    }
}
