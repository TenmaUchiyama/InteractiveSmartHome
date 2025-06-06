using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Core;
using SpatialLLM.Network;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SpatialAwarnessProvider))]
public class SpatialAwarnessProviderHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpatialAwarnessProvider spatialAwarnessProvider = (SpatialAwarnessProvider)target;

        if (GUILayout.Button("TEST_FURNITURE"))
        {
            spatialAwarnessProvider.TEST_FURNITURE();
        }


        if (GUILayout.Button("TEST FOV DEVICE"))
        {
            FOVRequest request = new FOVRequest();
            request.isInFov = true;
            request.order = "proximity";

            spatialAwarnessProvider.GetDevicesInFov("Light", request);
        }
        

          if (GUILayout.Button("TEST DIRECTION DEVICE"))
        {
            DirectionRequest request = new DirectionRequest();
            request.direction = "Front";
            request.order = "proximity";

            spatialAwarnessProvider.GetDevicesInDirection("Light", request);
        }
    }
}
