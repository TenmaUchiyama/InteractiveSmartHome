import path from 'path';
import fs from 'fs';
import { Subject } from 'rxjs';
import MqttBridge from '../device-bridge/MqttBridge';

interface IActionBlock {
  nextActionBlock: IActionBlock | undefined;
  isActionStarted: boolean;
  setNextActionBlock(nextActionBlock: IActionBlock): void;
  setPreviousActionBlock(previousActionBlock: IActionBlock): void;
  setDataStream(dataStream: Subject<any>): void;
  startAction(): void;
  exitAction(): void;
  getIsActionStarted(): boolean;
}

class SensorBlock implements IActionBlock {
  isActionStarted: boolean = false;
  private topic: string;

  nextActionBlock: IActionBlock | undefined;
  previousActionBlock: IActionBlock | undefined;

  dataStream = new Subject<any>();

  constructor(topic: string) {
    this.topic = topic;
  }
  setPreviousActionBlock(previousActionBlock: IActionBlock): void {
    this.previousActionBlock = previousActionBlock;
  }

  setNextActionBlock(nextActionBlock: IActionBlock): void {
    this.nextActionBlock = nextActionBlock;
    this.nextActionBlock.setDataStream(this.dataStream);
    this.nextActionBlock.setPreviousActionBlock(this);
  }
  getIsActionStarted(): boolean {
    return this.isActionStarted;
  }

  exitAction(): void {
    this.isActionStarted = false;
    MqttBridge.getInstance().unsubscribeFromTopic(this.topic);
  }

  setDataStream = (dataStream: Subject<any>) => {
    this.dataStream = dataStream;
  };

  startAction(): void {
    this.isActionStarted = true;
    MqttBridge.getInstance().subscribeToTopic(
      this.topic,
      this.receiveData.bind(this),
    );
  }

  private receiveData(data: any) {
    if (this.nextActionBlock && !this.nextActionBlock.getIsActionStarted()) {
      this.nextActionBlock.startAction();
    }

    this.dataStream.next(data);
  }
}

class TimerLogicBlock implements IActionBlock {
  isActionStarted: boolean = false;
  nextActionBlock: IActionBlock | undefined;
  previousActionBlock: IActionBlock | undefined;

  dataStream = new Subject<any>();
  waitTime: number;
  constructor(waitTime: number) {
    this.waitTime = waitTime;
  }
  setPreviousActionBlock(previousActionBlock: IActionBlock): void {
    this.previousActionBlock = previousActionBlock;
  }
  setNextActionBlock(nextActionBlock: IActionBlock): void {
    this.nextActionBlock = nextActionBlock;
  }

  setDataStream(dataStream: Subject<any>): void {
    this.nextActionBlock?.setDataStream(dataStream);
  }

  startAction(): void {
    console.log('TIMER STARTED');
    this.previousActionBlock?.exitAction();
    setTimeout(() => {
      this.nextActionBlock?.startAction();
      this.exitAction();
    }, this.waitTime);
  }

  exitAction(): void {
    this.isActionStarted = false;
  }

  getIsActionStarted(): boolean {
    return this.isActionStarted;
  }
}

class SimpleCompareLogic implements IActionBlock {
  isActionStarted: boolean = false;
  nextActionBlock: IActionBlock | undefined;
  previousActionBlock: IActionBlock | undefined;

  dataStream = new Subject<any>();
  threshold: number;

  constructor(threshold: number) {
    this.threshold = threshold;
  }

  setPreviousActionBlock(previousActionBlock: IActionBlock): void {
    this.previousActionBlock = previousActionBlock;
  }

  setNextActionBlock(nextActionBlock: IActionBlock): void {
    this.nextActionBlock = nextActionBlock;
    this.nextActionBlock.setDataStream(this.dataStream);
    this.nextActionBlock.setPreviousActionBlock(this);
  }

  setDataStream(dataStream: Subject<any>): void {
    this.dataStream = dataStream;
    this.dataStream.subscribe(this.handleDataFromPreviousBlock.bind(this));
  }

  startAction(): void {
    this.isActionStarted = true;
  }

  exitAction(): void {
    this.isActionStarted = false;
  }

  getIsActionStarted(): boolean {
    return this.isActionStarted;
  }

  private handleDataFromPreviousBlock(data: any) {
    const parsedData = parseInt(data);

    if (isNaN(parsedData)) {
      console.error('Data is not a number');
      return;
    }
    if (data > this.threshold) {
      if (this.nextActionBlock && !this.nextActionBlock.getIsActionStarted()) {
        this.previousActionBlock?.exitAction();
        this.nextActionBlock.startAction();
      }
    } else {
      console.log('Data is less than threshold');
    }
  }
}

class ActuatorBlock implements IActionBlock {
  isActionStarted: boolean = false;

  nextActionBlock: IActionBlock | undefined;
  previousActionBlock: IActionBlock | undefined;

  private topic: string;
  dataStream = new Subject<any>();

  constructor(topic: string) {
    this.topic = topic;
  }
  setPreviousActionBlock(previousActionBlock: IActionBlock): void {
    this.previousActionBlock = previousActionBlock;
  }
  setNextActionBlock(nextActionBlock: IActionBlock): void {
    this.nextActionBlock = nextActionBlock;
  }

  async startAction() {
    this.isActionStarted = true;
    await MqttBridge.getInstance().publishMessage(
      this.topic,
      'Hello from actuator',
    );
    this.nextActionBlock?.startAction();
  }

  exitAction(): void {
    this.isActionStarted = false;
    MqttBridge.getInstance().unsubscribeFromTopic(this.topic);
  }

  public getIsActionStarted() {
    return this.isActionStarted;
  }

  public setDataStream = (dataStream: Subject<any>) => {
    this.dataStream = dataStream;

    this.dataStream.subscribe(this.handleDataFromPreviousBlock);
  };

  handleDataFromPreviousBlock(handleDataFromPreviousBlock: any) {
    console.log(
      'Received Data From Previous Block: ',
      handleDataFromPreviousBlock,
    );
  }
}

const getActionBlock = (block: any) => {
  switch (block.action_type) {
    case 'sensor':
      return new SensorBlock(block.topic);
    case 'compare':
      return new SimpleCompareLogic(block.threshold);
    case 'timer':
      return new TimerLogicBlock(block.wait_time);
    case 'actuator':
      return new ActuatorBlock(block.topic);
  }
};

export function test() {
  const routine = [
    {
      action_type: 'sensor',
      name: 'sensor-test',
      description: 'this is just a test sensor block',
      topic: 'sensor-data',
    },
    {
      action_type: 'compare',
      name: 'threshold-test',
      description: 'this is a threshold logic block',
      threshold: 50,
    },
    {
      action_type: 'timer',
      name: 'timer-test',
      description: 'this is just a test timer block',
      wait_time: 4000,
    },
    {
      action_type: 'actuator',
      name: 'actuator-test',
      description: 'this is just a test actuator block',
      topic: 'actuator-data',
    },
  ];

  let actionBlocks: any[] = [];
  for (const [index, block] of routine.entries()) {
    let actionBlock = getActionBlock(block);
    if (index !== 0) {
      actionBlocks[index - 1].setNextActionBlock(actionBlock);
    }
    if (index === routine.length - 1) {
      actionBlock!.setNextActionBlock(actionBlocks[0]);
    }
    actionBlocks.push(actionBlock);
  }

  actionBlocks[0].startAction();
}

test();
