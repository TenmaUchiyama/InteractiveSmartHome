import {
  ISimpleComparatorLogicBlock as ISimpleComparatorLogicBlock,
  ISignalData,
  Comparator,
} from "@/types/ActionBlockInterfaces";
import ActionBlock from "@block/ActionBlock";
import Debugger from "@debugger/Debugger";

export default class SimpleComparatorLogicBlock
  extends ActionBlock
  implements ISimpleComparatorLogicBlock
{
  comparator: Comparator;
  value: number;
  constructor(comparatorBlockInitializers: ISimpleComparatorLogicBlock) {
    super(comparatorBlockInitializers);

    this.comparator = comparatorBlockInitializers.comparator;
    this.value = Number(comparatorBlockInitializers.value);
    Debugger.getInstance().debugLogOneshot(
      this.getRoutineId(),
      "SIMPLE COMPARATOR",
      `Comparing Operator: ${JSON.stringify(comparatorBlockInitializers, null, 2)}, Comparing Value: ${this.value}`
    );
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    const isValid = this.compare(data);

    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "SIMPLE COMPARATOR",
      `Received data from previous block: ${data.value}, isValid: ${isValid}`
    );

    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "SIMPLE COMPARATOR",
      `Comparing Operator: ${this.comparator}, Comparing Value: ${this.value}`
    );

    let sendingData: ISignalData = {
      action_id: this.id,
      data_type: "boolean",
      value: isValid,
    };
    //比較の結果をNodeに知らせる
    super.SendSignalDataToNodeFlow(sendingData);

    this.startNextActionBlock();
    this.senderDataStream?.next(sendingData);
  }

  // 比較関数
  private compare(data: ISignalData): boolean {
    const dataValue = data.value as number; // 受信したデータの値を取得

    switch (this.comparator) {
      case Comparator.GREATER_THAN:
        return this.value > dataValue;
      case Comparator.LESS_THAN:
        return this.value < dataValue;
      case Comparator.EQUAL:
        return this.value === dataValue;
      case Comparator.NOT_EQUAL:
        return this.value !== dataValue;
      case Comparator.GREATER_THAN_OR_EQUAL:
        return this.value >= dataValue;
      case Comparator.LESS_THAN_OR_EQUAL:
        return this.value <= dataValue;
      default:
        return false;
    }
  }
}
