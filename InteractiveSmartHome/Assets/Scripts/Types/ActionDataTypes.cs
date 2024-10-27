using System;
using System.Collections.Generic;
using System.Globalization;

namespace ActionDataTypes
{


    public enum ActionBlockType
    { 
        Block_Logic_Timer, 
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

    public static string GetActionType(ActionBlockType actionBlockType)
    {
        return actionTypeMap[actionBlockType];
    }

    public static string GetDeviceType(DeviceType deviceType)
    {
        return deviceTypeMap[deviceType];
    }


}
[Serializable]
public record Routine
{
    public bool? first { get; set; }
    public bool? last { get; set; }
    public Guid currentBlockId { get; set; }
    public Guid nextBlockId { get; set; }

    public Routine (Guid currentBlockId, Guid nextBlockId)
    {
        this.currentBlockId = currentBlockId;
        this.nextBlockId = nextBlockId;
    }
}
[Serializable]
public record RoutineData
{
    public Guid id { get; set; }
    public string name { get; set; }
    public List<Routine> actionRoutine { get; set; }
}





[Serializable]
public record ActionBlock 
{
    public Guid id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string action_type { get; set; }

    public ActionBlock(Guid id, string name, string description, string action_type)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.action_type = action_type;
    }
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