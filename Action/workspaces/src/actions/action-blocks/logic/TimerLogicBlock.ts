import { ActionType } from '@/types/ActionType';
import { IRxData, ITimerLogicBlock } from '@/types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';
import Debugger from '@debugger/Debugger';
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

  onReceiveDataFromPreviousBlock(data: IRxData): void {
    setTimeout(() => {
      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        'TIMER',
        'pass the data to next block ' + data,
      );
      this.senderDataStream?.next(data);
    }, 2000);
  }
}
