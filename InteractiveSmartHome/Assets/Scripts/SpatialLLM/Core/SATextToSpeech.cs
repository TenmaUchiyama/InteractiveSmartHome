using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;



namespace SpatialLLM.Core{
public class SATextToSpeech : Singleton<SATextToSpeech>
{

    [SerializeField] private TTSSpeaker _speaker;

  

    // Update is called once per frame
    private void Update() 
    {

        
       if(Input.GetKeyDown(KeyCode.Space))
       {
        Debug.Log("Clicked Space");
         _speaker.Speak("hello");    
       }
    }
}
}