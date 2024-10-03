import { Subject } from 'rxjs';
import { IActionBlock, IRxData } from '@/types/ActionBlockInterfaces';
import { ActionType } from '@/types/ActionType';

export default class ActionBlock implements IActionBlock {
  name: string;
  id: string;
  action_type: ActionType;
  description: string;
  isActionBlockActive: boolean;
  nextActionBlock: ActionBlock[] | undefined;
  senderDataStream: Subject<IRxData> | undefined;
  receiverDataStream: Subject<IRxData>[] = [];

  routineId: string;
  onReceiveDataFromPreviousBlock(data: IRxData) {}

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

  setRoutineId(routineId: string) {
    this.routineId = routineId;
  }

  getRoutineId(): string {
    return this.routineId;
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
      block.setReceiverDataStream(this.id, this.senderDataStream);
    });
  }
  setReceiverDataStream(
    action_id: string,
    dataStream: Subject<IRxData> | undefined,
  ): void {
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
