using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpatialLLM.Device;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Collections;


namespace SpatialLLM.Experiment{




public enum ExperimentFlowState
{
    NONE, 
    START_TASK,
    SHOW_DEVICE, 
    MOVING_TO_POINT, 

    OPERATION, 
    DONE
}
public class ExperimentManager : MonoBehaviour
{


    private TaskGenerator taskGenerator;

    private List<TaskData> taskDataList;
    private TaskData currentTask;
    [SerializeField]private  int currentTaskIndex;
    [SerializeField] private SystemExecutor systemExecutor;
    [SerializeField] private GameObject parentObject; 
    

    private ExperimentalDataManager experimentalDataManager;
    private ExperimentFlowState currentState = ExperimentFlowState.NONE;
    private Stack<ExperimentFlowState> stateHistory = new Stack<ExperimentFlowState>(); 

    private Dictionary<ExperimentFlowState, Func<UniTask>> stateActions; 

    

    

    //とりあえずデバイスの色をつける

    private void Start() {
        experimentalDataManager = GetComponent<ExperimentalDataManager>();
        taskGenerator = GetComponent<TaskGenerator>();

        ReadTaskData();
        

        stateActions = new Dictionary<ExperimentFlowState, Func<UniTask>>
        {
            { ExperimentFlowState.START_TASK, StartTask },
            { ExperimentFlowState.SHOW_DEVICE, ShowDevice },
            { ExperimentFlowState.MOVING_TO_POINT, MoveToPoint },
            {ExperimentFlowState.OPERATION, Operation},
            { ExperimentFlowState.DONE, DoneState }
        };
        
    
        Init();
    }


    private async void Init()
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.A));
        TransitionToState(ExperimentFlowState.START_TASK);
    }



// START TASK STATE
    private async UniTask StartTask()
    { 

        this.currentTask = this.taskDataList[this.currentTaskIndex]; 
        TransitionToState(ExperimentFlowState.SHOW_DEVICE);
    }



// SHOW DEVICE STATE
    private async UniTask ShowDevice()
    {
        DebugLogging("Show Device");
        DisplayDevices(this.currentTask.devices);

        foreach(SADevice device in this.currentTask.devices)
        {
            DebugLogging($"Device: {device.gameObject.name}");
        }
        await UniTask.WaitUntil(() => OVRInput.GetDown(OVRInput.RawButton.Y));
        ClearDeviceVisual();

        TransitionToState(ExperimentFlowState.MOVING_TO_POINT);
    }

// MOVE TO POINT STATE
    private async UniTask MoveToPoint()
    {
       DebugLogging("Moving to Point...");
        await MoveToPosition.Instance.MoveCameraRigAsync();
        TransitionToState(ExperimentFlowState.OPERATION);
    }


// OPERATING STATE 
private async UniTask Operation() 
{
    DebugLogging($"[{this.currentState.ToString()}] Operating"); 
    systemExecutor.BeginOperation(); 
    experimentalDataManager.StartTaskTimer();
    await systemExecutor.WaitForExecution();
    experimentalDataManager.StopTaskTimer();
    TransitionToState(ExperimentFlowState.DONE);

}

 
// DONE STATE
    private async UniTask DoneState()
    {
        

        this.ClearDeviceVisual();

        currentTaskIndex++; 

        if(currentTaskIndex >= this.taskDataList.Count)
        {
            DebugLogging("$<color=yellow>All Process Done!</color>"); 
            return; 
        }
  
        TransitionToState(ExperimentFlowState.START_TASK);


    }




    public void InitDevicesState(List<SADevice> saDevices) 
    {
        foreach(SADevice saDevice in saDevices)
        {
            saDevice.Init();
        }
    }






    public void GoBackToPreviousState()
    {
        if (stateHistory.Count > 0)
        {
            ExperimentFlowState previousState = stateHistory.Pop();
            DebugLogging($"戻る: {previousState}");
            TransitionToState(previousState);
        }
    }

    private void DisplayDevices(List<SADevice> saDevices)
    {
        foreach(SADevice saDevice in saDevices)
        {
            DrawOnHover drawOnHover = saDevice.gameObject.GetComponent<DrawOnHover>();
            if (drawOnHover != null)
            {
                drawOnHover.VisualizeTargetDevice(Color.red); 
            }
        }
    }


    private void ClearDeviceVisual() 
    {
        foreach(Transform child in parentObject.transform)
        {
            SADevice device = child.GetComponent<SADevice>();
            if(device != null)
            {
                device.Init();
            }
        }
    }




    private void ReadTaskData() 
    {
       taskDataList = taskGenerator.ReadTaskData();
    }

    
     private async void TransitionToState(ExperimentFlowState nextState)
    {
        stateHistory.Push(currentState);
        currentState = nextState;
        if (stateActions.ContainsKey(nextState))
        {
            await stateActions[nextState]();
        }
    }

    private void DebugLogging(string message)
    {
        Debug.Log($"<color=green>[{this.currentState.ToString()}{this.currentTaskIndex}] {message}</color>");
    }


    public TaskData GetCurrentTaskData() 
    {
        return this.currentTask;
    }
    
    public string GetCurrentTaskId()
    {
        return this.currentTask.taskId; 
    }
}
}