import { Router } from 'express';
import {
  connectionTest,
  startAllRoutine,
  startRoutine,
} from './routineController';

const routineRouter = Router();

routineRouter.get('/', connectionTest);

routineRouter.get('/start-all', startAllRoutine);

routineRouter.get('/start/:id', startRoutine);

export default routineRouter;
