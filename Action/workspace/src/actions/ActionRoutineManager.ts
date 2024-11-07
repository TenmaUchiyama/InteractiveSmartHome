import { MongoDB } from "@/database-connector/mongodb";
import ActionRoutine from "./ActionRoutine";
import { IRoutineData } from "@/types/ActionBlockInterfaces";

export default class ActionRoutineManager {
  // Routine ID と ActionRoutine のマップとして複数のRoutineを管理する
  private routineIdMap: Map<string, ActionRoutine> = new Map();

  getRoutineIdMap() {
    return this.routineIdMap;
  }
  async getRunnningRoutines(): Promise<IRoutineData[]> {
    const runningRoutines: IRoutineData[] = [];
    this.routineIdMap.forEach((actionRoutine) => {
      if (actionRoutine.getIsRunning()) {
        runningRoutines.push(actionRoutine.getRoutineData());
      }
    });
    return runningRoutines;
  }

  async startAllRoutines() {
    try {
      const routines = await MongoDB.getInstance().getAllRoutine();
      routines.forEach((routine) => {
        const actionRoutine = new ActionRoutine(routine);
        this.routineIdMap.set(routine.id, actionRoutine);
      });

      const startRoutine = async (actionRoutine: ActionRoutine) => {
        try {
          await actionRoutine.startRoutine();
          return { status: "ok", msg: "Successfully started routine" };
        } catch (error) {
          console.error(`Routine ${actionRoutine.name} failed:`, error);
          console.log(`Restarting routine ${actionRoutine.name}...`);
          return startRoutine(actionRoutine);
        }
      };

      const routinePromises = Array.from(this.routineIdMap.values()).map(
        (actionRoutine) => startRoutine(actionRoutine)
      );

      const results = await Promise.all(routinePromises);
      return { status: "ok", msg: "Successfully started all routines" };
    } catch (error) {
      console.error("Error starting routines:", error);
      return { status: "fail", msg: "Failed to start routines" };
    }
  }

  async stopAllRoutines() {
    this.routineIdMap.forEach(async (actionRoutine) => {
      await this.stopRoutine(actionRoutine.id);
    });
  }

  async startRoutine(routineId: string) {
    console.log("Starting Routine");
    const routine = await MongoDB.getInstance().getRoutine(routineId);
    if (routine === null) {
      return { status: "fail", msg: "Routine not found" };
    }
    if (this.routineIdMap.has(routine.id))
      return { status: "fail", msg: "Routine already running" };
    const actionRoutine = new ActionRoutine(routine);
    this.routineIdMap.set(routine.id, actionRoutine);

    try {
      return await actionRoutine.startRoutine();
    } catch (error) {
      console.error(`Routine ${actionRoutine.name} failed:`, error);
      return {
        status: "fail",
        msg: `Failed to start routine ${actionRoutine.name}`,
      };
    }
  }

  async stopRoutine(routineId: string) {
    // 余分な引用符を削除
    routineId = routineId.replace(/^"|"$/g, "");

    const keys = Array.from(this.routineIdMap.keys());
    keys.forEach((key) => console.log("Key:", key, "Type:", typeof key));

    // キーの比較を詳細に行う
    keys.forEach((key) => {
      const isEqual = key === routineId;
    });

    const actionRoutine = this.routineIdMap.get(routineId);

    if (!actionRoutine) {
      return { status: "fail", msg: "Routine not found" };
    }

    try {
      const result = await actionRoutine.stopRoutine();
      this.routineIdMap.delete(routineId);

      return { status: "ok", msg: "Successfully stopped routine" };
    } catch (error) {
      console.error(`Routine ${actionRoutine.name} failed:`, error);
      return {
        status: "fail",
        msg: `Failed to stop routine ${actionRoutine.name}`,
      };
    }
  }

  async isRoutineRunning(routineId: string) {
    const actionRoutine = this.routineIdMap.get(routineId);
    if (!actionRoutine) {
      return { status: "error" };
    }

    return { status: "ok", running: actionRoutine.getIsRunning() };
  }

  async restartRoutine(routineId: string) {
    const actionRoutine = this.routineIdMap.get(routineId);
    if (!actionRoutine) {
      return { status: "fail", msg: "Routine not found" };
    }

    try {
      await this.stopRoutine(routineId);
      await this.startRoutine(routineId);
      return { status: "ok", msg: "Successfully restarted routine" };
    } catch (error) {
      console.error(`Routine ${actionRoutine.name} failed:`, error);
      return {
        status: "fail",
        msg: `Failed to restart routine ${actionRoutine.name}`,
      };
    }
  }
}
