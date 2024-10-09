import { Request, Response } from 'express';
import { MongoDB } from '@database';

export const connectionTest = (req: Request, res: Response) => {
  res.status(200).send('Hello from routine');
};

export const getAllEdgesApi = async (req: Request, res: Response) => {
  try {
    console.log('Getting all edges');
    const edges = await MongoDB.getInstance().getAllEdges();
    return res.status(200).send(edges);
  } catch {
    return res.status(500).send('Failed to get edges');
  }
};

export const getEdgeApi = async (req: Request, res: Response) => {
  try {
    const edgeId = req.params.id;
    console.log('Getting edge:', edgeId);
    if (edgeId === undefined) {
      return res.status(400).send('Edge ID is required');
    }
    const edge = await MongoDB.getInstance().getEdge(edgeId);
    if (edge === null) {
      return res.status(404).send('Edge not found');
    }
    return res.status(200).send(edge);
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to get edge: ' + error.message);
  }
};

export const addEdgeApi = async (req: Request, res: Response) => {
  try {
    const edge = req.body;
    console.log('Adding edge:', edge);
    // エッジが配列かどうかを確認し、必要に応じて配列に変換
    const edgeArray = Array.isArray(edge) ? edge : [edge];

    await MongoDB.getInstance().addEdge(edgeArray);
    return res.status(201).send('Edge added');
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to add edge: ' + error.message);
  }
};

export const updateEdgeApi = async (req: Request, res: Response) => {
  try {
    const edge = req.body;
    console.log('Updating edge:', edge);
    await MongoDB.getInstance().updateEdge(edge);
    return res.status(200).send('Edge updated');
  } catch (error) {
    console.error(error);
    return res.status(500).send('Failed to update edge: ' + error.message);
  }
};

export const deleteEdgeApi = async (req: Request, res: Response) => {
  try {
    const edgeId = req.params.id;
    console.log('Deleting edge:', edgeId);
    await MongoDB.getInstance().deleteEdge(edgeId);
    return res.status(200).send('Edge deleted');
  } catch (error) {
    console.error(error);
    return res.status(500).send('Failed to delete edge: ' + error.message);
  }
};
