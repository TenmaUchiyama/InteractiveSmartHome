using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SpatialLLM.Core;
using UnityEngine;
using UnityEngine.Events;




namespace SpatialLLM.Experiment{
public class SystemExecutor : MonoBehaviour
{

    private UniTaskCompletionSource<bool> completionSource;


    private bool isStarted = false;
    public UnityEvent onBeginOperation;
    public UnityEvent onCompleteOperation;


        void Start()
        {
            
        }



        void Update()
        {
            //For Debug
            // if(Input.GetKeyDown(KeyCode.Escape))
            // {
            //     CompleteOperation();
            // }

            if(!isStarted) return;


              if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {

                
                    Debug.Log("[ControllerInput] Pressed");
                    SASpeechRecognizer.Instance.ActivateVoice();
             
                }


                if(OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
                {
                    Debug.Log("[ControllerInput] Released");
                    SASpeechRecognizer.Instance.DeactivateVoice();
               
                }
        }


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