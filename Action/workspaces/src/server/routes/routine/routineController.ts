import { Request, Response } from "express";
import { ActionBlock } from "@/src/actions/ActionBlock";
import { ActionRoutine } from "@/src/actions/ActionRoutine";
import { IRoutine, IAction, IDevice } from "@/src/types/ActionTypes";
import { MongoDB } from "@/src/database-connector/mongodb";
export const connectionTest = (req: Request, res: Response) => {
  res.send("Hello from Action Server Routine");
};

const getActionRoutine = async (routineID: string) => {
  let actions = await MongoDB.getInstance().getActionsInRoutine(routineID);
  if (!actions) {
    return;
  }

  let actionBlocksPromise = actions.map(async (action: any) => {
    let deviceData = await MongoDB.getInstance().getDeviceData(action.deviceId);
    if (deviceData) {
      let device: IDevice = deviceData as IDevice;
      return new ActionBlock(device);
    }

    return;
  });
  let actionBlocks = (await Promise.all(actionBlocksPromise)).filter(
    (block) => block !== undefined
  );

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

  res.send("All routines started successfully.");
};

/**
 * IDを指定してルーチンを開始する
 * @param req
 * @param res
 */
export const startRoutine = async (req: Request, res: Response) => {
  // リクエストパラメータからroutineIdを取得
  let routineId = req.params.routineId;

  // routineIdからActionRoutineを取得
  let actionRoutine = await getActionRoutine(routineId);
  if (actionRoutine) {
    actionRoutine.start();
  }

  // レスポンス
  res.send("Routine started: " + routineId);
};

/**
 * ルーチンを設定する
 */
// export const setRoutine = async (req: Request, res: Response) => {
//   let routine: IRoutine = req.body;
//   await MongoDB.getInstance().setRoutine(routine);
//   res.send("Routine set successfully.");
// };
