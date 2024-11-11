import TestBlock from "@block/test/TestBlock";
import { MongoDB } from "@/database-connector/mongodb";
import {
  IGateLogicBlock,
  INotGateLogicBlock,
  IRoutineData,
  ISignalData,
  IScheduleLogicBlock,
} from "@/types/ActionBlockInterfaces";
import GateLogicBlock from "@/actions/action-blocks/logic/GateLogicBlock";
import { ActionType } from "@/types/ActionType";
import { test } from "./routine-test-again";
import {
  NotGateLogicBlock,
  ScheduleLogicBlock,
} from "@/actions/action-blocks/logic";
import { Action } from "rxjs/internal/scheduler/Action";

async function main() {
  const testData: ISignalData = {
    action_id: "test",
    data_type: "boolean",
    value: true,
  };
  const testData2: ISignalData = {
    action_id: "test2",
    data_type: "string",
    value: "TEST",
  };
  const testData3: ISignalData = {
    action_id: "test3",
    data_type: "boolean",
    value: false,
  };

  const gateData: IGateLogicBlock = {
    logic_operator: "AND",
    id: "gate-sample",
    name: "gate-sample",
    description: "gate",
    action_type: ActionType.Logic_Gate,
  };

  const notGate: INotGateLogicBlock = {
    logic_operator: "NOT",
    id: "notGate-sample",
    name: "notGate-sample",
    description: "notGate",
    action_type: ActionType.Logic_NotGate,
  };

  const routine: IRoutineData = {
    id: "schedule-test-routine",
    name: "gate-test",
    action_routine: [
      {
        first: true,
        current_block_id: "shcedule-test",
        next_block_id: "light-1",
      },
      {
        last: true,
        current_block_id: "light-1",
        next_block_id: "",
      },
    ],
  };

  console.log("Starting Schedule");
  const scheduleData: IScheduleLogicBlock = {
    cronExpression: "* * * * * *",
    id: "shcedule-test",
    name: "schedule-test",
    description: "shcedule",
    action_type: ActionType.Logic_Schedule,
  };

  // const scheduleBlock = new ScheduleLogicBlock(scheduleData);
  // const testBlock = new TestBlock(testData);
  // scheduleBlock.setNextActionBlock([testBlock]);
  // scheduleBlock.startAction();

  console.log(JSON.stringify(routine, null, 2));
}

main();
