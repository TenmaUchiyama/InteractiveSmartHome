import chalk, { ChalkInstance } from 'chalk';

export default class Debugger {
  public static instance: Debugger;

  routineIdColorMap: { [routineId: string]: ChalkInstance } = {};
  colorList: ChalkInstance[] = [
    chalk.hex('#FFFFFF'), // 白
    chalk.hex('#F1C40F'), // 黄
    chalk.hex('#3498DB'), // 水色
    chalk.hex('#2ECC71'), // 緑
    chalk.hex('#E67E22'), // オレンジ
    chalk.hex('#8E44AD'), // 紫
    chalk.hex('#3357FF'), // 青
  ];

  public static getInstance(): Debugger {
    if (!Debugger.instance) {
      Debugger.instance = new Debugger();
    }
    return Debugger.instance;
  }

  public debugLog(routineName: string, caller: string, debugMsg: string) {
    // 新しいルーチン名の場合
    if (!(routineName in this.routineIdColorMap)) {
      // 色を順番に選択
      const colorIndex = Object.keys(this.routineIdColorMap).length; // 現在のルーチン数でインデックスを決定
      const color = this.colorList[colorIndex % this.colorList.length]; // 色をループさせる
      this.routineIdColorMap[routineName] = color; // 色をマッピングに追加
    }

    const color = this.routineIdColorMap[routineName];

    // コンソールに色付きメッセージを出力
    console.log(color(`${routineName} [${caller}]: ${debugMsg}`));
  }
}
