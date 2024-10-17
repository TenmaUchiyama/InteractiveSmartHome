import { Request, Response } from "express";
import { MongoDB } from "@database";
import ActionRoutine from "@/actions/ActionRoutine";
import ActionRoutineManager from "@/actions/ActionRoutineManager";
import SocketBroadcaster from "@/socket-broadcaster/SocketBroadcaster";
import {
  RunnningRoutineSocketData,
  SocketMessageType,
} from "@/types/SocketMessageType";

const routineManager = new ActionRoutineManager();

export const connectionTest = (req: Request, res: Response) => {
  res.status(200).send("Hello from routine");
};

export const startAllRoutineApi = async (req: Request, res: Response) => {
  try {
    const result = await routineManager.startAllRoutines();

    if (result && result.status === "fail") {
      res.status(500).send("Failed to start all routines");
    } else {
      res.status(200).send("Starting all routines");
    }
    await castRunningRoutine();
  } catch {
    return res.status(500).send("Failed to start all routines");
  }
};

export const startRoutineApi = async (req: Request, res: Response) => {
  try {
    console.log(routineManager.getRoutineIdMap());

    const routineId = req.params.id;
    console.log("starting routine", routineId);
    const isRunning = await routineManager.isRoutineRunning(routineId);

    if (isRunning.status === "ok" && isRunning.running) {
      await routineManager.restartRoutine(routineId);
      return res.status(200).send("Restart Routine");
    }
    const result = await routineManager.startRoutine(routineId);
    if (result && "status" in result && result.status === "fail") {
      res.status(500).send("Failed to start routine");
    } else {
      res.status(200).send("Starting routine");
    }

    await castRunningRoutine();
  } catch {
    return res.status(500).send("Failed to start routine");
  }
};

export const getRunningRoutineApi = async (req: Request, res: Response) => {
  try {
    const runningRoutine = await routineManager.getRunnningRoutines();

    return res.status(200).send(runningRoutine);
  } catch {
    return res.status(500).send("Failed to get running routines");
  }
};

export const stopRoutineApi = async (req: Request, res: Response) => {
  try {
    const routineId = req.params.id;
    console.log("stopping routine", routineId);
    const result: { status: string } | void =
      await routineManager.stopRoutine(routineId);
    console.log("Stop routine result:", result);
    if (result && "status" in result && result.status === "fail") {
      res.status(500).send("Failed to stop routine");
    } else {
      res.status(200).send("Stopping routine succeeded " + routineId);
    }
  } catch {
    return res.status(500).send("Failed to stop routine");
  }

  castRunningRoutine();
};

export const stopAllRoutinesApi = async (req: Request, res: Response) => {
  try {
    const result = await routineManager.stopAllRoutines();

    await castRunningRoutine();
    res.status(200).send("Stopping all routines");
  } catch {
    return res.status(500).send("Failed to stop all routines");
  }
};
export const getAllRoutineApi = async (req: Request, res: Response) => {
  try {
    const routines = await MongoDB.getInstance().getAllRoutine();
    console.log("Getting all routines");
    return res.status(200).send(routines);
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error); // エラー内容をログに記録
      return res.status(500).send("Failed to get routines: " + error.message);
    } else {
      console.error("Unexpected error:", error);
      return res.status(500).send("Failed to get routines: Unknown error");
    }
  }
};

export const getRoutineApi = async (req: Request, res: Response) => {
  try {
    const routineId = req.params.id;
    const routine = await MongoDB.getInstance().getRoutine(routineId);
    if (routine === null) {
      return res.status(404).send("Routine not found");
    }
    return res.status(200).send(routine);
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error); // エラー内容をログに記録
      return res.status(500).send("Failed to get routine: " + error.message);
    } else {
      console.error("Unexpected error:", error);
      return res.status(500).send("Failed to get routine: Unknown error");
    }
  }
};

export const addRoutineApi = async (req: Request, res: Response) => {
  try {
    const routine = req.body;
    console.log("Adding routine:", routine);
    // ルーチンが配列かどうかを確認し、必要に応じて配列に変換
    const routineArray = Array.isArray(routine) ? routine : [routine];

    await MongoDB.getInstance().addRoutines(routineArray);
    return res.status(201).send("Routine added");
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error); // エラー内容をログに記録
      return res.status(500).send("Failed to start routine: " + error.message);
    } else {
      console.error("Unexpected error:", error);
      return res.status(500).send("Failed to start routine: Unknown error");
    }
  }
};

export const deleteRoutineApi = async (req: Request, res: Response) => {
  try {
    const routineId = req.params.id;
    console.log("Deleting routine:", routineId);
    if (routineId === undefined) {
      return res.status(400).send("Routine ID is required");
    }
    await MongoDB.getInstance().deleteRoutine(routineId);
    return res.status(204).send("Routine deleted");
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error); // エラー内容をログに記録
      return res.status(500).send("Failed to start routine: " + error.message);
    } else {
      console.error("Unexpected error:", error);
      return res.status(500).send("Failed to start routine: Unknown error");
    }
  }
};

export const updateRoutineApi = async (req: Request, res: Response) => {
  try {
    const routine = req.body;
    console.log("Updating routine:", routine);

    await MongoDB.getInstance().updateRoutine(routine);
    return res.status(200).send("Routine updated");
  } catch (error: unknown) {
    if (error instanceof Error) {
      console.error(error); // エラー内容をログに記録
      return res.status(500).send("Failed to start routine: " + error.message);
    } else {
      console.error("Unexpected error:", error);
      return res.status(500).send("Failed to start routine: Unknown error");
    }
  }
};

export const castRunningRoutine = async () => {
  const runningRoutine = await routineManager.getRunnningRoutines();
  const socketMsg: RunnningRoutineSocketData = {
    message_type: SocketMessageType.Running_Routine,
    running_routine: runningRoutine,
  };
  await SocketBroadcaster.getInstance().broadcastMessage(socketMsg);
};
