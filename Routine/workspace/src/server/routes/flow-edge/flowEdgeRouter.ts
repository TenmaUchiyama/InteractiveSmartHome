import { Router } from 'express';
import {
  addEdgeApi,
  connectionTest,
  deleteEdgeApi,
  getAllEdgesApi,
  getEdgeApi,
  updateEdgeApi,
} from './flowEdgeController';

const flowEdgeRouter = Router();

flowEdgeRouter.get('/', connectionTest);

flowEdgeRouter.get('/get-all', getAllEdgesApi);

flowEdgeRouter.get('/get/:id', getEdgeApi);

flowEdgeRouter.post('/add', addEdgeApi);

flowEdgeRouter.put('/update', updateEdgeApi);

flowEdgeRouter.delete('/delete/:id', deleteEdgeApi);

export default flowEdgeRouter;
