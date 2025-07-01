using System;
using Cysharp.Threading.Tasks;
using SpatialLLM.Core;
using SpatialLLM.Device;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace SpatialLLM.Experiment
{
    public abstract class SystemExecutor : MonoBehaviour
    {
        [SerializeField] protected SAUIManager saUIManager;
        [SerializeField] protected ExperimentManager experimentManager;
        [SerializeField] protected ExperimentTask experimentTask;
        [SerializeField] protected WordLogger wordLogger;
        
        [SerializeField] protected UIButtonHelper uiButtonHelper;
    
        
        protected float elapsedTime = 0f;
        protected bool timerStarted = false;

    

        protected bool isStarted = false;
        protected UniTaskCompletionSource<bool> completionSource;

        public UnityEvent onBeginOperation;
        public UnityEvent onCompleteOperation;
        

        protected bool isDeviceOperated = false;
        protected bool deviceOperatable = false;



private float flickThreshold = -0.6f;
private float flickCooldownTime = 0.15f; // クールダウン時間（秒）
private float lastFlickTime = -999f;




        protected virtual void OnLeftThumbstickLeftFlick()
        {
            Debug.Log("<color=cyan>左スティックを左にフリック検知（基底クラス）</color>");
            // 子クラスでオーバーライドして使う
        }

         #if UNITY_EDITOR
        void FixedUpdate()
        {
             if (timerStarted)
            {
                elapsedTime += Time.fixedDeltaTime;
            }
                if (!deviceOperatable) return;
                bool isWhite = Input.GetKeyDown(KeyCode.W); 
                bool isBlue = Input.GetKeyDown(KeyCode.Q); 
                bool isRed = Input.GetKeyDown(KeyCode.E);
                if (Input.GetKeyDown(KeyCode.R))
                {
                    isDeviceOperated = true;
                }
                Color lightColor = Color.white; // デフォルトは白色
            if (Application.isPlaying && (isWhite || isBlue || isRed))
                {

                    if (isWhite)
                    {
                        lightColor = Color.white;
                    }
                    else if (isBlue)
                    {
                        lightColor = Color.blue;
                    }
                    else if (isRed)
                    {
                        lightColor = Color.red;
                    }

                
                    GameObject[] selectedObjects = Selection.gameObjects;

                    Debug.Log($"選択中のオブジェクト数: {selectedObjects.Length}");

                    foreach (var obj in selectedObjects)
                    {
                        // 自身にSADeviceがあるか
                        SADevice saDevice = obj.GetComponent<SADevice>();

                        // なければ親をたどる（GetComponentInParent で親も含めて検索）
                        if (saDevice == null)
                        {
                            saDevice = obj.GetComponentInParent<SADevice>();
                        }

                        if (saDevice != null)
                        {

                            saDevice.TurnOnWithColor(lightColor);
                            saDevice.GetComponent<DrawOnHover>()?.VisualizeTargetDevice(Color.blue);
                        }
                    }
                }
        }
    #endif

        public virtual void BeginOperation()
        {
           this.experimentTask.InitExperimentTask(experimentManager.GetCurrentTaskId());
            onBeginOperation?.Invoke();
            isStarted = true;
            elapsedTime = 0f;
            timerStarted = true;
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


        public void ExietExecutor()
        { 
            isStarted = false;
        }


        public virtual void CompleteOperation()
        {
            onCompleteOperation?.Invoke();
            completionSource?.TrySetResult(isStarted);
            isStarted = false;
        }

        protected virtual void Start(){}

        // 継承先で操作ロジックを書く
        protected virtual void Update()
        {

                     Vector2 thumbstick = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);

                // 時間経過でフリック解除（押し戻し不要）
                if (Time.time - lastFlickTime > flickCooldownTime &&
                    thumbstick.x < flickThreshold &&
                    Mathf.Abs(thumbstick.y) < 0.4f)
                {
                    lastFlickTime = Time.time;
                    OnLeftThumbstickLeftFlick();
                }

            
        }
    }
}