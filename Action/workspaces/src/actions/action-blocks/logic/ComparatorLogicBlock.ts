import { IComparatorLogicBlock } from '../../../types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';

export default class ComparatorLogicBlock
  extends ActionBlock
  implements IComparatorLogicBlock
{
  threshold: number;
  constructor(comparatorBlockInitializers: IComparatorLogicBlock) {
    super(comparatorBlockInitializers);
    this.threshold = comparatorBlockInitializers.threshold;
  }

  onReceiveDataFromPreviousBlock(data: any): void {
    if (data > this.threshold) {
      console.log('[COMPARATOR] Data is greater than threshold');
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
    }
  }
}
