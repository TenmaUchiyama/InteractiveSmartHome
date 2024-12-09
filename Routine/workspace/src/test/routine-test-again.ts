import path from "path";
import fs from "fs";
import { Subject } from "rxjs";
import MqttBridge from "../device-bridge/MqttBridge";

interface IDeviceData {
  device_id: string;
  device_name: string;
  device_type: string;
  mqtt_topic: string;
  device_position: {
    x: number;
    y: number;
    z: number;
  };
}

interface IActionBlock {
  id: string;
  name: string;
  description: string;
  action_type: string;
}

interface IDeviceBlock extends IActionBlock {
  device_data: IDeviceData;
}

interface IComparatorLogicBlock extends IActionBlock {
  action_type: "comparator";
  threshold: number;
}

interface ITimerLogicBlock extends IActionBlock {
  action_type: "timer";
  waitTime: number;
}

// class DeviceBlock extends ActionBlock implements IDeviceBlock {
//   device_data: IDeviceData;
//   topic: string;

//   constructor(deviceBlockInitializers: IDeviceBlock) {
//     super(deviceBlockInitializers);
//     this.device_data = deviceBlockInitializers.device_data;
//     this.topic = this.device_data.mqtt_topic;
//   }
// // }

// class SensorBlock extends DeviceBlock {
//   device_data: IDeviceData;

//   constructor(sensorBlockInitializers: IDeviceBlock) {
//     super(sensorBlockInitializers);
//   }

//   startAction(): void {
//     console.log(`[SENSOR] Subscribing to topic: ${this.topic}`);
//     MqttBridge.getInstance().subscribeToTopic(
//       this.topic,
//       this.onReceiveDataFromSensor.bind(this),
//     );
//   }

//   exitAction(): void {
//     MqttBridge.getInstance().unsubscribeFromTopic(this.topic);
//   }

//   onReceiveDataFromSensor(data: any) {}
// }

// class ToggleButtonSensorBlock extends SensorBlock {
//   action_type: 'toggle-button-sensor' = 'toggle-button-sensor';
//   constructor(sensorBlockInitializers: IDeviceBlock) {
//     super(sensorBlockInitializers);
//   }

//   onReceiveDataFromSensor(data: any) {
//     console.log('[TOGGLE]Received data from toggle button sensor:', data);
//     this.startNextActionBlock();
//     this.senderDataStream?.next(data);
//   }
// }

// // class OnButtonSensorBlock extends SensorBlock {
// //   action_type: 'on-button-sensor' = 'on-button-sensor';
// //   constructor(sensorBlockInitializers: IDeviceBlock) {
// //     super(sensorBlockInitializers);
// //   }

// //   onReceiveDataFromSensor(data: any) {
// //     console.log('[ON]Received data from off button sensor:', data);
// //     this.startNextActionBlock();
// //     this.senderDataStream?.next(data);
// //   }
// // }

// // class OffButtonSensorBlock extends SensorBlock {
// //   action_type: 'off-button-sensor' = 'off-button-sensor';
// //   constructor(sensorBlockInitializers: ISensorBlock) {
// //     super(sensorBlockInitializers);
// //   }

// //   onReceiveDataFromSensor(data: any) {
// //     console.log('[OFF]Received data from off button sensor:', data);
// //     this.startNextActionBlock();
// //     this.senderDataStream?.next(data);
// //   }
// // }

// class ThermometerSensorBlock extends SensorBlock {
//   action_type: 'thermometer-sensor' = 'thermometer-sensor';
//   constructor(sensorBlockInitializers: IDeviceBlock) {
//     super(sensorBlockInitializers);
//   }

//   onReceiveDataFromSensor(data: any) {
//     console.log('[THERMOMETER]Received data from thermometer sensor:', data);
//     this.senderDataStream?.next(data);
//   }
// }

// class LightActuatorBlock extends DeviceBlock {
//   constructor(sensorBlockInitializers: IDeviceBlock) {
//     super(sensorBlockInitializers);
//   }

//   onReceiveDataFromPreviousBlock(data: any): void {
//     console.log(`[LIGHT ACTUATOR] Sending data to topic ${this.topic}:`, data);
//     MqttBridge.getInstance().publishMessage(this.topic, data);
//   }
// }

// function getActionBlock(actionBlockData: any): ActionBlock {
//   switch (actionBlockData.action_type) {
//     case 'light-actuator':
//       return new LightActuatorBlock(actionBlockData.topic);
//     // case 'on-button-sensor':
//     //   return new OnButtonSensorBlock(actionBlockData.topic);
//     // case 'off-button-sensor':
//     //   return new OffButtonSensorBlock(actionBlockData.topic);
//     // case 'toggle-button-sensor':
//     //   return new ToggleButtonSensorBlock(actionBlockData.topic);
//     case 'thermometer-sensor':
//       return new ThermometerSensorBlock(actionBlockData.topic);
//     case 'timer':
//       return new TimerLogicBlock(actionBlockData.waitTime);
//     case 'comparator':
//       return new ComparatorLogicBlock(actionBlockData.threshold);
//     default:
//       throw new Error('Invalid action type');
//   }
// }

const routine = [
  {
    first: true,
    currentId: "1",
    next_id: "2",
  },
  {
    currentId: "2",
    next_id: "3",
  },
  {
    first: true,
    currentId: "8",
    next_id: "3",
  },
];

const actions = [
  {
    id: "1",
    action: {
      action_type: "on-button-sensor",
      topic: "on-button-sensor",
    },
  },
  {
    id: "2",
    action: {
      action_type: "timer",
      waitTime: 2000,
    },
  },
  {
    id: "3",
    action: {
      action_type: "light-actuator",
      topic: "light-actuator",
    },
  },
  {
    id: "4",
    action: {
      action_type: "thermometer-sensor",
      topic: "thermometer-sensor",
    },
  },
  {
    id: "5",
    action: {
      action_type: "compare",
      threshold: 30,
    },
  },
  {
    id: "6",
    action: {
      action_type: "toggle-button-sensor",
      topic: "toggle-button-sensor",
    },
  },
  {
    id: "7",
    action: {
      action_type: "light-actuator",
      topic: "light-actuator",
    },
  },
  {
    id: "8",
    action: {
      action_type: "off-button-sensor",
      topic: "off-button-sensor",
    },
  },
];

// function startRoutine() {
//   const actionIdMap = new Map<string, ActionBlock>();

//   for (const action of actions) {
//     const block = getActionBlock(action.action);
//     actionIdMap.set(action.id, block);
//   }

//   for (const route of routine) {
//     const currentBlock = actionIdMap.get(route.currentId);
//     const nextBlock = actionIdMap.get(route.next_id);
//     if (currentBlock && nextBlock) {
//       currentBlock.setNextActionBlock([nextBlock]);
//     }
//   }

//   const firstBlocks = routine.filter((route) => route.first);

//   firstBlocks.forEach((block) => {
//     const action = actionIdMap.get(block.currentId);
//     if (action) {
//       action.startAction();
//     } else {
//       console.error(`Action not found for ID: ${block.currentId}`);
//     }
//   });
// }

import { ActionType } from "../types/ActionType";
export function test() {
  const testToggleDeviceData: IDeviceData = {
    device_id: "device-1",
    device_name: "Toggle Button Sensor",
    device_type: "toggle-button-sensor",
    mqtt_topic: "toggle-button-sensor",
    device_position: {
      x: 0,
      y: 0,
      z: 0,
    },
  };
  const toggleButtonDeviceBlock: IDeviceBlock = {
    id: "toggle-1",
    name: "Toggle Button Sensor",
    description: "Toggle Button Sensor",
    action_type: "toggle-button-sensor",
    device_data: testToggleDeviceData,
  };

  const lightActuatorDeviceData: IDeviceData = {
    device_id: "device-2",
    device_name: "Light Actuator",
    device_type: "light-actuator",
    mqtt_topic: "light-actuator",
    device_position: {
      x: 0,
      y: 0,
      z: 0,
    },
  };

  const lightActuatorDeviceBlock: IDeviceBlock = {
    id: "light-1",
    name: "Light Actuator",
    description: "Light Actuator",
    action_type: "light-actuator",
    device_data: lightActuatorDeviceData,
  };

  const timerBlock: ITimerLogicBlock = {
    id: "timer-1",
    name: "Timer",
    description: "Timer",
    action_type: "timer",
    waitTime: 5000,
  };

  const comparatorBlock: IComparatorLogicBlock = {
    id: "comparator-1",
    name: "Comparator",
    description: "Comparator",
    action_type: "comparator",
    threshold: 30,
  };

  let test = "ToggleButtonSensorBlock";

  switch (test) {
    case ActionType.ToggleButtonSensor:
      console.log("ToggleButtonSensorBlock");
      break;
    case ActionType.ThermometerSensor:
      console.log("ThermometerSensorBlock");
      break;
    default:
      console.log("Default");
  }

  // const toggleButtonSensorBlock = new ToggleButtonSensorBlock(
  //   toggleButtonDeviceBlock,
  // );

  // const timerLogicBlock = new TimerLogicBlock(timerBlock);
  // const lightActuatorBlock = new LightActuatorBlock(lightActuatorDeviceBlock);
  // const comparatorLogicBlock = new ComparatorLogicBlock(comparatorBlock);

  // toggleButtonSensorBlock.setNextActionBlock([
  //   timerLogicBlock,
  //   comparatorLogicBlock,
  // ]);
  // timerLogicBlock.setNextActionBlock([comparatorLogicBlock]);
  // comparatorLogicBlock.setNextActionBlock([lightActuatorBlock]);

  // toggleButtonSensorBlock.startAction();
}

test();
