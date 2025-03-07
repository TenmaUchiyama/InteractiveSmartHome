using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SpatialLLM.Core;
using UnityEngine;
using UnityEngine.Events;




namespace SpatialLLM.Experiment{
public class SystemExecutor : MonoBehaviour
{

    [SerializeField] protected SAUIManager saUIManager; 
    [SerializeField] protected ExperimentManager experimentManager;
    private UniTaskCompletionSource<bool> completionSource;


    protected bool isTriggarable =true;

    protected bool isRecording = false;


    protected bool isPointing = false;

    private bool isStarted = false;
    public UnityEvent onBeginOperation;
    public UnityEvent onCompleteOperation;


        protected virtual void Start()  
        {
            if(SASpeechRecognizer.Instance) SASpeechRecognizer.Instance.OnVoiceRecognized.AddListener(OnVoiceRecognized);
        }

        void OnDestroy()
        {
            if(SASpeechRecognizer.Instance) SASpeechRecognizer.Instance.OnVoiceRecognized.RemoveListener(OnVoiceRecognized);
        }

        private void OnVoiceRecognized(string recognizedText)
        {
            saUIManager.SetRecognizedTxt(recognizedText);

            if(!saUIManager.IsRecognizedWordEmplty())
            {
                saUIManager.SetInstructionText("Press Y to confirm");
            }
            
        }


        protected bool isOperationDone = false;
        public virtual void Update()
        {
            //For Debug
            // if(Input.GetKeyDown(KeyCode.Escape))
            // {
            //     CompleteOperation();
            // }
             if( OVRInput.GetDown(OVRInput.RawButton.Y))
                { 
                    
                    
                if(isOperationDone)    
                {
                    this.CompleteOperation();
                    isOperationDone = false;
                    saUIManager.ClearRecognizedWord();
                    return;
                }




                if(!saUIManager.IsRecognizedWordEmplty())
                {
                    if(isPointing) return;
                    experimentManager.DisplayCurrentOperation();
                    isOperationDone = true;
                }

                
                    
                }
           

            if(!isStarted) return;


              if(Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.RawButton.X))
              {
                experimentManager.BackToShowDevice();
              }


              if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    if(!isTriggarable)return ;
                
                    Debug.Log("[ControllerInput] Pressed");
                    isRecording = true;
                    SASpeechRecognizer.Instance.ActivateVoice();
             
                }


                if(OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
                {
                    if(!isTriggarable) return ;
                    Debug.Log("[ControllerInput] Released");
                    isRecording = false;
                    SASpeechRecognizer.Instance.DeactivateVoice();
               
                }
        }


        public virtual void BeginOperation(){
            onBeginOperation.Invoke();
            isStarted = true;

            saUIManager.SetInstructionText("Press Trigger to Record");
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
        public virtual void CompleteOperation()
        {   
            onCompleteOperation?.Invoke();
            completionSource?.TrySetResult(isStarted);
            isStarted = false;
        }
}
}