import { ActionEvents, ActionEventKey } from './ActionEvents';
import DeviceSignalTransit from '../device-bridge/DeviceBridge'; // 実際のファイルパスに置き換えてください

export class ActionBlock {
    private id: number;

    constructor(id: number) {
        this.id = id;
    }

    public Start(): void {
        console.log(`ActionBlock ${this.id} is starting.`);
        setTimeout(() => {
            console.log(`ActionBlock ${this.id} has finished.`);
            ActionEvents.GetInstance().emit(ActionEventKey.ExitAction, this.id);
        }, 1000);
    }
}
