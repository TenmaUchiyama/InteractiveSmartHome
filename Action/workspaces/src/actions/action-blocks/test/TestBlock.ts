import {
  IActionBlock,
  IRoutineData,
  IRxData,
} from '@/types/ActionBlockInterfaces';
import ActionBlock from '../ActionBlock';
import * as readline from 'readline';
import { ActionType } from '@/types/ActionType';
import Debugger from '@/debugger/Debugger';

export default class TestBlock extends ActionBlock {
  data: IRxData;
  constructor(testInit: IRxData) {
    super({
      id: 'test-id',
      name: 'TestBlock',
      description: 'TestBlock',
      action_type: ActionType.Test,
    });

    this.data = {
      action_id: 'test_id',
      data_type: 'boolean',
      value: true,
    };
  }

  startAction() {
    super.startAction();

    console.log(
      '[TEST BLOCK]: Starting test block and sending data',
      this.data,
    );

    this.startNextActionBlock();
    this.senderDataStream?.next(this.data);
  }

  onReceiveDataFromPreviousBlock(data: IRxData): void {
    console.log('Received data from previous block:', data);
  }

  routine: IRoutineData = {
    id: 'gate-test-routine',
    name: 'gate-test',
    action_routine: [
      {
        first: true,
        current_block_id: 'toggle-button-sensor',
        next_block_id: 'gate-sample',
      },
      {
        first: true,
        current_block_id: 'thermo-1',
        next_block_id: 'comparator-101',
      },
      {
        current_block_id: 'comparator-101',
        next_block_id: 'gate-sample',
      },
      {
        current_block_id: 'gate-sample',
        next_block_id: 'light-1',
      },
    ],
  };
}
