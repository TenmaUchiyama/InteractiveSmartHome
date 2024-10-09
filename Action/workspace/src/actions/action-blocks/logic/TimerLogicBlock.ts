import { ActionType } from '@/types/ActionType';
import { IRxData, ITimerLogicBlock } from '@/types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';
import Debugger from '@debugger/Debugger';
export default class TimerLogicBlock
  extends ActionBlock
  implements ITimerLogicBlock
{
  waitTime: number;
  private timerId: NodeJS.Timeout | null = null;

  constructor(timerBlockInitializers: ITimerLogicBlock) {
    super(timerBlockInitializers);
    this.waitTime = timerBlockInitializers.waitTime;
    this.action_type = ActionType.Logic_Timer;
  }

  exitAction(): void {
    if (this.timerId) {
      clearTimeout(this.timerId);
      this.timerId = null;
    }

    super.exitAction();
  }

  onReceiveDataFromPreviousBlock(data: IRxData): void {
    this.timerId = setTimeout(() => {
      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        'TIMER',
        'pass the data to next block ' + data,
      );
      data.action_id = this.id;
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
    }, this.waitTime * 1000);
  }
}
