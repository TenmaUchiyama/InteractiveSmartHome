import { EventEmitter } from "stream";
import {ActionBlock} from "./ActionBlock";
import { ActionEvents, ActionEventKey } from "./ActionEvents";


export class ActionRoutine 
{

    private actionBlocks : ActionBlock[]; 
    private currentBlockIndex: number;

    constructor(actionBlocks: ActionBlock[]) 
    {
        this.actionBlocks = actionBlocks; 
        this.currentBlockIndex = 0; 
        ActionEvents.GetInstance().on(ActionEventKey.ExitAction, this.onActionBlockExit.bind(this));
    }


    public Start(): void {
        this.runNextActionBlock();
    }

    private runNextActionBlock(): void {
        if (this.currentBlockIndex < this.actionBlocks.length) {
            const currentActionBlock = this.actionBlocks[this.currentBlockIndex];
            currentActionBlock.Start();
        } else {
            console.log('All ActionBlocks have been executed.');
        }
    }

    private onActionBlockExit(id: number): void {
        console.log(`ActionBlock ${id} exited.`);
        this.currentBlockIndex++;
        this.runNextActionBlock();
    }

}
