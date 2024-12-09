using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialLLM.Core;
public class TestSearchDevice : MonoBehaviour
{
   [SerializeField] SpatialAwarnessProvider spatialAwarnessProvider; 



   private void Update() {
    if(Input.GetKeyDown(KeyCode.A))
    {
        List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(DirectionUtil.Direction.Front);
    }

    if(Input.GetKeyDown(KeyCode.B))
    {
        List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(DirectionUtil.Direction.Back);
    }


    if(Input.GetKeyDown(KeyCode.C))
    {
        List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(DirectionUtil.Direction.Up);
    }

    if(Input.GetKeyDown(KeyCode.D))
    {
        List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(DirectionUtil.Direction.Down);
    }

    if(Input.GetKeyDown(KeyCode.E))
    {
        List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(DirectionUtil.Direction.Right);
    }

    if(Input.GetKeyDown(KeyCode.F))
    {
        List<Device> devices = spatialAwarnessProvider.FindDeviceInDirection(DirectionUtil.Direction.Left);
    }
   }
}
