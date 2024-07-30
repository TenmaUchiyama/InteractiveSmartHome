import { ActionBlock } from "../actions/ActionBlock";
import { ActionRoutine } from "../actions/ActionRoutine";
import { randomUUID } from 'crypto';

const uuid = randomUUID();



let actionRoutine = new ActionRoutine([
    new ActionBlock(randomUUID(), "topic1_1"), 
    new ActionBlock(randomUUID(), "topic1_2"), 
    new ActionBlock(randomUUID(), "topic1_3"), 
    new ActionBlock(randomUUID(), "topic1_4"), 
    new ActionBlock(randomUUID(), "topic1_5"), 
    new ActionBlock(randomUUID(), "topic1_6"), 
    new ActionBlock(randomUUID(), "topic1_7"), 
]);


let actionRoutine2 = new ActionRoutine([
    new ActionBlock(randomUUID(), "topic2_1"), 
    new ActionBlock(randomUUID(), "topic2_2"), 
    new ActionBlock(randomUUID(), "topic2_3"), 
    new ActionBlock(randomUUID(), "topic2_4"), 
    new ActionBlock(randomUUID(), "topic2_5"), 
    new ActionBlock(randomUUID(), "topic2_6"), 
    new ActionBlock(randomUUID(), "topic2_7"), 
]);



actionRoutine.Start(); 
actionRoutine2.Start();