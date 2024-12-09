using System;
using System.Collections.Generic;
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



   public static class SimpleComparatorUtil
    {
          public enum ComparatorType
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public static ComparatorType GetSimpleComparatorType(string comparator)
    {
        switch (comparator)
        {
            case "=":
                return ComparatorType.Equal;
            case "!=":
                return ComparatorType.NotEqual;
            case ">":
                return ComparatorType.GreaterThan;
            case "<":
                return ComparatorType.LessThan;
            case ">=":
                return ComparatorType.GreaterThanOrEqual;
            case "<=":
                return ComparatorType.LessThanOrEqual;
            default:
                return ComparatorType.Equal;
        }
    }
    public static string GetSimpleComparatorSymbol(ComparatorType comparatorType)
    {
        switch (comparatorType)
        {
            case ComparatorType.Equal:
                return "=";
            case ComparatorType.NotEqual:
                return "!=";
            case ComparatorType.GreaterThan:
                return ">";
            case ComparatorType.LessThan:
                return "<";
            case ComparatorType.GreaterThanOrEqual:
                return ">=";
            case ComparatorType.LessThanOrEqual:
                return "<=";
            default:
                return "=";
        }
    }
    }
  
    public record SimpleComparatorBlockData : IActionBlock
    {
        public string comparator {get; set;}
        public float value {get; set;}


         public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
    
        public string action_type { get; set;}


        public SimpleComparatorBlockData(Guid id, string name, string description, string comparator, float value) 
        {
            this.comparator = comparator;
            this.value = value;


             this.id = id;
            this.name = name;
            this.description = description;
   
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_SimpleComparator);

        }
    }


    public static class RangeComparatorUtil
    {
        public enum ComparatorType
        {
            InsideRange,
            OutsideRange
        }


        public  class RangeComparatorTempData
        {
            public string operatorFrom {get; set;}
            public string operatorTo {get; set;}
            public float from {get; set;}
            public float to {get; set;}
        }

        public static RangeComparatorTempData GetRangeComparatorData(ComparatorType comparatorType, float from, float to)
        {
            switch (comparatorType)
            {
                case ComparatorType.InsideRange:
                    return new RangeComparatorTempData{
                        operatorFrom = ">=",
                        operatorTo = "<=",
                        from = from,
                        to = to
                    };
                case ComparatorType.OutsideRange:
                    return new RangeComparatorTempData{
                        operatorFrom = "<=",
                        operatorTo = ">=",
                        from = from,
                        to = to
                    };
                default:
                    return new RangeComparatorTempData{
                        operatorFrom = ">=",
                        operatorTo = "<=",
                        from = from,
                        to = to
                    };
            }
        }



        public static ComparatorType GetComparatorType(string operatorFrom, string operatorTo)
        {
                    if (operatorFrom == ">=" && operatorTo == "<=")
                    {
                        return ComparatorType.InsideRange;
                    }
                    else if (operatorFrom == "<=" && operatorTo == ">=")
                    {
                        return ComparatorType.OutsideRange;
                    }else{
                        return ComparatorType.InsideRange;
                    }
            
        }


    }


    public record RangeComparatorBlockData : IActionBlock
    {
        public string operatorFrom {get; set;}
        public string operatorTo {get; set;}
        public float from {get; set;}
        public float to {get; set;}

        public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string action_type { get; set;}

        public RangeComparatorBlockData(Guid id, string name, string description, string operatorFrom, string operatorTo, float from, float to) 
        {
            this.operatorFrom = operatorFrom;
            this.operatorTo = operatorTo;
            this.from = from;
            this.to = to;

            this.id = id;
            this.name = name;
            this.description = description;
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_RangeComparator);
        }
        
          
    }
        public static class GateLogicUtil 
    {
        public enum GateLogicType
        {
            AND,
            OR
        }

        public static GateLogicType GetGateLogicType(string comparator)
        {
            switch (comparator)
            {
                case "AND":
                    return GateLogicType.AND;
                case "OR":
                    return GateLogicType.OR;
                default:
                    return GateLogicType.AND;
            }
        }

        public static string GetGateLogicSymbol(GateLogicType comparatorType)
        {
            switch (comparatorType)
            {
                case GateLogicType.AND:
                    return "AND";
                case GateLogicType.OR:
                    return "OR";
                default:
                    return "AND";
            }
        }
    }



    public record GateLogicBlockData : IActionBlock
    {
        public string logic_operator {get; set;}
            
            public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string action_type { get; set;}



        public GateLogicBlockData(Guid id, string name, string description, string logic_operator) 
        {
            this.logic_operator = logic_operator;
            this.id = id;
            this.name = name;
            this.description = description;
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_Gate);
        }

     
    }



    public record NotGateLogicBlockData : IActionBlock
    {

        public string logic_operator;
        public Guid id { get; set;}
        public string name { get; set;}
        public string description { get; set;}
        public string action_type { get; set;}



        public NotGateLogicBlockData(Guid id, string name, string description) 
        {
            
            this.id = id;
            this.name = name;
            this.description = description;
            this.logic_operator = "NOT";
            this.action_type = BlockActionTypeMap.GetActionTypeString(ActionBlockType.Logic_NotGate);
        }

    }
}