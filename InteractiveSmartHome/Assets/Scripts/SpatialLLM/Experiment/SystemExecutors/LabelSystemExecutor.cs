using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SpatialLLM.Core;
using SpatialLLM.Device;
using UnityEngine;
using UnityEngine.Events;




namespace SpatialLLM.Experiment{
public class LabelSystemExecutor : SystemExecutor
{
        
        [SerializeField] private ExperimentManager experimentManager;

        void Update()
        {
            if(OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
            {
                foreach(SADevice saDevice in experimentManager.GetAllDevices())
                {
                    
                }
            }
        }
    }
}