import { Db, MongoClient } from "mongodb";
import {
  IDBDeviceBlock,
  IDeviceData,
  IRoutine,
  IRoutineData,
} from "@/types/ActionBlockInterfaces";
import Debugger from "@debugger/Debugger";
import { ActionType } from "@/types/ActionType";
import { deleteActionApi } from "@/server/routes/action/actionController";
import { IDBEdge, IDBNode } from "@/types/FlowNodeType";
import * as dotenv from "dotenv";
const envFile =
  process.env.NODE_ENV === "docker" ? ".env.docker" : ".env.development";
dotenv.config({ path: envFile });
export class MongoDB {
  public static instance: MongoDB = new MongoDB();

  private client: MongoClient;
  private db: Db;
  private DATABASE = "DebugInteractionSmartHome";
  private COL_ROUTINE = "routine";
  private COL_ACTION = "action";
  private COL_DEVICE = "device";

  private COL_NODE = "flow-node";
  private COL_EDGE = "flow-edge";

  private upsert: boolean = false;

  constructor() {
    // MongoDB接続URI
    // "mongodb://mongodb:27017" はDockerコンテナ内でのMongoDBのURI
    console.log("ENV:" + process.env.NODE_ENV);
    console.log("MONGO_URL: " + process.env.MONGO_URL);
    const uri = process.env.MONGO_URL || "mongodb://localhost:27017";
    this.client = new MongoClient(uri);
    this.client
      .connect()
      .then(() => {
        console.log("[MONGODB] Connected to MongoDB");
      })
      .catch((err) => {
        console.error("[MONGODB] Failed to connect to MongoDB", err);
      });

    this.db = this.client.db(this.DATABASE);
  }
  public static getInstance(): MongoDB {
    if (!MongoDB.instance) {
      console.log("Creating new [MongoDB] instance");
      MongoDB.instance = new MongoDB();
    }
    return MongoDB.instance;
  }

  // ==================== Cloud ==========================

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
      console.error("Error fetching routine:", err);
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
      console.error("Error fetching routines:", err);
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
      console.error("Error fetching device data:", err);
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
      console.error("Error fetching all devices:", err);
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
      console.error("Error fetching action data:", err);
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
      console.error("Error fetching all actions:", err);
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
      console.error("Error adding devices:", err);
      throw err; // エラーを再スロー
    }
  };

  // アクションを追加するメソッド
  addActions = async (actions: object | object[]): Promise<void> => {
    try {
      const actionArray = Array.isArray(actions) ? actions : [actions];

      if (actionArray.length === 1) {
        // 単一のオブジェクトがある場合は insertOne を使用
        await this.db.collection(this.COL_ACTION).insertOne(actionArray[0]);
      } else if (actionArray.length > 1) {
        // 配列の場合は insertMany を使用
        await this.db.collection(this.COL_ACTION).insertMany(actionArray);
      }

      console.log(`${actionArray.length} action(s) processed successfully.`);
    } catch (err) {
      console.error("Error adding/updating actions:", err);
      throw err;
    }
  };

  // ルーチンを追加するメソッド
  addRoutines = async (
    routines: IRoutineData | IRoutineData[]
  ): Promise<void> => {
    try {
      const routineArray = Array.isArray(routines) ? routines : [routines]; // 配列に変換
      const result = await this.db
        .collection(this.COL_ROUTINE)
        .insertMany(routineArray);
      console.log(`${result.insertedCount} routine(s) added successfully.`);
    } catch (err) {
      console.error("Error adding routines:", err);
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
      console.error("Error deleting routine:", err);
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
      console.error("Error deleting device:", err);
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
      console.error("Error deleting action:", err);
      throw err; // エラーを再スロー
    }
  };

  deleteActions = async (actionIds: string[]): Promise<void> => {
    try {
      const bulkOps = actionIds.map((actionId) => {
        return {
          deleteOne: {
            filter: { id: actionId },
            upsert: this.upsert,
          },
        };
      });

      if (bulkOps.length > 0) {
        const result = await this.db
          .collection(this.COL_ACTION)
          .bulkWrite(bulkOps);
        console.log(`${result.deletedCount} action(s) deleted successfully.`);
      } else {
        console.log("No actions to delete.");
      }
    } catch (err) {
      console.error("Error deleting actions:", err);
      throw err; // エラーを再スロー
    }
  };

  // ルーチンを更新するメソッド
  updateRoutine = async (routineData: Partial<IRoutineData>): Promise<void> => {
    try {
      const routineId = routineData.id;
      const result = await this.db
        .collection(this.COL_ROUTINE)
        .replaceOne({ id: routineId }, routineData, { upsert: this.upsert });
      console.log(
        `${result.matchedCount} routine(s) matched, ${result.modifiedCount} routine(s) updated successfully.`
      );
    } catch (err) {
      console.error("Error updating routine:", err);
      throw err; // エラーを再スロー
    }
  };

  updateRoutineById = async (
    routine_id: string,
    actionRoutineData: IRoutine[]
  ): Promise<void> => {
    try {
      console.log("Receive Data from updateRoutineById");
      const result = await this.db
        .collection(this.COL_ROUTINE)
        .updateOne(
          { id: routine_id },
          { $set: { action_routine: actionRoutineData } }
        );

      console.log(
        `${result.matchedCount} routine(s) matched, ${result.modifiedCount} routine(s) updated successfully.`
      );
    } catch (err) {
      console.error("Error updating action_routine:", err);
      throw err;
    }
  };

  // アクションを更新するメソッド
  updateAction = async (actionData: any[]): Promise<void> => {
    try {
      const collection = this.db.collection(this.COL_ACTION); // コレクション名は適宜変更してください

      // bulkWrite 用の操作配列を作成
      const bulkOps = actionData.map((action) => {
        return {
          replaceOne: {
            filter: { id: action.id }, // 更新対象のフィルタ
            replacement: action, // 更新内容z
            upsert: this.upsert, // ドキュメントが存在しない場合は挿入しない
          },
        };
      });

      if (bulkOps.length > 0) {
        const result = await collection.bulkWrite(bulkOps);
        console.log("Bulk update result:", result);
      } else {
        console.log("No operations to perform.");
      }
    } catch (error) {
      console.error("Error performing bulk update:", error);
      throw error; // 必要に応じてエラーを再スロー
    }
  };

  updateSingleAction = async (actionData: any): Promise<void> => {
    try {
      const result = await this.db
        .collection(this.COL_ACTION)
        .replaceOne({ id: actionData.id }, actionData);
      console.log(
        `${result.matchedCount} action(s) matched, ${result.modifiedCount} action(s) updated successfully.`
      );
    } catch (err) {
      console.error("Error updating action:", err);
      throw err; // エラーを再スロー
    }
  };
  // デバイスを更新するメソッド
  updateDevice = async (deviceData: Partial<IDeviceData>): Promise<void> => {
    try {
      const deviceId = deviceData.device_id;
      const result = await this.db
        .collection(this.COL_DEVICE)
        .replaceOne({ device_id: deviceId }, deviceData);
      console.log(
        `${result.matchedCount} device(s) matched, ${result.modifiedCount} device(s) updated successfully.`
      );
    } catch (err) {
      console.error("Error updating device:", err);
      throw err; // エラーを再スロー
    }
  };

  // ==================== Flow ===============================
  getAllNode = async (): Promise<IDBNode[]> => {
    try {
      const nodeCursor = await this.db.collection(this.COL_NODE).find();
      const nodeDatas = await nodeCursor.toArray();
      const dataWithoutId = nodeDatas.map((nodeData) => {
        const { _id, ...data } = nodeData;
        return data;
      });
      return dataWithoutId as IDBNode[];
    } catch (err) {
      console.error("Error fetching all nodes:", err);
    }
  };
  getNodes = async (nodeIds: string[]): Promise<IDBNode[]> => {
    try {
      const nodesCursor = await this.db
        .collection(this.COL_NODE)
        .find({ id: { $in: nodeIds } }); // nodeIdsの配列を使って条件を指定

      const nodesArray = await nodesCursor.toArray(); // 結果を配列に変換

      const sanitizedNodes = nodesArray.map(
        ({ _id, ...node }) => node as IDBNode
      );

      return sanitizedNodes;
    } catch (err) {
      console.error("Error fetching nodes:", err);
      return [];
    }
  };

  addNode = async (node: IDBNode | IDBNode[]): Promise<boolean> => {
    try {
      const nodeArr = Array.isArray(node) ? node : [node];
      await this.db.collection(this.COL_NODE).insertMany(nodeArr);
      return true;
    } catch (err) {
      console.error("Error adding node:", err);
      throw err;
    }
  };

  updateNodes = async (nodes: IDBNode[]): Promise<void> => {
    try {
      const collection = this.db.collection(this.COL_NODE); // コレクション名は適宜変更してください

      // bulkWrite 用の操作配列を作成
      const bulkOps = nodes.map((node) => {
        return {
          replaceOne: {
            filter: { id: node.id }, // 更新対象のフィルタ
            replacement: node, // 更新内容
            upsert: this.upsert, // ドキュメントが存在しない場合は挿入しない
          },
        };
      });

      Debugger.getInstance().debugLog(
        "MongoDB",
        "updateNodes",
        "Bulk update operations:" + JSON.stringify(bulkOps)
      );
      if (bulkOps.length > 0) {
        const result = await collection.bulkWrite(bulkOps);
        console.log("Bulk update result:", result);
      } else {
        console.log("No operations to perform.");
      }
    } catch (error) {
      console.error("Error performing bulk update:", error);
      throw error; // 必要に応じてエラーを再スロー
    }
  };

  deleteNodes = async (node_ids: string[]): Promise<void> => {
    try {
      const collection = this.db.collection(this.COL_NODE); // コレクション名は適宜変更してください

      // bulkWrite 用の操作配列を作成
      const bulkOps = node_ids.map((node_id) => {
        return {
          deleteOne: {
            filter: { id: node_id }, // 削除対象のフィルタ
          },
        };
      });

      Debugger.getInstance().debugLog(
        "MongoDB",
        "deleteNodes",
        "Bulk delete operations:" + JSON.stringify(bulkOps)
      );
      if (bulkOps.length > 0) {
        const result = await collection.bulkWrite(bulkOps);
        console.log("Bulk delete result:", result);
      } else {
        console.log("No operations to perform.");
      }
    } catch (error) {
      console.error("Error performing bulk delete:", error);
      throw error; // 必要に応じてエラーを再スロー
    }
  };

  getAllEdges = async (): Promise<IDBNode[]> => {
    try {
      const edgesData = await this.db
        .collection(this.COL_EDGE)
        .find()
        .toArray();
      return edgesData.map(({ _id, ...edge }) => edge) as IDBNode[]; // _idを除外
    } catch (err) {
      console.error("Error fetching edges:", err);
      throw err; //
    }
  };

  getEdge = async (edgeId: string): Promise<IDBNode | null> => {
    try {
      const edgeData = await this.db
        .collection(this.COL_EDGE)
        .findOne({ id: edgeId });
      if (edgeData === null) return null; // データが見つからない場合
      const { _id, ...edge } = edgeData; // _idを除外
      return edge as IDBNode; // エッジデータを返す
    } catch (err) {
      console.error("Error fetching edge:", err);
      throw err; // エラーを再スロー
    }
  };

  addEdge = async (edge: any): Promise<void> => {
    try {
      const edgeArray = Array.isArray(edge) ? edge : [edge];
      const result = await this.db
        .collection(this.COL_EDGE)
        .insertMany(edgeArray);
      console.log(`edge(s) added successfully.`);
    } catch (err) {
      console.error("Error adding edge:", err);
    }
  };

  updateEdge = async (edge: IDBEdge): Promise<void> => {
    try {
      const edgeId = edge.id;
      const result = await this.db
        .collection(this.COL_EDGE)
        .replaceOne({ id: edgeId }, edge, { upsert: this.upsert });
      console.log(
        `${result.matchedCount} edge(s) matched, ${result.modifiedCount} edge(s) updated successfully.`
      );
    } catch (err) {
      console.error("Error updating edge:", err);
    }
  };

  deleteEdge = async (edgeId: string): Promise<void> => {
    try {
      const result = await this.db
        .collection(this.COL_EDGE)
        .deleteOne({ id: edgeId });
      console.log(`${result.deletedCount} edge(s) deleted successfully.`);
    } catch (err) {
      console.error("Error deleting edge:", err);
    }
  };
}
