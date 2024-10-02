import { Db, MongoClient } from 'mongodb';
import {
  IDBDeviceBlock,
  IDeviceData,
  IRoutine,
  IRoutineData,
} from '@/types/ActionBlockInterfaces';
import Debugger from '@debugger/Debugger';
import { ActionType } from '@/types/ActionType';

export class MongoDB {
  public static instance: MongoDB = new MongoDB();

  private client: MongoClient;
  private db: Db;

  private COL_ROUTINE = 'routine';
  private COL_ACTION = 'action';
  private COL_DEVICE = 'device';

  constructor() {
    // MongoDB接続URI
    const uri = 'mongodb://mongodb:27017'; // 適切なURIに置き換えてください
    this.client = new MongoClient(uri);
    this.client
      .connect()
      .then(() => {
        console.log('[MONGODB] Connected to MongoDB');
      })
      .catch((err) => {
        console.error('[MONGODB] Failed to connect to MongoDB', err);
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

  // IDからルーチンを取得するメソッド
  getRoutine = async (routineId: string): Promise<IRoutineData | null> => {
    try {
      const routineData = await this.db
        .collection(this.COL_ROUTINE)
        .findOne({ id: routineId });
      if (routineData === null) return null; // データが見つからない場合
      const { _id, ...routine } = routineData; // _idを除外
      return routine as IRoutineData; // ルーチンデータを返す
    } catch (err) {
      console.error('Error fetching routine:', err);
      throw err; // エラーを再スロー
    }
  };

  getAllRoutine = async (): Promise<IRoutineData[]> => {
    try {
      // コレクションからデータを取得
      const routineCursor = await this.db.collection(this.COL_ROUTINE).find();

      // カーソルからデータを配列に変換
      const routineDatas = await routineCursor.toArray();
      const dataWithoutId = routineDatas.map((routineData) => {
        const { _id, ...data } = routineData;
        return data;
      });
      return dataWithoutId as IRoutineData[]; // データを返す
    } catch (err) {
      console.error('Error fetching routines:', err);
      throw err; // エラーを再スローして呼び出し元で処理できるようにする
    }
  };

  // デバイスデータを取得するメソッド
  getDevice = async (deviceId: string): Promise<IDeviceData | null> => {
    try {
      const deviceDatas = await this.db
        .collection(this.COL_DEVICE)
        .findOne({ device_id: deviceId });

      if (deviceDatas === null) return null; // データが見つからない場合
      const { _id, ...deviceData } = deviceDatas; // _idを除外
      return deviceData as IDeviceData; // デバイスデータを返す
    } catch (err) {
      console.error('Error fetching device data:', err);
      throw err; // エラーを再スロー
    }
  };

  // すべてのデバイスを取得するメソッド
  getAllDevices = async (): Promise<IDeviceData[]> => {
    try {
      const deviceCursor = await this.db.collection(this.COL_DEVICE).find();
      const deviceDatas = await deviceCursor.toArray(); // カーソルを配列に変換
      const dataWithoutId = deviceDatas.map((deviceData) => {
        const { _id, ...data } = deviceData;
        return data;
      });
      return dataWithoutId as IDeviceData[]; // デバイスデータの配列を返す
    } catch (err) {
      console.error('Error fetching all devices:', err);
      throw err; // エラーを再スロー
    }
  };

  // アクションデータを取得するメソッド
  getAction = async (actionId?: string): Promise<object | null> => {
    try {
      const actionDatas = await this.db
        .collection(this.COL_ACTION)
        .findOne({ id: actionId });
      if (actionDatas === null) return null; // データが見つからない場合
      const { _id, ...actionData } = actionDatas; // _idを除外
      return actionData; // アクションデータを返す
    } catch (err) {
      console.error('Error fetching action data:', err);
      throw err; // エラーを再スロー
    }
  };

  // すべてのアクションを取得するメソッド
  getAllActions = async (): Promise<object[]> => {
    try {
      const actionCursor = await this.db.collection(this.COL_ACTION).find();
      let actionDatas = await actionCursor.toArray(); // カーソルを配列に変換\
      let dataWithoutId = actionDatas.map((actionData) => {
        const { _id, ...data } = actionData;
        return data;
      });
      return dataWithoutId; // アクションデータの配列を返す
    } catch (err) {
      console.error('Error fetching all actions:', err);
      throw err; // エラーを再スロー
    }
  };

  // デバイスを追加するメソッド
  addDevices = async (devices: IDeviceData | IDeviceData[]): Promise<void> => {
    try {
      const deviceArray = Array.isArray(devices) ? devices : [devices]; // 配列に変換
      const result = await this.db
        .collection(this.COL_DEVICE)
        .insertMany(deviceArray);
      console.log(`${result.insertedCount} device(s) added successfully.`);
    } catch (err) {
      console.error('Error adding devices:', err);
      throw err; // エラーを再スロー
    }
  };

  // アクションを追加するメソッド
  addActions = async (actions: object | object[]): Promise<void> => {
    try {
      const actionArray = Array.isArray(actions) ? actions : [actions]; // 配列に変換
      const result = await this.db
        .collection(this.COL_ACTION)
        .insertMany(actionArray);
      console.log(`${result.insertedCount} action(s) added successfully.`);
    } catch (err) {
      console.error('Error adding actions:', err);
      throw err; // エラーを再スロー
    }
  };

  // ルーチンを追加するメソッド
  addRoutines = async (
    routines: IRoutineData | IRoutineData[],
  ): Promise<void> => {
    try {
      const routineArray = Array.isArray(routines) ? routines : [routines]; // 配列に変換
      const result = await this.db
        .collection(this.COL_ROUTINE)
        .insertMany(routineArray);
      console.log(`${result.insertedCount} routine(s) added successfully.`);
    } catch (err) {
      console.error('Error adding routines:', err);
      throw err; // エラーを再スロー
    }
  };

  deleteRoutine = async (routineId: string): Promise<void> => {
    try {
      const result = await this.db
        .collection(this.COL_ROUTINE)
        .deleteOne({ id: routineId });
      console.log(`${result.deletedCount} routine(s) deleted successfully.`);
    } catch (err) {
      console.error('Error deleting routine:', err);
      throw err; // エラーを再スロー
    }
  };

  // デバイスを削除するメソッド
  deleteDevice = async (deviceId: string): Promise<void> => {
    try {
      const result = await this.db
        .collection(this.COL_DEVICE)
        .deleteOne({ device_id: deviceId });
      console.log(`${result.deletedCount} device(s) deleted successfully.`);
    } catch (err) {
      console.error('Error deleting device:', err);
      throw err; // エラーを再スロー
    }
  };

  // アクションを削除するメソッド
  deleteAction = async (actionId: string): Promise<void> => {
    try {
      const result = await this.db
        .collection(this.COL_ACTION)
        .deleteOne({ id: actionId });
      console.log(`${result.deletedCount} action(s) deleted successfully.`);
    } catch (err) {
      console.error('Error deleting action:', err);
      throw err; // エラーを再スロー
    }
  };

  // ルーチンを更新するメソッド
  updateRoutine = async (routineData: Partial<IRoutineData>): Promise<void> => {
    try {
      const routineId = routineData.id;
      const result = await this.db
        .collection(this.COL_ROUTINE)
        .updateOne({ id: routineId }, { $set: routineData });
      console.log(
        `${result.matchedCount} routine(s) matched, ${result.modifiedCount} routine(s) updated successfully.`,
      );
    } catch (err) {
      console.error('Error updating routine:', err);
      throw err; // エラーを再スロー
    }
  };

  // アクションを更新するメソッド
  updateAction = async (actionData: Partial<any>): Promise<void> => {
    try {
      const actionId = actionData['id'];
      const result = await this.db
        .collection(this.COL_ACTION)
        .updateOne({ id: actionId }, { $set: actionData });
      console.log(
        `${result.matchedCount} action(s) matched, ${result.modifiedCount} action(s) updated successfully.`,
      );
    } catch (err) {
      console.error('Error updating action:', err);
      throw err; // エラーを再スロー
    }
  };

  // デバイスを更新するメソッド
  updateDevice = async (deviceData: Partial<IDeviceData>): Promise<void> => {
    try {
      const deviceId = deviceData.device_id;
      const result = await this.db
        .collection(this.COL_DEVICE)
        .updateOne({ device_id: deviceId }, { $set: deviceData });
      console.log(
        `${result.matchedCount} device(s) matched, ${result.modifiedCount} device(s) updated successfully.`,
      );
    } catch (err) {
      console.error('Error updating device:', err);
      throw err; // エラーを再スロー
    }
  };
}
