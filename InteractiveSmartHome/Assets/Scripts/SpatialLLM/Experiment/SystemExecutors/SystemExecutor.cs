using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialLLM.Experiment
{
    public abstract class SystemExecutor : MonoBehaviour
    {
        [SerializeField] protected SAUIManager saUIManager;
        [SerializeField] protected ExperimentManager experimentManager;

        protected bool isStarted = false;
        protected UniTaskCompletionSource<bool> completionSource;

        public UnityEvent onBeginOperation;
        public UnityEvent onCompleteOperation;

        public virtual void BeginOperation()
        {
            onBeginOperation?.Invoke();
            isStarted = true;
            saUIManager.SetInstructionText("Press Trigger to Record");
            Debug.Log("Operation Begun");
        }

        public virtual async UniTask WaitForExecution()
        {
            Debug.Log($"{this.GetType().Name} の WaitForExecution を開始...");
            completionSource = new UniTaskCompletionSource<bool>();
            await completionSource.Task;
            Debug.Log($"{this.GetType().Name} の WaitForExecution が完了");
        }

        public virtual void CompleteOperation()
        {
            onCompleteOperation?.Invoke();
            completionSource?.TrySetResult(isStarted);
            isStarted = false;
        }

        protected virtual void Start(){}

        // 継承先で操作ロジックを書く
        protected virtual void Update(){}
    }
}