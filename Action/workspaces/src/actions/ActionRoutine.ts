import {
  IActionBlock,
  IRoutine,
  IRoutineData,
} from '@/types/ActionBlockInterfaces';
import ActionBlock from '@block/ActionBlock';
import getActionBlock from './GetBlockInstance';
import { MongoDB } from '@database';
import Debugger from '@debugger/Debugger';

export default class ActionRoutine implements IRoutineData {
  id: string;
  name: string;
  action_routine: IRoutine[];

  constructor(routineData: IRoutineData) {
    this.id = routineData.id;
    this.name = routineData.name;
    this.action_routine = routineData.action_routine;
  }

  async createActionIdMap(): Promise<Map<string, ActionBlock>> {
    const uniqueBlockIds = new Set<string>();
    for (const route of this.action_routine) {
      uniqueBlockIds.add(route.current_block_id);
    }

    const actionMap = new Map<string, ActionBlock>();

    for (const blockId of uniqueBlockIds) {
      const block = await MongoDB.getInstance().getAction(blockId);
      if (block === null) {
        console.error(`Block not found for ID: ${blockId}`);
        continue;
      }

      const newBlock = await getActionBlock(block);
      if (newBlock) {
        newBlock.setRoutineId(this.id);
      } else {
        console.error(`Block not found for ID: ${blockId}`);
        continue;
      }

      actionMap.set(blockId, newBlock);
    }

    return actionMap;
  }

  async startRoutine() {
    const actionIdMap = await this.createActionIdMap();

    Debugger.getInstance().debugLog(
      this.id,
      'ActionRoutine',
      'Starting routine: ' + JSON.stringify(actionIdMap.keys()),
    );
    for (const route of this.action_routine) {
      let currentBlock: ActionBlock | undefined;
      if (actionIdMap.has(route.current_block_id)) {
        currentBlock = actionIdMap.get(route.current_block_id);
      } else {
        Debugger.getInstance().debugError(
          this.id,
          'ActionRoutine',
          `Block not found for ID: ${route.current_block_id}`,
        );
      }

      if (route.last) break;
      if (route.next_block_id === '') continue;

      let nextBlock: ActionBlock | undefined;
      if (actionIdMap.has(route.next_block_id)) {
        nextBlock = actionIdMap.get(route.next_block_id);
      } else {
        Debugger.getInstance().debugError(
          this.id,
          'ActionRoutine',
          `Block not found for ID: ${route.next_block_id}`,
        );
      }

      if (currentBlock && nextBlock) {
        currentBlock.setNextActionBlock([nextBlock]);
      }
    }

    const firstBlocks: IRoutine[] = this.action_routine.filter(
      (route) => route.first,
    );

    firstBlocks.forEach((block: IRoutine) => {
      const action = actionIdMap.get(block.current_block_id);
      if (action) {
        action.startAction();
      } else {
        Debugger.getInstance().debugError(
          this.id,
          'ActionRoutine',
          `Block not found for ID: ${block.current_block_id}`,
        );
      }
    });
  }
}
