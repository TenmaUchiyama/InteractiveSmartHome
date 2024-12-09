using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ActionDataTypes.Device;
using ActionDataTypes.Logic;
using MRFlow.Component;

namespace ActionDataTypes
{


    public enum ActionBlockType
    { 
        Logic_Timer,
        Logic_SimpleComparator,
        Logic_RangeComparator,
        Logic_Gate,
        Logic_NotGate,
        Logic_Schedule,
        Device,
    }
    public enum DeviceType
    {
        Light,
        Thermometer,
        ToggleButton,
        Scheduler
    }

public class BlockActionTypeMap 
{
    private static Dictionary<ActionBlockType, string> actionTypeMap = new Dictionary<ActionBlockType, string>
    {
        {ActionBlockType.Logic_Timer, "logic-timer"},
        {ActionBlockType.Logic_SimpleComparator, "logic-simple-comparator"},
        {ActionBlockType.Logic_RangeComparator, "logic-range-comparator"},
        {ActionBlockType.Logic_Gate, "logic-gate"},
        {ActionBlockType.Logic_NotGate, "logic-not-gate"},
        {ActionBlockType.Logic_Schedule, "logic-schedule"},
        {ActionBlockType.Device, "device"}
    };




    private static Dictionary<DeviceType, string> deviceTypeMap = new Dictionary<DeviceType, string>
    {
        {DeviceType.Light, "actuator-light"},
        {DeviceType.Thermometer, "sensor-thermometer"},
        {DeviceType.ToggleButton, "sensor-toggle-button"},
        {DeviceType.Scheduler, "api-scheduler"}
    };


   private static readonly Dictionary<ActionBlockType, Type> actionDataMap = new Dictionary<ActionBlockType, Type>
{
    { ActionBlockType.Logic_Timer , typeof(TimerBlockData)},
    { ActionBlockType.Device , typeof(DeviceBlockData)},
    { ActionBlockType.Logic_SimpleComparator, typeof(SimpleComparatorBlockData)},
    { ActionBlockType.Logic_Gate, typeof(GateLogicBlockData)},   
    { ActionBlockType.Logic_NotGate, typeof(NotGateLogicBlockData)},
    { ActionBlockType.Logic_RangeComparator, typeof(RangeComparatorBlockData)},
    
};


    public static string GetActionTypeString(ActionBlockType actionBlockType)
    {
        return actionTypeMap[actionBlockType];
    }
    
    public static ActionBlockType GetStringTypeFromActionType(string actionType)
    {
        return actionTypeMap.FirstOrDefault(x => x.Value == actionType).Key;
    }

    public static string GetDeviceTypeString(DeviceType deviceType)
    {
        return deviceTypeMap[deviceType];
    }

      public static Type GetActionDataType(ActionBlockType actionBlockType)
    {
        if (actionDataMap.TryGetValue(actionBlockType, out var dataType))
        {
            return dataType;
        }

        return null;
    }




}
[Serializable]
public record Routine
{
    public bool? first { get; set; }
    public bool? last { get; set; }
    public Guid current_block_id { get; set; }
    public Guid next_block_id { get; set; }

    public Routine (Guid currentBlockId, Guid nextBlockId)
    {
        this.current_block_id = currentBlockId;
        this.next_block_id = nextBlockId;
    }
}
[Serializable]
public record RoutineData
{
    public Guid id { get; set; }
    public string name { get; set; }
    public List<Routine> action_routine { get; set; }

    public RoutineData(Guid id, string name, List<Routine> actionRoutine)
    {
        this.id = id;
        this.name = name;
        this.action_routine = actionRoutine;
    }
}





public interface IActionBlock 
{
    public Guid id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string action_type { get; set; }

 
}




[Serializable]
public record DeviceData
{
    public Guid device_id { get; set; }
    public string device_name { get; set; }
    public string device_type { get; set; }
    public string mqtt_topic { get; set; }  
    public DevicePosition device_position { get; set; }  
}


[Serializable]
public record DevicePosition
{
    public float x { get; set; }
    public float y { get; set; }
}





}