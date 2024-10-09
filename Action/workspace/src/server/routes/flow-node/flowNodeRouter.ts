import { Router } from 'express';
import {
  addNodeApi,
  connectionTest,
  deleteNodeApi,
  getAllNodesApi,
  getMultipleNodesApi,
  getNodeApi,
  updateNodeApi,
} from './flowNodeController';

const flowNodeRouter = Router();

flowNodeRouter.get('/', connectionTest);

flowNodeRouter.get('/get-all', getAllNodesApi);

flowNodeRouter.get('/get/:id', getNodeApi);
flowNodeRouter.post('/get/', getMultipleNodesApi);

flowNodeRouter.post('/add', addNodeApi);

flowNodeRouter.put('/update', updateNodeApi);

flowNodeRouter.delete('/delete/:id', deleteNodeApi);

export default flowNodeRouter;
