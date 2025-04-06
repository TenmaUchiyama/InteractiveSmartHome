using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Oculus.Interaction;
using SpatialLLM.Core;
using UnityEngine;
using UnityEngine.Events;



namespace SpatialLLM.Experiment{
public class PointerSystemExecutor : SystemExecutor
    {
        [SerializeField] private RayInteractor rayInteractor;
        [SerializeField ]private HandPointing pointing;

        private bool isMovable = false;

        private void DisablePointing() 
        {
            this.pointing.gameObject.SetActive(false);
            this.rayInteractor.gameObject.SetActive(false);
        }

        private void EnablePointing() 
        {
            this.pointing.gameObject.SetActive(true); 
            this.rayInteractor.gameObject.SetActive(false);
        }

        protected override void Start()
        {
            // Pointer 用は最初に rayInteractor を非アクティブにしておく
            DisablePointing();
            this.isPointing = true;
            base.Start();
        }

        public override void BeginOperation()
        {
            base.BeginOperation();
            EnablePointing();
        }

        public override void CompleteOperation()
        {
             DisablePointing();
            base.CompleteOperation();
        }


        public void PointSystemDone()
        {
            this.isPointing = false; 
            this.saUIManager.SetInstructionText("Press Y to proceed to the next step");

        }
        void Update()
        {

            base.Update();
                if( OVRInput.GetDown(OVRInput.RawButton.Y))
                {



                    if(isMovable)
                    {
                        this.CompleteOperation(); 
                        
                        saUIManager.ClearRecognizedWord();
                        isOperationDone = true;
                        
                    }
                }

                if(OVRInput.GetDown(OVRInput.RawButton.X))
                {
                    isMovable = true; 

                     this.saUIManager.SetInstructionText("Press Y to proceed to the next step");
                }

        }
    }
}