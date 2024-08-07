import { MongoClient } from "mongodb";
import { randomUUID } from "crypto";

export interface IRoutine {
  id: string;
  name: string;
  actionBlocks: string[];
}

export interface IAction {
  id: string;
  actionType: string;
  deviceId?: string;
  name: string;
  description: string;
}

export interface IDevice {
  id: string;
  topic: string;
  name: string;
  deviceType: string;
  valueType: string;
  location: {
    x: number;
    y: number;
    z: number;
  };
}

export async function insertDocument(collection_name: string, initDevice: any) {
  const url = "mongodb://localhost:27017"; // MongoDBサーバーのURL
  const dbName = "InteractiveSmartHome"; // データベース名
  const client = new MongoClient(url);

  try {
    // MongoDBに接続
    await client.connect();
    console.log("Connected successfully to server");

    const db = client.db(dbName);
    const collection = db.collection(collection_name); // コレクション名

    if (Array.isArray(initDevice)) {
      for (let i = 0; i < initDevice.length; i++) {
        let result = await collection.insertOne(initDevice[i]);
        console.log("Inserted document:", result.insertedId);
      }
    } else {
      let result = await collection.insertOne(initDevice);
      console.log("Inserted document:", result.insertedId);
    }
  } catch (err) {
    console.error("Error occurred while inserting document:", err);
  } finally {
    // クライアントを閉じる
    await client.close();
  }
}
