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

        protected override void Start()
        {
            // Pointer 用は最初に rayInteractor を非アクティブにしておく
            rayInteractor.gameObject.SetActive(false);
            base.Start();
        }

        public override void BeginOperation()
        {
            base.BeginOperation();
            rayInteractor.gameObject.SetActive(true);
        }

        public override void CompleteOperation()
        {
            rayInteractor.gameObject.SetActive(false);
            base.CompleteOperation();
        }
    }
}