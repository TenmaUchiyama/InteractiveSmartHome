import ActionBlock from "./ActionBlock";


export default class ActionManager
{

    private actionBlocks: ActionBlock[] ; 
    private currentBlockInd: number = 0;


    constructor (){


        this.actionBlocks = [
            new ActionBlock("Action1"),
            new ActionBlock("Action2"),
            new ActionBlock("Action3"),
            new ActionBlock("Action4"),
            new ActionBlock("Action5"),
            new ActionBlock("Action6"),
        ]
    }



    startRoutine = async () => {
        await 
    }

    executeCurrentBlock = async () => {
        if (this.currentBlockInd < this.actionBlocks.length) {
            const currentAction = this.actionBlocks[this.currentBlockInd];
            currentAction.startAction();
            this.moveToNextBlock();
        } else {
            this.postProcess();
        }
    }


    moveToNextBlock = () => {
        this.currentBlockInd++;
        this.executeCurrentBlock();
    }


    postProcess =() => {
        console.log("Done Everything")
    }

    



}



