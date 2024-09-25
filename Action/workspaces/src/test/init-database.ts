import { MongoClient } from 'mongodb';
import {
  RoutineData,
  DeviceData,
  TimerLogicType,
  ActionSequence,
} from 'src/types/DBTypes';

export async function fetchData() {
  // MongoDB接続URI
  const uri = 'mongodb://mongodb:27017'; // 適切なURIに置き換えてください
  const client = new MongoClient(uri);
  const database = client.db('InteractiveSmartHome');
  try {
    client.connect();
    const routineCollection = database.collection('routine');
    const actionCollection = database.collection('action');

    let timers: TimerLogicType[] = [];
    for (let i = 1; i < 4; i++) {
      let timerLogic: TimerLogicType = {
        id: i.toString(),
        name: `Timer Logic ${i}`,
        description: `Timer Logic ${i} description`,
        time: (1000 * i).toString(),
      };

      timers.push(timerLogic);
    }

    let routines: RoutineData = {
      id: '1',
      name: 'Routine 1',
      actionBlocks: timers.map((timer) => {
        let actionSequence: ActionSequence = {
          first: timer.id === '1',
          actionId: timer.id,
          nextActionId:
            timers.find((t) => t.id === (parseInt(timer.id) + 1).toString())
              ?.id ?? null,
        };

        return actionSequence;
      }),
    };

    const routineResult = await routineCollection.insertOne(routines);
    const actionResult = await actionCollection.insertMany(timers);

    console.log(routineResult);
    console.log(actionResult);
  } catch (err) {
    console.error(err);
  }
}

fetchData();
