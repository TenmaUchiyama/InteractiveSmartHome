import {
  IActionBlock,
  IRoutineData,
  ISignalData,
} from "@/types/ActionBlockInterfaces";
import ActionBlock from "../ActionBlock";
import * as readline from "readline";
import { ActionType } from "@/types/ActionType";
import Debugger from "@/debugger/Debugger";

export default class TestBlock extends ActionBlock {
  data: ISignalData;
  constructor(testInit: ISignalData) {
    super({
      id: "test-id",
      name: "TestBlock",
      description: "TestBlock",
      action_type: ActionType.Test,
    });

    this.data = {
      action_id: "test_id",
      data_type: "boolean",
      value: true,
    };
  }

  startAction() {
    super.startAction();

    console.log(
      "[TEST BLOCK]: Starting test block and sending data",
      this.data
    );

    this.startNextActionBlock();
    this.senderDataStream?.next(this.data);
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    console.log("Received data from previous block:", data);
  }
}
