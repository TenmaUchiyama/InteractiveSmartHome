import { Router } from 'express';
import {
  connectionTest,
  startAllRoutine,
  // startRoutine,
} from './routineController';
import { test } from '../../../test/action-routine-test';

const routineRouter = Router();

routineRouter.get('/', connectionTest);

routineRouter.get('/start-all', startAllRoutine);

routineRouter.get('/test', test);

// routineRouter.get("/start/:routineId", startRoutine);
//
export default routineRouter;
