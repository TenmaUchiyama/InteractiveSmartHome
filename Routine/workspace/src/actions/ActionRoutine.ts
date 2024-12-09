import {
  IActionBlock,
  IRoutine,
  IRoutineData,
} from "@/types/ActionBlockInterfaces";
import ActionBlock from "@block/ActionBlock";
import getActionBlock from "./GetBlockInstance";
import { MongoDB } from "@database";
import Debugger from "@debugger/Debugger";

export default class ActionRoutine implements IRoutineData {
  id: string;
  name: string;
  action_routine: IRoutine[];
  is_runninng: boolean = false;
  actionIdMap: Map<string, ActionBlock> = new Map();
  runningActions: Promise<void>[] = [];
  routine_data: IRoutineData;

  constructor(routineData: IRoutineData) {
    this.id = routineData.id;
    this.name = routineData.name;
    this.action_routine = routineData.action_routine;
    this.routine_data = routineData;
  }

  getIsRunning(): boolean {
    return this.is_runninng;
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

  getRoutineData(): IRoutineData {
    return this.routine_data;
  }

  async startRoutine() {
    this.actionIdMap = await this.createActionIdMap();
    console.log(`Action ID Map: ${JSON.stringify(this.actionIdMap, null, 2)}`);
    Debugger.getInstance().debugLog(
      this.id,
      "ActionRoutine",
      "Starting routine: " +
        JSON.stringify(
          Array.from(this.actionIdMap.values()).map(
            (actionBlock) => actionBlock.name
          )
        )
    );
    for (const route of this.action_routine) {
      let currentBlock: ActionBlock | undefined;
      if (this.actionIdMap.has(route.current_block_id)) {
        currentBlock = this.actionIdMap.get(route.current_block_id);
      } else {
        Debugger.getInstance().debugError(
          this.id,
          "ActionRoutine",
          `Block not found for ID: ${route.current_block_id}`
        );
      }

      if (route.last) break;
      if (route.next_block_id === "") continue;

      let nextBlock: ActionBlock | undefined;
      if (this.actionIdMap.has(route.next_block_id)) {
        nextBlock = this.actionIdMap.get(route.next_block_id);
      } else {
        Debugger.getInstance().debugError(
          this.id,
          "ActionRoutine",
          `Block not found for ID: ${route.next_block_id}`
        );
      }

      if (currentBlock && nextBlock) {
        currentBlock.setNextActionBlock([nextBlock]);
      }
    }

    const firstBlocks: IRoutine[] = this.action_routine.filter(
      (route) => route.first
    );

    firstBlocks.forEach((block: IRoutine) => {
      const action = this.actionIdMap.get(block.current_block_id);
      if (action) {
        action.startAction();
      } else {
        Debugger.getInstance().debugError(
          this.id,
          "ActionRoutine",
          `Block not found for ID: ${block.current_block_id}`
        );
      }
    });

    this.is_runninng = true;
  }

  async stopRoutine() {
    if (this.actionIdMap.size === 0) return;

    for (const block of this.actionIdMap.values()) {
      await block.exitAction();
    }
    this.actionIdMap.clear();
  }

  async restartRoutine() {
    await this.stopRoutine();

    await this.startRoutine();
  }

  getRoutines() {
    return this.action_routine;
  }
}
