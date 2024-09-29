import { MongoClient, Db } from 'mongodb';
import { ActionType, DeviceType } from '../types/ActionType';
import { Action } from 'rxjs/internal/scheduler/Action';

class DataInserter {
  client: MongoClient;
  db: Db;

  COL_ROUTINE = 'routine';
  COL_ACTION = 'action';
  COL_DEVICE = 'device';

  routinesData = [
    {
      id: 'routine-1',
      name: 'Routine 1',
      action_routine: [
        {
          first: true,
          current_block_id: 'thermo-1',
          next_block_id: 'timer-1',
        },
        {
          current_block_id: 'timer-1',
          next_block_id: 'light-1',
        },
        {
          last: true,
          current_block_id: 'light-1',
          next_block_id: '',
        },
      ],
    },
    {
      id: 'routine-101',
      name: 'Routine 1',
      action_routine: [
        {
          first: true,
          current_block_id: 'thermo-101',
          next_block_id: 'comparator-101',
        },
        {
          current_block_id: 'comparator-101',
          next_block_id: 'timer-101',
        },
        {
          current_block_id: 'timer-101',
          next_block_id: 'light-101',
        },
        {
          last: true,
          current_block_id: 'light-101',
          next_block_id: '',
        },
      ],
    },
    {
      id: 'routine-102',
      name: 'Routine 2',
      action_routine: [
        {
          first: true,
          current_block_id: 'thermo-101',
          next_block_id: 'comparator-101',
        },
        {
          current_block_id: 'comparator-101',
          next_block_id: 'timer-101',
        },
        {
          current_block_id: 'comparator-101',
          next_block_id: 'light-101',
        },
        {
          last: true,
          current_block_id: 'light-101',
          next_block_id: '',
        },
      ],
    },
  ];

  actionBlocks = [
    {
      id: 'thermo-1',
      name: 'Thermometer Sensor',
      description: 'Thermometer Sensor',
      action_type: ActionType.Device,
      device_type: DeviceType.Sensor_Thermometer,
      device_data_id: 'thermo1',
    },
    {
      id: 'timer-1',
      name: 'Timer',
      description: 'Timer',
      action_type: ActionType.Logic_Timer,
      waitTime: 5000,
    },
    {
      id: 'light-1',
      name: 'Light Actuator',
      description: 'Light Actuator',
      action_type: ActionType.Device,
      device_type: DeviceType.Actuator_Light,
      device_data_id: 'device-2',
    },
    {
      id: 'comparator-101',
      name: 'Comparator',
      description: 'Comparator',
      action_type: ActionType.Logic_Comparator,
      threshold: 30,
    },
    {
      id: 'timer-101',
      name: 'Timer',
      description: 'Timer',
      action_type: ActionType.Logic_Timer,
      waitTime: 5000,
    },
    {
      id: 'light-101',
      name: 'Light Actuator',
      description: 'Light Actuator',
      action_type: ActionType.Device,
      device_type: DeviceType.Actuator_Light,
      device_data_id: 'device-201',
    },
  ];

  devicesData = [
    {
      device_id: 'thermo1',
      device_name: 'thermo living',
      device_type: DeviceType.Sensor_Thermometer,
      mqtt_topic: 'thermo-sensor',
      device_location: {
        x: 0,
        y: 0,
        z: 0,
      },
    },
    {
      device_id: 'device-2',
      device_name: 'Light Actuator',
      device_type: DeviceType.Actuator_Light,
      mqtt_topic: 'light-actuator',
      device_location: {
        x: 0,
        y: 0,
        z: 0,
      },
    },
    {
      device_id: 'device-201',
      device_name: 'Light Actuator',
      device_type: 'light',
      mqtt_topic: 'light-actuator',
      device_location: {
        x: 0,
        y: 0,
        z: 0,
      },
    },
  ];

  constructor() {
    // MongoDB接続URI
    const uri = 'mongodb://mongodb:27017'; // 適切なURIに置き換えてください
    this.client = new MongoClient(uri);

    this.client
      .connect()
      .then(() => {
        console.log('[MONGODB] Connected to MongoDB');
        this.db = this.client.db('InteractiveSmartHome');
        this.insertData(); // データ挿入関数を呼び出す
      })
      .catch((err) => {
        console.error('[MONGODB] Failed to connect to MongoDB', err);
      });
  }

  async insertData() {
    try {
      // コレクションにデータを挿入
      await this.db.collection(this.COL_ROUTINE).insertMany(this.routinesData);
      await this.db.collection(this.COL_ACTION).insertMany(this.actionBlocks);
      await this.db.collection(this.COL_DEVICE).insertMany(this.devicesData);

      console.log('Data inserted successfully');
    } catch (err) {
      console.error('Error inserting data:', err);
    } finally {
      await this.client.close();
    }
  }
}

// クラスのインスタンスを作成して実行
new DataInserter();
