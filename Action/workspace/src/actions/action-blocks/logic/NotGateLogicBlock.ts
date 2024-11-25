import { ActionType } from "@/types/ActionType";
import {
  ISignalData,
  IGateLogicBlock,
  INotGateLogicBlock,
} from "@/types/ActionBlockInterfaces";
import ActionBlock from "../ActionBlock";
import Debugger from "@debugger/Debugger";

export default class GateLogicBlock
  extends ActionBlock
  implements INotGateLogicBlock
{
  logic_operator: "NOT";

  constructor(gateLogicInit: INotGateLogicBlock) {
    super(gateLogicInit);
    this.logic_operator = gateLogicInit.logic_operator;
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    Debugger.getInstance().debugLog(
      this.routineId,
      "NOT LOGIC",
      "Received data: " + JSON.stringify(data)
    );

    const value = data.value;
    const dataType = data.data_type;

    if (dataType === "boolean") {
      const passToNextBlock = !value;

      data.action_id = this.id;
      data.value = passToNextBlock;

      let outputData: ISignalData = {
        action_id: this.id,
        data_type: "boolean",
        value: passToNextBlock,
      };
      Debugger.getInstance().debugLog(
        this.routineId,
        "NOT LOGIC",
        "Sending Data: " + JSON.stringify(outputData)
      );

      super.SendSignalDataToNodeFlow(outputData);
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
    }
  }
}
