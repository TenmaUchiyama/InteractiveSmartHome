import { Request, Response } from 'express';
import { MongoDB } from '../../../database-connector/mongodb';
import ActionRoutine from '../../../actions/ActionRoutine';

export const connectionTest = (req: Request, res: Response) => {
  res.status(200).send('Hello from routine');
};

export const startAllRoutine = async (req: Request, res: Response) => {
  try {
    console.log('Starting all routines');
    const routines = await MongoDB.getInstance().getAllRoutine();

    let routinePromises = routines.map(async (routine, index) => {
      const routineStarter = new ActionRoutine(routine);
      return routineStarter.startRoutine();
    });

    routinePromises = [routinePromises[1], routinePromises[0]]; // 2つのルーチンだけを実行するようにする

    // Promise.allで全てのroutinesを並行で実行
    await Promise.all(routinePromises);
    return res.status(200).send('Starting all routines');
  } catch {
    return res.status(500).send('Failed to start routines');
  }
};

export const startRoutine = async (req: Request, res: Response) => {
  try {
    const routineId = req.params.id;
    const routine = await MongoDB.getInstance().getRoutine(routineId);
    if (routine === null) {
      return res.status(404).send('Routine not found');
    }

    const routineStarter = new ActionRoutine(routine);
    await routineStarter.startRoutine();
    return res.status(200).send('Starting routine');
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to start routine: ' + error.message);
  }
};
