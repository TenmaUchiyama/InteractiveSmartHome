using System;
using System.Collections.Generic;
using System.Globalization;

namespace ActionDataTypes
{

[Serializable]
public class Routine
{
    public bool? first { get; set; }
    public bool? last { get; set; }
    public string currentBlockId { get; set; }
    public string nextBlockId { get; set; }
}
[Serializable]
public class RoutineData
{
    public string id { get; set; }
    public string name { get; set; }
    public List<Routine> actionRoutine { get; set; }
}





[Serializable]
public class ActionBlock 
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string type { get; set; }

}


[Serializable]
public class DeviceData
{
    public string device_id { get; set; }
    public string device_name { get; set; }
    public string device_type { get; set; }
    public string mqtt_topic { get; set; }  
    public DevicePosition device_position { get; set; }  
}


[Serializable]
public class DevicePosition
{
    public float x { get; set; }
    public float y { get; set; }

}





}