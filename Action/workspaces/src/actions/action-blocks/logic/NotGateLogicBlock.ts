import { ActionType } from '@/types/ActionType';
import {
  IRxData,
  IGateLogicBlock,
  INotGateLogicBlock,
} from '@/types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';
import Debugger from '@debugger/Debugger';

export default class GateLogicBlock
  extends ActionBlock
  implements INotGateLogicBlock
{
  logic_operator: 'NOT';

  constructor(gateLogicInit: INotGateLogicBlock) {
    super(gateLogicInit);
    this.logic_operator = gateLogicInit.logic_operator;
  }

  onReceiveDataFromPreviousBlock(data: IRxData): void {
    const value = data.value;
    const dataType = data.data_type;

    if (dataType === 'boolean') {
      const passToNextBlock = !value;
      data.action_id = this.id;
      data.value = passToNextBlock;

      this.startNextActionBlock();
      this.senderDataStream?.next(data);
    }
  }
}