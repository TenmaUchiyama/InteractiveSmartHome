import { Router } from 'express';
import {
  addRoutineApi,
  connectionTest,
  deleteRoutineApi,
  getAllRoutineApi,
  getRoutineApi,
  startAllRoutineApi,
  startRoutineApi,
  updateRoutineApi,
} from './routineController';

const routineRouter = Router();

routineRouter.get('/', connectionTest);

routineRouter.get('/start-all', startAllRoutineApi);

routineRouter.get('/start/:id', startRoutineApi);

routineRouter.get('/get-all', getAllRoutineApi);

routineRouter.get('/get/:id', getRoutineApi);

routineRouter.post('/add', addRoutineApi);

routineRouter.put('/update', updateRoutineApi);

routineRouter.delete('/delete/:id', deleteRoutineApi);

export default routineRouter;
