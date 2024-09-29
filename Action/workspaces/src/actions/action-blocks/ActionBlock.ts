import { Subject } from 'rxjs';
import { IActionBlock } from '../../types/ActionBlockInterfaces';
import { ActionType } from '../../types/ActionType';

export default class ActionBlock implements IActionBlock {
  name: string;
  id: string;
  action_type: ActionType;
  description: string;
  isActionBlockActive: boolean;
  nextActionBlock: ActionBlock[] | undefined;
  senderDataStream: Subject<any> | undefined;
  receiverDataStream: Subject<any>[] = [];
  onReceiveDataFromPreviousBlock(data: any) {}

  constructor(actionBlockInitializers: IActionBlock) {
    this.id = actionBlockInitializers.id;
    this.name = actionBlockInitializers.name;
    this.description = actionBlockInitializers.description;
    this.action_type = actionBlockInitializers.action_type;

    this.isActionBlockActive = false;
    this.senderDataStream = new Subject();
  }

  startAction(): void {
    this.isActionBlockActive = true;
  }
  exitAction(): void {
    throw new Error('Method not implemented.');
  }

  getIsActionBlockActive(): boolean {
    return this.isActionBlockActive;
  }
  setNextActionBlock(nextActionBlock: ActionBlock[] | undefined): void {
    if (!this.nextActionBlock) this.nextActionBlock = [];

    this.nextActionBlock = !this.nextActionBlock
      ? nextActionBlock
      : [...this.nextActionBlock, ...nextActionBlock!];

    this.nextActionBlock!.forEach((block) => {
      block.setReceiverDataStream(this.senderDataStream);
    });
  }
  setReceiverDataStream(dataStream: Subject<any> | undefined): void {
    if (dataStream && !this.receiverDataStream.includes(dataStream)) {
      this.receiverDataStream.push(dataStream);
      dataStream.subscribe(this.onReceiveDataFromPreviousBlock.bind(this));
    }
  }

  async startNextActionBlock() {
    if (this.nextActionBlock) {
      this.nextActionBlock.forEach(async (block) => {
        if (!block.getIsActionBlockActive()) {
          await block.startAction();
        }
      });
    }
  }
}
