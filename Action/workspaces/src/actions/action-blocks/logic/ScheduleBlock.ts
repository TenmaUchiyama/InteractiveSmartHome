import { ActionType } from '@/types/ActionType';
import { IRxData, IScheduleLogicBlock } from '@/types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';
import Debugger from '@debugger/Debugger';
import cron, { ScheduledTask } from 'node-cron';

export default class ScheduleLogicBlock
  extends ActionBlock
  implements IScheduleLogicBlock
{
  cronExpression: string;
  scheduledTask: ScheduledTask | null = null;

  constructor(scheduleBlockInitializers: IScheduleLogicBlock) {
    super(scheduleBlockInitializers);
    this.cronExpression = scheduleBlockInitializers.cronExpression;
    this.action_type = ActionType.Logic_Schedule;
  }

  startAction(): void {
    super.startAction();
    this.scheduledTask = cron.schedule(this.cronExpression, () => {
      const data: IRxData = {
        action_id: this.id,
        data_type: 'trigger',
        value: null,
      };
      this.senderDataStream?.next(data);
    });

    Debugger.getInstance().debugLog(
      this.routineId,
      'SCHEDULE',
      `Schedule started with cron expression: ${this.cronExpression}`,
    );
  }

  exitAction(): void {
    if (this.scheduledTask) {
      this.scheduledTask.stop();
    }
  }
}
