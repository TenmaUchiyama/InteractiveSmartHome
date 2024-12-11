import { Router } from "express";
import {
  addDeviceApi,
  connectionTest,
  deleteAllDevicesApi,
  deleteDeviceApi,
  getAllDevicesApi,
  getDeviceApi,
} from "./deviceController";

const deviceRouter = Router();

deviceRouter.get("/", connectionTest);

deviceRouter.get("/get-all", getAllDevicesApi);

deviceRouter.get("/get/:id", getDeviceApi);

deviceRouter.post("/add", addDeviceApi);

deviceRouter.put("/update", addDeviceApi);

deviceRouter.delete("/delete/:id", deleteDeviceApi);

deviceRouter.get("/delete/all-device", deleteAllDevicesApi);
export default deviceRouter;
