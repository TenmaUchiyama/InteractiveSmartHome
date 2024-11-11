import { ActionType } from "@/types/ActionType";
import { ISignalData, ITimerLogicBlock } from "@/types/ActionBlockInterfaces";
import ActionBlock from "../ActionBlock";
import Debugger from "@debugger/Debugger";
import MqttBridge from "@/device-bridge/MqttBridge";
export default class TimerLogicBlock
  extends ActionBlock
  implements ITimerLogicBlock
{
  waitTime: number;
  private timerId: NodeJS.Timeout | null = null;

  constructor(timerBlockInitializers: ITimerLogicBlock) {
    super(timerBlockInitializers);
    this.waitTime = timerBlockInitializers.waitTime;
    this.action_type = ActionType.Logic_Timer;
  }

  exitAction(): void {
    if (this.timerId) {
      clearTimeout(this.timerId);
      this.timerId = null;
    }

    super.exitAction();
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    let singalData: ISignalData = {
      action_id: this.id,
      data_type: "number",
      value: this.waitTime,
    };

    const interval = setInterval(() => {
      // 残りの秒数をMQTTで配信
      super.SendSignalDataToNodeFlow(singalData);

      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        "TIMER",
        "Sending data to MQTT " + JSON.stringify(singalData)
      );

      // 残り秒数をデクリメント
      (singalData.value as number) -= 1;

      // 0に達した場合に次のブロックに進む
      if ((singalData.value as number) < 0) {
        clearInterval(interval); // カウントダウン終了後にインターバルを停止
        Debugger.getInstance().debugLog(
          this.getRoutineId(),
          "TIMER",
          "pass the data to next block " + JSON.stringify(data)
        );
        data.action_id = this.id;
        this.startNextActionBlock();
        this.senderDataStream?.next(data);
      }
    }, 1000);
  }
}
