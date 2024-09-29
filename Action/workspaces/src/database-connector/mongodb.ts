import { Db, MongoClient } from 'mongodb';
import {
  IDBDeviceData,
  IDeviceData,
  IRoutine,
  IRoutineData,
} from '../types/ActionBlockInterfaces';
import { ActionType } from '../types/ActionType';

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
        console.log('[MONGODB] connected to MongoDB');
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
  getRoutine = async (routineId: string): Promise<IRoutineData> => {
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
  getDeviceData = async (deviceId: string): Promise<IDeviceData> => {
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
  getActionData = async (actionId?: string): Promise<object> => {
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
}
