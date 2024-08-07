import "./mongo/mongo";
import { randomUUID } from "crypto";
import { IRoutine, IDevice, insertDocument, IAction } from "./mongo/mongo";

let initDevice: IDevice[] = [
  {
    id: randomUUID(),
    topic: "sensor/button",
    name: "button",
    valueType: "binary",
    deviceType: "sensor",
    location: { x: 0, y: 0, z: 0 },
  },
  {
    id: randomUUID(),
    topic: "actuator/led",
    name: "led",
    valueType: "binary",
    deviceType: "actuator",
    location: { x: 0, y: 0, z: 0 },
  },
];

let actions: IAction[] = [
  {
    id: randomUUID(),
    actionType: "sensor",
    deviceId: initDevice[0].id,
    name: "button",
    description: "button pressed",
  },
  {
    id: randomUUID(),
    actionType: "actuator",
    deviceId: initDevice[1].id,
    name: "led actuater",
    description: "turn on led",
  },
];

let routine: IRoutine = {
  id: randomUUID(),
  name: "simple test routine",
  actionBlocks: actions.map((action) => action.id),
};

const smartHomeDevices = [
  {
    id: "a044df75-e67f-46dd-9db5-ea29c9dcdcd0",
    device: "温度センサー",
    description:
      "リビングルームの温度を監視し、最適な快適性を提供するために使用されます。",
  },
  {
    id: "d577159f-b4e4-40d8-a69e-cc7c3ef0b692",
    device: "スマート電球",
    description: "色温度と明るさを調整できるリビングルーム用のスマート電球。",
  },
  {
    id: "14cef1ec-de23-4fa0-b30e-b639daff1525",
    device: "ドアセンサー",
    description: "玄関ドアの開閉を検知し、セキュリティ通知を提供します。",
  },
  {
    id: "4b52c077-0273-4986-a08a-db4fd6ccd9c2",
    device: "スマートプラグ",
    description: "家電製品のオンオフを遠隔で制御できるスマートプラグ。",
  },
  {
    id: "f5d07fa2-dec6-407b-9823-d11029de3a27",
    device: "モーションセンサー",
    description:
      "部屋の動きを感知し、照明や警報をトリガーするためのモーションセンサー。",
  },
  {
    id: "6fa25ffb-0542-44b2-b705-f203c1141dde",
    device: "スマートサーモスタット",
    description: "家全体の温度を自動で調整し、エネルギー効率を最適化します。",
  },
  {
    id: "40956a17-790c-4d2a-9116-c66a4b3e65a8",
    device: "カメラ",
    description: "屋外のセキュリティカメラで、不審者の動きを記録します。",
  },
  {
    id: "63976ff7-8412-4837-9098-2f05f0aae94b",
    device: "煙検知器",
    description: "火災の兆候を検出し、警報を発するスマート煙検知器。",
  },
  {
    id: "9a0ec0ca-2e06-489c-8191-5cef083fb4f9",
    device: "スマートスピーカー",
    description: "音声コントロールと音楽再生機能を備えたスマートスピーカー。",
  },
  {
    id: "f0552613-9b70-4fc0-a630-b1bdc1c301d6",
    device: "ガレージドアオープナー",
    description:
      "ガレージドアを遠隔で開閉できるスマートガレージドアオープナー。",
  },
  {
    id: "af545f6f-a4f5-4478-971a-8adf7ba5fcc0",
    device: "スマートブラインド",
    description: "窓のブラインドを自動で開閉するシステム。",
  },
  {
    id: "2f1e4325-9094-4cdc-b20a-08c086973122",
    device: "空気質センサー",
    description: "室内の空気質を監視し、空気清浄機と連動するセンサー。",
  },
  {
    id: "e173f7f5-2f22-4477-b259-4015b5b48a9c",
    device: "スマートリモコン",
    description: "家電製品を一元管理するためのスマートリモコン。",
  },
  {
    id: "7a038927-c987-482e-b256-39d4a1c9348f",
    device: "スマートロック",
    description: "ドアの施錠と解錠を遠隔で管理できるスマートロック。",
  },
  {
    id: "d443ebfb-eb76-413c-a6ab-e2c46d0e0253",
    device: "スマートサイレン",
    description: "異常を検知すると警報を鳴らすスマートサイレン。",
  },
  {
    id: "66c34b5a-4f11-436c-b8c8-3cae297cc2f8",
    device: "スマート冷蔵庫",
    description: "食材の在庫管理やレシピ提案機能を備えたスマート冷蔵庫。",
  },
  {
    id: "4168f082-8088-4e8c-8cb2-f8c163f0c584",
    device: "漏水センサー",
    description: "水漏れを検知し、アラートを送信するセンサー。",
  },
  {
    id: "632fd116-49b1-4a96-b53f-cd5b6383859e",
    device: "照度センサー",
    description: "照明の自動調整のために部屋の明るさを測定するセンサー。",
  },
  {
    id: "a88e93d9-764d-48c5-8aaa-5ebad2d03a64",
    device: "スマートファン",
    description: "リモートで風量と回転速度を調整できるスマートファン。",
  },
  {
    id: "224bd11f-a9bd-42da-b7f1-7a2e41c99fa3",
    device: "スマート浄水器",
    description: "水質を監視し、浄水プロセスを最適化する浄水器。",
  },
];

let ids = `a044df75-e67f-46dd-9db5-ea29c9dcdcd0
d577159f-b4e4-40d8-a69e-cc7c3ef0b692
14cef1ec-de23-4fa0-b30e-b639daff1525
4b52c077-0273-4986-a08a-db4fd6ccd9c2
f5d07fa2-dec6-407b-9823-d11029de3a27
6fa25ffb-0542-44b2-b705-f203c1141dde
40956a17-790c-4d2a-9116-c66a4b3e65a8
63976ff7-8412-4837-9098-2f05f0aae94b
9a0ec0ca-2e06-489c-8191-5cef083fb4f9
f0552613-9b70-4fc0-a630-b1bdc1c301d6
af545f6f-a4f5-4478-971a-8adf7ba5fcc0
2f1e4325-9094-4cdc-b20a-08c086973122
e173f7f5-2f22-4477-b259-4015b5b48a9c
7a038927-c987-482e-b256-39d4a1c9348f
d443ebfb-eb76-413c-a6ab-e2c46d0e0253
66c34b5a-4f11-436c-b8c8-3cae297cc2f8
4168f082-8088-4e8c-8cb2-f8c163f0c584
632fd116-49b1-4a96-b53f-cd5b6383859e
a88e93d9-764d-48c5-8aaa-5ebad2d03a64
224bd11f-a9bd-42da-b7f1-7a2e41c99fa3`.split("\n");

let count = 0;
for (let i = 0; i < smartHomeDevices.length; i++) {
  if (ids.includes(smartHomeDevices[i].id)) {
    count++;
  }
}
console.log(count);
