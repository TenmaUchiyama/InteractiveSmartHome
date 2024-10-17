import { Request, Response } from "express";
import { MongoDB } from "@database";
import { IDBNode } from "@/types/FlowNodeType";

export const connectionTest = (req: Request, res: Response) => {
  res.status(200).send("Hello from routine");
};

export const getAllNodesApi = async (req: Request, res: Response) => {
  try {
    console.log("Getting all nodes");
    const nodes = await MongoDB.getInstance().getAllNode();
    return res.status(200).send(nodes);
  } catch {
    return res.status(500).send("Failed to get nodes");
  }
};

export const getNodeApi = async (req: Request, res: Response) => {
  try {
    const nodeId = req.params.id;
    console.log("Getting node:", nodeId);
    if (nodeId === undefined) {
      return res.status(400).send("Node ID is required");
    }
    const node = await MongoDB.getInstance().getNodes([nodeId]);
    if (node === null) {
      return res.status(404).send("Node not found");
    }
    return res.status(200).send(node);
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send("Failed to get node: " + error.message);
  }
};

export const getMultipleNodesApi = async (req: Request, res: Response) => {
  try {
    const nodeIds = req.body as string[];
    console.log("Getting multiple nodes:", nodeIds);
    if (nodeIds === undefined || nodeIds.length === 0) {
      return res.status(400).send("Node IDs are required");
    }
    const nodes = await MongoDB.getInstance().getNodes(nodeIds);
    if (nodes === null) {
      return res.status(404).send("Nodes not found");
    }
    return res.status(200).send(nodes);
  } catch (error) {
    console.error(error);
    return res.status(500).send("Failed to get nodes: " + error.message);
  }
};

export const addNodeApi = async (req: Request, res: Response) => {
  try {
    const node = req.body as IDBNode;
    console.log("Adding node:", node);
    // ノードが配列かどうかを確認し、必要に応じて配列に変換
    const nodeArray = Array.isArray(node) ? node : [node];

    await MongoDB.getInstance().addNode(nodeArray);
    return res.status(201).send("Node added");
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send("Failed to add node: " + error.message);
  }
};

export const updateNodeApi = async (req: Request, res: Response) => {
  try {
    const node = req.body as IDBNode[];

    console.log("Updating node:", node);
    await MongoDB.getInstance().updateNodes(node);
    return res.status(200).send("Node updated");
  } catch (error) {
    console.error(error);
    return res.status(500).send("Failed to update node: " + error.message);
  }
};

export const deleteNodesApi = async (req: Request, res: Response) => {
  try {
    const nodeIds: string[] = req.body.ids; // リクエストボディから複数のIDを取得
    console.log("Deleting nodes:", nodeIds);

    if (!Array.isArray(nodeIds) || nodeIds.length === 0) {
      return res.status(400).send("No node IDs provided for deletion");
    }

    await MongoDB.getInstance().deleteNodes(nodeIds); // 複数のノードを削除するメソッドを呼び出し
    return res.status(200).send("Nodes deleted");
  } catch (error) {
    console.error(error);
    return res.status(500).send("Failed to delete nodes: " + error.message);
  }
};
