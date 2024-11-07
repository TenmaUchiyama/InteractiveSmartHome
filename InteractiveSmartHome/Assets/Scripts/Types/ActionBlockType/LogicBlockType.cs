using System;
using ActionDataTypes;



namespace ActionDataTypes.Logic
{


    public record TimerBlockData : IActionBlock
    {
        public float waitTime {get; set;}


         public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string type { get; set;}
        public string action_type { get; set;}
        public TimerBlockData(Guid id, string name, string description, float waitTime)
        {
            this.waitTime = waitTime;

            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_Timer);

        }
    }


    public record SimpleComparatorBlockData : IActionBlock
    {
        public string comparator {get; set;}
        public string value {get; set;}


         public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string type { get; set;}
        public string action_type { get; set;}


        public SimpleComparatorBlockData(Guid id, string name, string description, string type, string comparator, string value) 
        {
            this.comparator = comparator;
            this.value = value;


             this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_SimpleComparator);

        }
    }


}