import { ActionBlock } from '../ActionBlock';

export abstract class LogicAction extends ActionBlock {
  protected abstract mainLogic(): void;

  startAction(): void {
    this.mainLogic();
  }

  finish(): void {
    this.finishAction();
  }
}
