import { TimerLogicType } from '../../../types/ActionBlockTypes';
import { ActionBlock } from '../ActionBlock';
import { LogicAction } from './LogicAction';
import { ActionBlockType } from '../../../types/ActionBlockTypes';

export class TimerLogicAction extends LogicAction {
  private time: number;

  constructor(routine_id: string, timerData: TimerLogicType) {
    let actionBlock = timerData as ActionBlockType;
    super(routine_id, actionBlock);

    this.time = parseInt(timerData.time);
  }
  protected mainLogic(): void {
    setTimeout(() => {
      console.log(`Timer Logic ${this.time}s`);
    }, this.time);
    this.finish();
  }
}
