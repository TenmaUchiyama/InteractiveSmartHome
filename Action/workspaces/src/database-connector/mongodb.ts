import { Db, MongoClient } from 'mongodb';
import { DeviceData, RoutineData, ActionSequence } from '../types/DBTypes';
import { ActionBlockType } from '../types/ActionBlockTypes';

export class MongoDB {
  public static instance: MongoDB = new MongoDB();

  private client: MongoClient;
  private db: Db;

  private COL_ROUTINE = 'routine';
  private COL_ACTION = 'action';
  private COL_DEVICE = 'device';

  private constructor() {
    // MongoDB接続URI
    const uri = 'mongodb://mongodb:27017'; // 適切なURIに置き換えてください
    this.client = new MongoClient(uri);
    this.client
      .connect()
      .then(() => {
        console.log('connected to MongoDB');
      })
      .catch((err) => {
        console.error('Failed to connect to MongoDB', err);
      });

    this.db = this.client.db('InteractiveSmartHome');
  }

  public static getInstance(): MongoDB {
    if (!MongoDB.instance) {
      console.log('Creating new [MongoDB] instance');
      MongoDB.instance = new MongoDB();
    }
    return MongoDB.instance;
  }

  getWholeData = async () => {
    console.log('connected to MongoDB');
    const routineCollection = this.db.collection('routine');
    const actionCollection = this.db.collection('action');
    const deviceCollection = this.db.collection('device');

    // データの取得
    const routines: RoutineData[] = (
      await routineCollection.find().toArray()
    ).map((data: any) => {
      let output: RoutineData = {
        id: data.id, // MongoDBの_idフィールドを文字列に変換
        name: data.name,
        actionBlocks: data.actionBlocks,
      };

      return output;
    });
    const actions = (await actionCollection.find().toArray()).map(
      (data: any) => {
        let output: ActionBlockType = {
          id: data.id,
          name: data.name,
          description: data.description,
        };

        return output;
      },
    );
    const devices = (await deviceCollection.find().toArray()).map(
      (data: any) => {
        let output: DeviceData = {
          id: data.id,
          topic: data.topic,
          name: data.name,
          deviceType: data.deviceType,
          valueType: data.valueType,
          location: {
            x: data.location.x,
            y: data.location.y,
            z: data.location.z,
          },
        };
        return output;
      },
    );

    return {
      devices: devices,
      actions: actions,
      routines: routines,
    };
  };
  /**
   * Get Device Data from Action
   */
  getRoutineData = async (routineId: string) => {
    let routineDatas = await this.db
      .collection(this.COL_ROUTINE)
      .findOne({ id: routineId });
    if (routineDatas === null) return null;
    let { _id, ...routineData } = routineDatas;
    return routineData;
  };

  /**
   * Get Action Data from MongoDB
   */
  getActionData = async (actionId: string) => {
    let actionDatas = await this.db
      .collection(this.COL_ACTION)
      .findOne({ id: actionId });
    if (actionDatas === null) return null;
    let { _id, ...actionData } = actionDatas;
    return actionData;
  };

  /**
   * Get Routine Data from MongoDB
   */
  getDeviceData = async (deviceId: string) => {
    let deviceDatas = await this.db
      .collection(this.COL_DEVICE)
      .findOne({ id: deviceId });
    if (deviceDatas === null) return null;
    let { _id, ...deviceData } = deviceDatas;
    return deviceData;
  };

  /**
   * Get Actions from Routine
   * @param routineId
   * @returns
   */
  getActionsInRoutine = async (routineId: string) => {
    let routine = await this.db
      .collection(this.COL_ROUTINE)
      .findOne({ id: routineId });

    if (routine === null) return null;
    let actionIds = routine.actionBlocks.map(
      (actionData: ActionSequence) => actionData.actionId,
    );
    let actionDatas = await this.db
      .collection(this.COL_ACTION)
      .find({ id: { $in: actionIds } })
      .toArray();

    let actions = actionDatas.map(({ _id, ...actionData }) => actionData);
    return actions;
  };
}
