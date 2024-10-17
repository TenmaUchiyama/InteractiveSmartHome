import { Subject } from "rxjs";
import { IActionBlock, IRxData } from "@/types/ActionBlockInterfaces";
import { ActionType } from "@/types/ActionType";
import Debugger from "@/debugger/Debugger";

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
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      this.name,
      "Exiting action block"
    );
    this.isActionBlockActive = false;

    if (this.senderDataStream) {
      try {
        // 完了の呼び出しを避ける
        this.senderDataStream.unsubscribe(); // 購読を解除
      } catch (error) {
        console.error("Error unsubscribing from senderDataStream:", error);
      }
      this.senderDataStream = undefined; // ストリームを無効化
    }

    this.receiverDataStream.forEach((stream) => {
      if (stream) {
        try {
          stream.unsubscribe(); // 各受信ストリームの購読を解除
        } catch (error) {
          console.error("Error unsubscribing from receiver stream:", error);
        }
      }
    });
    this.receiverDataStream = []; // 配列をリセット
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
    dataStream: Subject<IRxData> | undefined
  ): void {
    if (dataStream && !this.receiverDataStream.includes(dataStream)) {
      this.receiverDataStream.push(dataStream);
      dataStream.subscribe(this.onReceiveDataFromPreviousBlock.bind(this));
    }
  }

  startNextActionBlock() {
    if (this.nextActionBlock) {
      this.nextActionBlock.forEach((block) => {
        if (!block.getIsActionBlockActive()) {
          block.startAction();
        }
      });
    }
  }
}
