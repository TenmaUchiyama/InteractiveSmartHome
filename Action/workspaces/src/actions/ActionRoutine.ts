import {
  IActionBlock,
  IRoutine,
  IRoutineData,
} from '../types/ActionBlockInterfaces';
import ActionBlock from './action-blocks/ActionBlock';
import getActionBlock from './GetBlockInstance';
import { MongoDB } from '../database-connector/mongodb';

export default class ActionRoutine implements IRoutineData {
  id: string;
  name: string;
  action_routine: IRoutine[];

  constructor(routineData: IRoutineData) {
    this.id = routineData.id;
    this.name = routineData.name;
    this.action_routine = routineData.action_routine;
  }

  async startRoutine() {
    const actionIdMap = new Map<string, ActionBlock>();

    for (const route of this.action_routine) {
      let currentBlock: ActionBlock | undefined;
      if (actionIdMap.has(route.current_block_id)) {
        currentBlock = actionIdMap.get(route.current_block_id);
      } else {
        const currentBlockId = route.current_block_id;
        const blockData = await MongoDB.getInstance().getActionData(
          currentBlockId,
        );

        const newBlock = await getActionBlock(blockData);

        actionIdMap.set(route.current_block_id, newBlock);
        currentBlock = newBlock;
      }

      if (route.last) continue;
      if (route.next_block_id === '') continue;

      let nextBlock: ActionBlock | undefined;
      if (actionIdMap.has(route.next_block_id)) {
        nextBlock = actionIdMap.get(route.next_block_id);
      } else {
        const blockData = await MongoDB.getInstance().getActionData(
          route.next_block_id,
        );
        const newBlock = await getActionBlock(blockData);

        actionIdMap.set(route.next_block_id, newBlock);
        nextBlock = newBlock;
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
        console.error(`Action not found for ID: ${block.current_block_id}`);
      }
    });

    console.log('[ACTION ROUTINE] Routine started ID: ', this.id);
  }
}
