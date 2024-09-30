import {
  IDBDeviceData,
  IDeviceData,
  IRoutineData,
} from '../types/ActionBlockInterfaces';
import { ActionType, DeviceType } from '../types/ActionType';
import ActionRoutine from '../actions/ActionRoutine';
import { MongoDB } from '../database-connector/mongodb';
import Debugger from '../debugger/Debugger';
import chalk from 'chalk';
async function main() {
  // const routine = await MongoDB.getInstance().getAllRoutine();

  // const routineStarter = new ActionRoutine(routine[0]);
  // routineStarter.startRoutine();
  const log = console.log;
  log(chalk.blue.bgRed.bold('Hello world!'));
}

main();
