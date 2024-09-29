import {
  IDBDeviceData,
  IDeviceData,
  IRoutineData,
} from '../types/ActionBlockInterfaces';
import { ActionType, DeviceType } from '../types/ActionType';
import ActionRoutine from '../actions/ActionRoutine';
import { MongoDB } from '../database-connector/mongodb';

async function main() {
  const routine = await MongoDB.getInstance().getAllRoutine();

  const routineStarter = new ActionRoutine(routine[0]);
  routineStarter.startRoutine();
}

main();
