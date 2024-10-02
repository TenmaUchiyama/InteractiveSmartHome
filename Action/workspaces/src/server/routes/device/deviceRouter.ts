import { Router } from 'express';
import {
  addDeviceApi,
  connectionTest,
  deleteDeviceApi,
  getAllDevicesApi,
  getDeviceApi,
} from './deviceController';

const deviceRouter = Router();

deviceRouter.get('/', connectionTest);

deviceRouter.get('/get-all', getAllDevicesApi);

deviceRouter.get('/get/:id', getDeviceApi);

deviceRouter.post('/add', addDeviceApi);

deviceRouter.put('/update', addDeviceApi);

deviceRouter.delete('/delete/:id', deleteDeviceApi);
export default deviceRouter;
