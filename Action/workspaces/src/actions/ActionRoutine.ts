import { ActionBlock } from "./ActionBlock";
import { ActionEvents, ActionEventKey } from "./ActionEvents";

export class ActionRoutine {
  private actionBlocks: ActionBlock[];
  private currentBlockIndex: number;

  constructor(actionBlocks: ActionBlock[]) {
    this.actionBlocks = actionBlocks;
    this.currentBlockIndex = 0;
    ActionEvents.GetInstance().on(
      ActionEventKey.ExitAction,
      this.onActionBlockExit.bind(this)
    );
  }

  public Start(): void {
    this.runNextActionBlock();
  }

  private runNextActionBlock(): void {
    if (this.currentBlockIndex < this.actionBlocks.length) {
      const currentActionBlock = this.actionBlocks[this.currentBlockIndex];
      currentActionBlock.start();
    } else {
      console.log("All ActionBlocks have been executed.");
      process.exit(0);
    }
  }

  private onActionBlockExit(id: string): void {
    let currentActionBlockId = this.actionBlocks[this.currentBlockIndex].getID();
   
    if(currentActionBlockId !== id)
    {
      console.log("X: ActionBlock ID does not match the current block ID.");
      return
    }else{
      console.log(`O: ActionBlock ${id} has been exited.`);
    } 
    this.currentBlockIndex++;
    this.runNextActionBlock();
  }
}
