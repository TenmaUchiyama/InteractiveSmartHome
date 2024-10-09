import { Request, Response } from 'express';
import { MongoDB } from '@database';

export const connectionTest = (req: Request, res: Response) => {
  res.status(200).send('Hello from device');
};

export const getAllActionsApi = async (req: Request, res: Response) => {
  try {
    console.log('Getting all actions');
    const actions = await MongoDB.getInstance().getAllActions();
    return res.status(200).send(actions);
  } catch {
    return res.status(500).send('Failed to get actions');
  }
};

export const getActionApi = async (req: Request, res: Response) => {
  try {
    const actionId = req.params.id;
    console.log('Getting action:', actionId);
    if (actionId === undefined) {
      return res.status(400).send('Action ID is required');
    }
    const action = await MongoDB.getInstance().getAction(actionId);
    if (action === null) {
      return res.status(404).send('Action not found');
    }
    return res.status(200).send(action);
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to get action: ' + error.message);
  }
};

export const addActionApi = async (req: Request, res: Response) => {
  try {
    const action = req.body;
    console.log('Adding action:', action);
    // アクションが配列かどうかを確認し、必要に応じて配列に変換
    const actionArray = Array.isArray(action) ? action : [action];

    await MongoDB.getInstance().addActions(actionArray);
    return res.status(201).send('Action added');
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to add action: ' + error.message);
  }
};

export const updateActionApi = async (req: Request, res: Response) => {
  try {
    const action = req.body as any;
    let isActionArray = Array.isArray(action);

    if (isActionArray) {
      await MongoDB.getInstance().updateAction(action);
    } else {
      await MongoDB.getInstance().updateSingleAction(action);
    }
    return res.status(200).send('Action updated');
  } catch (error) {
    console.error(error);
    return res.status(500).send('Failed to update action: ' + error.message);
  }
};

export const deleteActionApi = async (req: Request, res: Response) => {
  try {
    const actionId = req.params.id;
    console.log('Deleting action:', actionId);
    if (actionId === undefined) {
      return res.status(400).send('Action ID is required');
    }
    await MongoDB.getInstance().deleteAction(actionId);
    return res.status(204).send('Action deleted');
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to delete action: ' + error.message);
  }
};
