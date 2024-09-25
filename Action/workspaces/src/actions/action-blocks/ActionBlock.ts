import { ActionEvents, ActionEventKey } from '../ActionEvents';
import { ActionBlockType } from '../../types/ActionBlockTypes';

export class ActionBlock {
  private action_id: string;
  private routine_id: string;

  private name: string;
  private description: string;

  constructor(routine_id: string, acitonBlock: ActionBlockType) {
    this.routine_id = routine_id;
    this.action_id = acitonBlock.id;
    this.name = acitonBlock.name;
    this.description = acitonBlock.description;
  }

  public getID(): string {
    return this.action_id;
  }

  protected startAction(): void {}

  protected waitAction(): void {}

  protected finishAction(): void {
    ActionEvents.GetInstance().emit(
      `${ActionEventKey.ExitAction}_${this.routine_id}`,
      this.action_id,
    );
  }
}
