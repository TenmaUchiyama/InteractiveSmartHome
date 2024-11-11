const { MongoClient } = require("mongodb");
const fs = require("fs");
const path = require("path");

const uri = "mongodb://localhost:27017"; // MongoDBの接続URI
const dbName = "DebugInteractionSmartHome"; // 使用するデータベース名

// JSONファイルのパスと対応するコレクション名
const filesAndCollections = [
  {
    filePath: "./DebugInteractiveSmartHome.action.json",
    collectionName: "action",
  },
  {
    filePath: "./DebugInteractiveSmartHome.device.json",
    collectionName: "device",
  },
  {
    filePath: "./DebugInteractiveSmartHome.flow-edge.json",
    collectionName: "flow-edge",
  },
  {
    filePath: "./DebugInteractiveSmartHome.flow-node.json",
    collectionName: "flow-node",
  },
  {
    filePath: "./DebugInteractiveSmartHome.routine.json",
    collectionName: "routine",
  },
];

function removeOidFields(data) {
  if (Array.isArray(data)) {
    return data.map((item) => removeOidFields(item));
  } else if (data && typeof data === "object") {
    for (const key in data) {
      if (data[key] && typeof data[key] === "object" && "$oid" in data[key]) {
        delete data[key]; // $oidフィールドを削除
      } else {
        data[key] = removeOidFields(data[key]);
      }
    }
  }
  return data;
}

async function importJsonToMongoDB() {
  const client = new MongoClient(uri);

  try {
    await client.connect();
    console.log("MongoDBに接続しました");

    const db = client.db(dbName);

    for (const { filePath, collectionName } of filesAndCollections) {
      const absolutePath = path.resolve(__dirname, filePath);

      // JSONファイルを読み込み
      const fileData = fs.readFileSync(absolutePath, "utf8");
      let jsonData = JSON.parse(fileData);

      // $oidフィールドを削除
      jsonData = removeOidFields(jsonData);

      // データをコレクションに挿入
      const collection = db.collection(collectionName);
      if (Array.isArray(jsonData)) {
        await collection.insertMany(jsonData);
      } else {
        await collection.insertOne(jsonData);
      }

      console.log(
        `${collectionName}コレクションにデータを追加しました: ${filePath}`
      );
    }

    console.log("全てのJSONファイルのインポートが完了しました。");
  } catch (error) {
    console.error("エラーが発生しました:", error);
  } finally {
    await client.close();
    console.log("MongoDBとの接続を閉じました");
  }
}

// スクリプトを実行
importJsonToMongoDB();
