import { Router } from "express";
import {
  addNodeApi,
  connectionTest,
  deleteNodesApi,
  getAllNodesApi,
  getMultipleNodesApi,
  getNodeApi,
  updateNodeApi,
} from "./flowNodeController";

const flowNodeRouter = Router();

flowNodeRouter.get("/", connectionTest);

flowNodeRouter.get("/get-all", getAllNodesApi);

flowNodeRouter.get("/get/:id", getNodeApi);
flowNodeRouter.post("/get/", getMultipleNodesApi);

flowNodeRouter.post("/add", addNodeApi);

flowNodeRouter.put("/update", updateNodeApi);

flowNodeRouter.delete("/delete/", deleteNodesApi);

export default flowNodeRouter;
