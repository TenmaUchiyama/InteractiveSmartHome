import { EventEmitter } from "events";

export enum BridgeEventKey
{
    StartAction = "start_action", 
    ExitAction = "exit_action"
}


export class BridgeEvents{
    private static instance: BridgeEvents; 

    private emitter: EventEmitter;

    private constructor(){
        this.emitter = new EventEmitter;

    }



    public static GetInstance = () => {
        if(BridgeEvents.instance == null)
        {
            BridgeEvents.instance = new BridgeEvents()
        }

        return BridgeEvents.instance;
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