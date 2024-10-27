using System;
using ActionDataTypes;



namespace ActionDataTypes.Logic
{


    public record TimerBlockData : ActionBlock
    {
        public int waitTime {get; set;}

        public TimerBlockData(Guid id, string name, string description, string type, int waitTime) : base(id, name, description, type)
        {
            this.waitTime = waitTime;
        }
    }


}