import { MongoClient } from 'mongodb';
import { IAction, IDevice, IRoutine } from 'src/types/ActionTypes';

export async function fetchData() {
  // MongoDB接続URI
  const uri = 'mongodb://mongodb:27017'; // 適切なURIに置き換えてください
  const client = new MongoClient(uri);

  try {
    // データベースに接続
    await client.connect();
    console.log("connected to MongoDB");
    // データベースとコレクションの指定
    const database = client.db('InteractiveSmartHome'); // データベース名に置き換え
    const routineCollection = database.collection('routine');
    const actionCollection = database.collection('action');
    const deviceCollection = database.collection('device');

    // データの取得
    const routines: IRoutine[] = (await routineCollection.find().toArray()).map((data: any) => {
        let output: IRoutine = {
            id: data.id,  // MongoDBの_idフィールドを文字列に変換
            name: data.name, 
            actionBlocks: data.actionBlocks
        };
    
        return output;
    });
    const actions = (await actionCollection.find().toArray()).map((data: any) => {
        let output : IAction = {
            id: data.id,
            actionType: data.actionType,
            deviceId: data.deviceId,
            name: data.name,
            description: data.description
             }

        return output;
        
    });
    const devices =( await deviceCollection.find().toArray()).map((data:any) => {
    let output : IDevice = {
        id : data.id,
        topic : data.topic,
        name : data.name,
        deviceType : data.deviceType,
        valueType : data.valueType,
        location : {
            x : data.location.x,
            y : data.location.y,
            z : data.location.z
        },
        
        
    }
    return output; 
    });

    return {
        devices : devices,
        actions : actions,
        routines : routines

    }
  } finally {
  
    // クライアントのクローズ
    await client.close();
  }
}

