import { ActionBlock } from "../actions/ActionBlock";
import { ActionRoutine } from "../actions/ActionRoutine";
import { randomUUID } from 'crypto';
import { fetchData } from "./mongo-test";
import { IAction, IDevice, IRoutine } from "src/types/ActionTypes";




const main = async () => {
    let datas = await fetchData();
    
    let routineActions = datas.routines.flatMap((routine: IRoutine) => {
        let data_actionBlock: IAction[] = routine.actionBlocks.map((actionBlockId: string) => {
            let action = datas.actions.find((action: IAction) => action.id === actionBlockId);
            if (action === undefined) {
                throw new Error(`Action ${actionBlockId} not found`);
            }
            return action;
        });
        
        let actionBlocks: ActionBlock[] = data_actionBlock.map((action: IAction) => {
            if (action.actionType === "sensor" || action.actionType === "actuator") {
                let device = datas.devices.find((device: IDevice) => device.id === action.deviceId);
                if (device) {
                    return new ActionBlock(device);
                }
            }
            throw new Error(`Device for action ${action.id} not found`);
        });
    
        // 配列の配列にならないように、flatMapを使用してフラット化
        return actionBlocks;
    });
    
   
    let actionRoutine = new ActionRoutine(datas.routines[0].id, routineActions);


}   

main()



 



// let actionRoutine = new ActionRoutine([
//     new ActionBlock(randomUUID(), "topic1_1"), 
//     new ActionBlock(randomUUID(), "topic1_2"), 
//     new ActionBlock(randomUUID(), "topic1_3"), 
//     new ActionBlock(randomUUID(), "topic1_4"), 
//     new ActionBlock(randomUUID(), "topic1_5"), 
//     new ActionBlock(randomUUID(), "topic1_6"), 
//     new ActionBlock(randomUUID(), "topic1_7"), 
// ]);


// let actionRoutine2 = new ActionRoutine([
//     new ActionBlock(randomUUID(), "topic2_1"), 
//     new ActionBlock(randomUUID(), "topic2_2"), 
//     new ActionBlock(randomUUID(), "topic2_3"), 
//     new ActionBlock(randomUUID(), "topic2_4"), 
//     new ActionBlock(randomUUID(), "topic2_5"), 
//     new ActionBlock(randomUUID(), "topic2_6"), 
//     new ActionBlock(randomUUID(), "topic2_7"), 
// ]);



// actionRoutine.Start(); 
// actionRoutine2.Start();