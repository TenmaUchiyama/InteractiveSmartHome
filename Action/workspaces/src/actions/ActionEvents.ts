import { EventEmitter } from "events";

export enum ActionEventKey
{
    StartAction = "start_action", 
    ExitAction = "exit_action"
}


class ActionEvents{
    private static instance: ActionEvents; 

    private emitter: EventEmitter;

    private constructor(){
        this.emitter = new EventEmitter;

    }



    public static GetInstance = () => {
        if(ActionEvents.instance == null)
        {
            ActionEvents.instance = new ActionEvents()
        }

        return ActionEvents.instance;
    }


    public on(event: string, listener: (...args: any[]) => void): void {
        this.emitter.on(event, listener);
    }

    public emit(event: string, ...args: any[]): void {
        this.emitter.emit(event, ...args);
    }

    public off(event: string, listener: (...args: any[]) => void): void {
        this.emitter.off(event, listener);
    }
}