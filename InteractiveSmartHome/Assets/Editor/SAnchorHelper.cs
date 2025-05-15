using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(SpatialAnchorEditor))]
public class SAnchorHelper : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector(); 

        SpatialAnchorEditor sacnhor = (SpatialAnchorEditor)target;

        if (GUILayout.Button("TEST"))
        {
            
            sacnhor.TestSpawn();
        }


    
    }
}
