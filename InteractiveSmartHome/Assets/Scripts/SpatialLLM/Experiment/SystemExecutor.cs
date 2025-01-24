using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;




namespace SpatialLLM.Experiment{
public class SystemExecutor : MonoBehaviour
{

    private UniTaskCompletionSource<bool> completionSource;


    private bool isStarted = false;
    public UnityEvent onBeginOperation;
    public UnityEvent onCompleteOperation;


        public void BeginOperation(){
            onBeginOperation.Invoke();
            isStarted = true;
        }

        public virtual async UniTask WaitForExecution()
        {
            Debug.Log($"{this.GetType().Name} の WaitForExecution を開始...");
            completionSource = new UniTaskCompletionSource<bool>();

            // `CompleteExecution()` が呼ばれるまで待機
            await completionSource.Task;

            Debug.Log($"{this.GetType().Name} の WaitForExecution が完了");
        }

        // サブクラスから完了を通知するメソッド
        public void CompleteOperation()
        {   
            onCompleteOperation?.Invoke();
            completionSource?.TrySetResult(isStarted);
            isStarted = false;
        }
}
}