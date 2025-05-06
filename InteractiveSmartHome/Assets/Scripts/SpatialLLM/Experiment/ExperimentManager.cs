using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Meta.WitAi.Json;
using SpatialLLM.Device;
using UnityEngine;



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
    private DeviceArrangementGenerator arrangementGenerator;
    private List<DeviceArrangeData> arrangeDataList;
    private DeviceArrangeData currentArrange;
    
    
    [SerializeField]private  int currentArrangeIndex;
    [SerializeField] private SystemExecutor systemExecutor;

    [SerializeField] private SAUIManager saUIManager;

    [SerializeField] private int SEED;
    
    private ExperimentalDataManager experimentalDataManager;
    private ExperimentTask experimentTask;
    private int currentTaskId; 
    private ExperimentFlowState currentState = ExperimentFlowState.NONE;
    private Stack<ExperimentFlowState> stateHistory = new Stack<ExperimentFlowState>(); 
    private Dictionary<ExperimentFlowState, Func<UniTask>> stateActions; 

    public int CurrentArrangeIndex  => currentArrangeIndex;

    

    //とりあえずデバイスの色をつける

    private void Start() {
        experimentalDataManager = GetComponent<ExperimentalDataManager>();
        arrangementGenerator = GetComponent<DeviceArrangementGenerator>();
        experimentTask = GetComponent<ExperimentTask>();

    

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

        
        Debug.Log("[INIT] Press Space to Init");
        await UniTask.WaitUntil(() => {
            
            
             if(Input.GetKeyDown(KeyCode.Space))
             {
                Debug.Log("Space Pressed"); 
                return true;
             }
             return false;
             });


        TransitionToState(ExperimentFlowState.START_TASK);
    }

    private void SendAllDeviceDataToSocket() 
    {
        List<DeviceArrangeDataSerializable> data  = arrangementGenerator.GetAllSerializedDeviceArrangeData(); 
    }



// START TASK STATE
    private async UniTask StartTask()
    { 
        this.currentArrange = this.arrangeDataList[this.currentArrangeIndex]; 
        this.experimentTask.Initialize(this.currentArrange);
        TransitionToState(ExperimentFlowState.SHOW_DEVICE);
    }



// SHOW DEVICE STATE
    private async UniTask ShowDevice()
{
    DebugLogging("Show Device");

    // DeviceColorPair のリストから SADevice のリストに変換
    List<DeviceColorPair> deviceArrangeDatas = this.currentArrange.devices; 

    // DisplayDevices は void を返すので、await しない
    DisplayDevices(deviceArrangeDatas);

    foreach(DeviceColorPair pair in deviceArrangeDatas)
    {
        Debug.Log($"<color=yellow>paris {pair.device.name}</color>");
    }
    Debug.Log("<color=red>[ShowDevice] Press A to Go To Operation</color>");
    saUIManager.SetDeviceCountText(deviceArrangeDatas.Count().ToString());
    saUIManager.SetInstructionText("Press Y If You Are Ready"); 
<<<<<<< HEAD
    await UniTask.WaitUntil(() => OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.A));
=======
    await UniTask.WaitUntil(() => OVRInput.GetDown(OVRInput.RawButton.Y) || Input.GetKeyDown(KeyCode.Y));
>>>>>>> stack
    saUIManager.ClearInstruction();
    ClearDeviceVisual();

    TransitionToState(ExperimentFlowState.OPERATION);
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
    this.experimentTask.StartTaskTimer();
<<<<<<< HEAD
    // await systemExecutor.WaitForExecution()
=======
>>>>>>> stack
    await systemExecutor.WaitForExecution();
    saUIManager.ClearInstruction();
    this.experimentTask.StopTaskTimer();
    TransitionToState(ExperimentFlowState.DONE);

}

 
// DONE STATE
    private async UniTask DoneState()
    {
        

        this.ClearDeviceVisual();
   

        currentArrangeIndex++; 

        if(currentArrangeIndex >= this.arrangeDataList.Count)
        {
            DebugLogging("$<color=yellow>All Process Done!</color>"); 
            this.saUIManager.ClearUI();
            this.saUIManager.SetInstructionText("All Tasks Done!");

            return; 
        }
        await this.experimentalDataManager.WriteExperimentalDataAsync(this.experimentTask);
        this.experimentTask.ClearAllData();

        TransitionToState(ExperimentFlowState.START_TASK);
    }



    public void StartLLMResponse() 
    {
        this.experimentTask.StartLLMTimer(); 
    }

    public void  StopLLMResponse(string prompt)
    {
        this.experimentTask.StopLLMTimer();
        this.experimentTask.AddCurrentQueryAttempt(prompt, this.currentArrange);
    }

    public void BackToShowDevice() 
    {
        TransitionToState(ExperimentFlowState.SHOW_DEVICE);
    }



    public void InitDevicesState(List<SADevice> saDevices) 
    {
        List<SADevice> allDevices = SADeviceRef.Instance.GetAllDevices();
        foreach(SADevice saDevice in allDevices)
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

    private void DisplayDevices(List<DeviceColorPair> deviceColorPairs)
    {
        foreach(DeviceColorPair devicePair in this.currentArrange.devices)
        {
            SADevice saDevice = devicePair.device; 
            DrawOnHover drawOnHover = saDevice.gameObject.GetComponent<DrawOnHover>();
            saDevice.TurnOnWithColor(devicePair.GetUnityColor());
            if (drawOnHover != null)
            {
                drawOnHover.VisualizeTargetDevice(Color.red); 
            }
        }
    }


    public void DisplayCurrentOperation()
    {
        List<DeviceColorPair> deviceColorPairs =  this.currentArrange.devices; 

        foreach(DeviceColorPair devicePair in deviceColorPairs)
        {
            SADevice saDevice = devicePair.device; 
            DrawOnHover drawOnHover = saDevice.gameObject.GetComponent<DrawOnHover>();
            saDevice.TurnOnWithColor(devicePair.GetUnityColor());
            if (drawOnHover != null)
            {
                drawOnHover.VisualizeTargetDevice(Color.blue); 
            }
        }
    }




    private void ClearDeviceVisual() 
    {
        List<SADevice> allDevices = SADeviceRef.Instance.GetAllDevices();
        foreach(SADevice device in allDevices)
        {
            if(device != null)
            {
                device.Init();
            }
        }
    }




    private void ReadTaskData() 
    {
        Debug.Log($"{arrangementGenerator.ReadTaskData().Count}");
       arrangeDataList = arrangementGenerator.ReadTaskData();
       Shuffle(arrangeDataList, SEED);
       
    }


    private void Shuffle<T>(List<T> list, int seed)
{
    System.Random rng = new System.Random(seed);
    int n = list.Count;
    while (n > 1)
    {
        n--;
        int k = rng.Next(n + 1);
        (list[k], list[n]) = (list[n], list[k]);
    }
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
        Debug.Log($"<color=green>[{this.currentState.ToString()}_{this.currentArrangeIndex}] {message}</color>");
    }


    public DeviceArrangeData GetCurrentArrangeData() 
    {
        return this.currentArrange;
    }
    
    public string GetCurrentTaskId()
    {
       
        return this.currentArrange.device_arrange_id; 
    }
}
}