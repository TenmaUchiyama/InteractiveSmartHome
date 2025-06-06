using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ArrangementDebugger))]
public class ArrangementDebugerHelper : Editor
{


    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();


        if (GUILayout.Button("Turn On With ID"))
        {
            ArrangementDebugger arrangementDebugger = (ArrangementDebugger)target;
            arrangementDebugger.TurnOnWithId();
        }

        if (GUILayout.Button("Turn On Current"))
        {
            ArrangementDebugger arrangementDebugger = (ArrangementDebugger)target;
            arrangementDebugger.TurnOn();
        }

        if (GUILayout.Button("Turn On Previous Arrangement"))
        {
            ArrangementDebugger arrangementDebugger = (ArrangementDebugger)target;
            arrangementDebugger.TurnOnPreviousArrangement();
        }

        if (GUILayout.Button("Turn On Next Arrangement"))
        {
            ArrangementDebugger arrangementDebugger = (ArrangementDebugger)target;
            arrangementDebugger.TurnOnNextArrangement();
        }
    }
}
