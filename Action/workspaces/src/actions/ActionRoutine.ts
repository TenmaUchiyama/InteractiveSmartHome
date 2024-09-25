import { ActionBlock } from './action-blocks/ActionBlock';
import { ActionEvents, ActionEventKey } from './ActionEvents';

export class ActionRoutine {
  private actionBlocks: ActionBlock[];
  private currentBlockIndex: number;
  private routineId: string;

  constructor(routineId: string, actionBlocks: ActionBlock[]) {
    this.actionBlocks = actionBlocks;
    this.currentBlockIndex = 0;
    this.routineId = routineId;

    ActionEvents.GetInstance().on(
      `${ActionEventKey.ExitAction}_${this.routineId}`,
      this.onActionBlockExit.bind(this),
    );
  }

  public start(): void {
    this.runNextActionBlock();
  }

  private runNextActionBlock(): void {
    if (this.currentBlockIndex < this.actionBlocks.length) {
      const currentActionBlock = this.actionBlocks[this.currentBlockIndex];
      // currentActionBlock.startAction();
    } else {
      console.log('All ActionBlocks have been executed.');
      process.exit(0);
    }
  }

  private onActionBlockExit(id: string): void {
    console.log(id);
    console.log(this.currentBlockIndex);
    console.log(this.actionBlocks);

    let currentActionBlockId =
      this.actionBlocks[this.currentBlockIndex].getID();

    if (currentActionBlockId !== id) {
      console.log('X: ActionBlock ID does not match the current block ID.');
      return;
    } else {
      console.log(`O: ActionBlock ${id} has been exited.`);
    }
    this.currentBlockIndex++;
    this.runNextActionBlock();
  }
}
