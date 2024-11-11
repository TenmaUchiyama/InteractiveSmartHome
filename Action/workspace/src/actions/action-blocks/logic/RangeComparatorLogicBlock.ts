import {
  IRangeComparatorLogicBlock,
  ISignalData,
} from "@/types/ActionBlockInterfaces";
import { ActionType } from "@/types/ActionType";
import ActionBlock from "@block/ActionBlock";
import Debugger from "@debugger/Debugger";

export default class RangeComparatorLogicBlock
  extends ActionBlock
  implements IRangeComparatorLogicBlock
{
  operatorFrom: ">" | "<" | ">=" | "<=";
  operatorTo: ">" | "<" | ">=" | "<=";
  from: number;
  to: number;

  constructor(comparatorBlockInitializers: IRangeComparatorLogicBlock) {
    super(comparatorBlockInitializers);

    this.operatorFrom = comparatorBlockInitializers.operatorFrom;
    this.operatorTo = comparatorBlockInitializers.operatorTo;
    this.from = comparatorBlockInitializers.from;
    this.to = comparatorBlockInitializers.to;
  }

  onReceiveDataFromPreviousBlock(data: any): void {
    const isValid = this.compare(data);
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "RANGE COMPARATOR",
      `Received data from previous block: ${data.value}, isValid: ${isValid}`
    );
    if (isValid) {
      this.startNextActionBlock();
      this.senderDataStream?.next({
        action_id: this.id,
        data_type: "boolean",
        value: true,
      });
    }
  }

  // 比較関数
  private compare(data: any): boolean {
    const dataValue = data.value; // 受信したデータの値を取得

    const isValidFrom = this.evaluate(dataValue, this.from, this.operatorFrom);
    const isValidTo = this.evaluate(dataValue, this.to, this.operatorTo);

    return isValidFrom && isValidTo;
  }

  // 演算子に基づいて比較を行うヘルパー関数
  private evaluate(
    value: number,
    boundary: number,
    operator: ">" | "<" | "=" | "!=" | ">=" | "<="
  ): boolean {
    switch (operator) {
      case ">":
        return value > boundary;
      case "<":
        return value < boundary;
      case "=":
        return value === boundary;
      case "!=":
        return value !== boundary;
      case ">=":
        return value >= boundary;
      case "<=":
        return value <= boundary;
      default:
        return false;
    }
  }
}
