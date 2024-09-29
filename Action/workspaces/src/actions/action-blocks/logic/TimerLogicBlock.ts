import { ActionType } from '../../../types/ActionType';
import { ITimerLogicBlock } from '../../../types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';

export default class TimerLogicBlock
  extends ActionBlock
  implements ITimerLogicBlock
{
  waitTime: number;
  constructor(timerBlockInitializers: ITimerLogicBlock) {
    super(timerBlockInitializers);
    this.waitTime = timerBlockInitializers.waitTime;
    this.action_type = ActionType.Logic_Timer;
  }

  onReceiveDataFromPreviousBlock(data: any): void {
    setTimeout(() => {
      console.log('[TIMER] pass the data to next block', data);
      this.senderDataStream?.next(data);
    }, 2000);
  }
}
