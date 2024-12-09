import { ActionType } from "@/types/ActionType";
import { ISignalData, IGateLogicBlock } from "@/types/ActionBlockInterfaces";
import ActionBlock from "../ActionBlock";
import Debugger from "@debugger/Debugger";

export default class GateLogicBlock
  extends ActionBlock
  implements IGateLogicBlock
{
  logic_operator: "AND" | "OR";

  state_map: Map<string, boolean> = new Map<string, boolean>();

  constructor(gateLogicInit: IGateLogicBlock) {
    super(gateLogicInit);
    this.logic_operator = gateLogicInit.logic_operator;
  }

  setReceiverDataStream(action_id: string, dataStream: any): void {
    super.setReceiverDataStream(action_id, dataStream);
    this.state_map.set(action_id, false);
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    const value = data.value;
    const actionId = data.action_id;
    const dataType = data.data_type;

    if (dataType === "boolean") {
      this.state_map.set(actionId, value as boolean);
    }
    const gateLogicResult = this.checkGateLogic();

    console.log("===============================");
    this.state_map.forEach((value, key) => {
      Debugger.getInstance().debugLog(
        this.routineId,
        "GATE LOGIC",
        "Received data: " + key + " : " + value
      );
    });
    console.log("===============================");
    let nextData: ISignalData = {
      action_id: this.id,
      data_type: "boolean",
      value: gateLogicResult,
    };

    super.SendSignalDataToNodeFlow(nextData);
    data.action_id = this.id;
    this.startNextActionBlock();
    this.senderDataStream?.next(nextData);
  }

  checkGateLogic(): boolean {
    let okCount: number = 0;
    switch (this.logic_operator) {
      case "AND":
        let allTrue = true;
        this.state_map.forEach((value, key) => {
          allTrue = allTrue && value;
          if (value) okCount++;
        });

        let sendSignal: ISignalData = {
          action_id: this.id,
          data_type: "number",
          value: okCount,
        };

        super.SendSignalDataToNodeFlow(sendSignal);
        return allTrue;
      case "OR":
        let oneTrue = false;
        this.state_map.forEach((value, key) => {
          oneTrue = oneTrue || value;
          if (value) okCount++;
        });
        let sendSignalOr: ISignalData = {
          action_id: this.id,
          data_type: "number",
          value: okCount,
        };

        super.SendSignalDataToNodeFlow(sendSignalOr);
        return oneTrue;
    }
  }
}
