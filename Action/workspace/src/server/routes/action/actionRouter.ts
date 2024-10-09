import { Router } from 'express';

import {
  addActionApi,
  connectionTest,
  deleteActionApi,
  getActionApi,
  getAllActionsApi,
  updateActionApi,
} from './actionController';

const actionRouter = Router();

actionRouter.get('/', connectionTest);

actionRouter.get('/get-all', getAllActionsApi);

actionRouter.get('/get/:id', getActionApi);

actionRouter.post('/add', addActionApi);

actionRouter.put('/update', updateActionApi);

actionRouter.delete('/delete/:id', deleteActionApi);

export default actionRouter;
