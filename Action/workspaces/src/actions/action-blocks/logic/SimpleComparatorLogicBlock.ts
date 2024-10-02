import {
  ISimpleComparatorLogicBlock as ISimpleComparatorLogicBlock,
  IRxData,
} from '@/types/ActionBlockInterfaces';
import ActionBlock from '@block/ActionBlock';
import Debugger from '@debugger/Debugger';

export default class SimpleComparatorLogicBlock
  extends ActionBlock
  implements ISimpleComparatorLogicBlock
{
  operator: '>' | '<' | '=' | '!=' | '>=' | '<=';
  value: number;
  constructor(comparatorBlockInitializers: ISimpleComparatorLogicBlock) {
    super(comparatorBlockInitializers);

    this.operator = comparatorBlockInitializers.operator;
    this.value = comparatorBlockInitializers.value;
  }

  onReceiveDataFromPreviousBlock(data: IRxData): void {
    const isValid = this.compare(data);
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      'SIMPLE COMPARATOR',
      `Received data from previous block: ${data.value}, isValid: ${isValid}`,
    );

    if (isValid) {
      this.startNextActionBlock();

      this.senderDataStream?.next({
        data_type: 'boolean',
        value: true,
      });
    }
  }

  // 比較関数
  private compare(data: IRxData): boolean {
    const dataValue = data.value as number; // 受信したデータの値を取得

    switch (this.operator) {
      case '>':
        return dataValue > this.value;
      case '<':
        return dataValue < this.value;
      case '=':
        return dataValue === this.value;
      case '!=':
        return dataValue !== this.value;
      case '>=':
        return dataValue >= this.value;
      case '<=':
        return dataValue <= this.value;
      default:
        return false;
    }
  }
}
