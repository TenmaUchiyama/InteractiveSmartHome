import { Request, Response } from 'express';
import { ActionBlock } from '../../../actions/action-blocks/ActionBlock';
import { ActionRoutine } from '../../../actions/ActionRoutine';
import { TimerLogicAction } from '../../../actions/action-blocks/logic';
import { RoutineData, DeviceData } from '../../../types';
import { MongoDB } from '../../../database-connector/mongodb';

export const connectionTest = (req: Request, res: Response) => {
  res.send('Hello from Action Server Routine');
};

const getActionRoutine = async (routineID: string) => {
  //DBからルーティーンアクションを取得
  let actions = await MongoDB.getInstance().getActionsInRoutine(routineID);
  if (!actions) {
    return;
  }

  //ルーティーンアクションIDからアクションブロックインスタンスを生成
  let actionBlocksPromise = actions.map(async (action: any) => {
    if (action.actionType === 'logic') {
      return new TimerLogicAction(routineID, action);
    }

    return undefined;
  });
  let actionBlocks = (await Promise.all(actionBlocksPromise)).filter(
    (block) => block !== undefined,
  ) as ActionBlock[];

  return new ActionRoutine(routineID, actionBlocks);
};

/**
 * すべてのルーチンを開始する
 * @param req
 * @param res
 */
export const startAllRoutine = async (req: Request, res: Response) => {
  let datas = await MongoDB.getInstance().getWholeData();

  for (const routine of datas.routines) {
    let actionRoutine = await getActionRoutine(routine.id);
    if (actionRoutine) {
      // actionRoutineがundefinedでないことを確認
      // 全ての非同期処理が完了するのを待つ
      actionRoutine.start();
    }
  }

  res.send('All routines started successfully.');
};

/**
 * IDを指定してルーチンを開始する
 * @param req
 * @param res
 */
// export const  import { ActionBlock } from './action-blocks/ActionBlock';
// import { ActionEvents, ActionEventKey } from './ActionEvents';

// export class ActionRoutine {
//   private actionBlocks: ActionBlock[];
//   private currentBlockIndex: number;
//   private routineId: string;

//   constructor(routineId: string, actionBlocks: ActionBlock[]) {
//     this.actionBlocks = actionBlocks;
//     this.currentBlockIndex = 0;
//     this.routineId = routineId;

//     ActionEvents.GetInstance().on(
//       `${ActionEventKey.ExitAction}_${this.routineId}`,
//       this.onActionBlockExit.bind(this),
//     );
//   }

//   public start(): void {
//     this.runNextActionBlock();
//   }

//   private runNextActionBlock(): void {
//     if (this.currentBlockIndex < this.actionBlocks.length) {
//       const currentActionBlock = this.actionBlocks[this.currentBlockIndex];
//       currentActionBlock.startAction();
//     } else {
//       console.log('All ActionBlocks have been executed.');
//       process.exit(0);
//     }
//   }

//   private onActionBlockExit(id: string): void {
//     console.log(id);
//     console.log(this.currentBlockIndex);
//     console.log(this.actionBlocks);

//     let currentActionBlockId =
//       this.actionBlocks[this.currentBlockIndex].getID();

//     if (currentActionBlockId !== id) {
//       console.log('X: ActionBlock ID does not match the current block ID.');
//       return;
//     } else {
//       console.log(`O: ActionBlock ${id} has been exited.`);
//     }
//     this.currentBlockIndex++;
//     this.runNextActionBlock();
//   }
// }
// startRoutine = async (req: Request, res: Response) => {
//   // リクエストパラメータからroutineIdを取得
//   let routineId = req.params.routineId;

//   // routineIdからActionRoutineを取得
//   let actionRoutine = await getActionRoutine(routineId);
//   if (actionRoutine) {
//     actionRoutine.start();
//   }

//   // レスポンス
//   res.send('Routine started: ' + routineId);
// };

/**
 * ルーチンを設定する
 */
// export const setRoutine = async (req: Request, res: Response) => {
//   let routine: IRoutine = req.body;
//   await MongoDB.getInstance().setRoutine(routine);
//   res.send("Routine set successfully.");
// };
