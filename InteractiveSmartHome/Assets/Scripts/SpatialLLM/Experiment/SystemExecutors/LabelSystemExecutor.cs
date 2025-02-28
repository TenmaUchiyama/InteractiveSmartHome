using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using SpatialLLM.Core;
using SpatialLLM.Device;
using UnityEngine;
using UnityEngine.Events;




namespace SpatialLLM.Experiment{
public class LabelSystemExecutor : SystemExecutor
{
        
        [SerializeField] private ExperimentManager experimentManager;

        private void Update()
        {
            base.Update();


            if(isRecording) return ;
            if(OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
            {

                this.isTriggarable = false;
                foreach(SADevice saDevice in experimentManager.GetAllDevices())
                {
                    saDevice.DisplayShowLabel(true);
                }
            }


            if(OVRInput.GetUp(OVRInput.RawButton.LHandTrigger))
            {

                this.isTriggarable = true;
                foreach(SADevice saDevice in experimentManager.GetAllDevices())
                {
                    saDevice.DisplayShowLabel(false);
                }
            }
        }
    }
}