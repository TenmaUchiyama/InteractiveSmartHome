export default class Debugger {
  public static instance: Debugger;

  // 色の定義
  private colors: { [key: string]: string } = {
    yellow: "\u001b[33m",
    magenta: "\u001b[35m",
    cyan: "\u001b[36m",
    white: "\u001b[37m",
    brightYellow: "\u001b[93m", // 明るい黄
    brightMagenta: "\u001b[95m", // 明るい紫
    brightCyan: "\u001b[96m", // 明るい水色
    brightWhite: "\u001b[97m", // 明るい白
    bgYellow: "\u001b[43m", // 背景黄
    bgMagenta: "\u001b[45m", // 背景紫
    bgCyan: "\u001b[46m", // 背景水色
    bgWhite: "\u001b[47m",
  };

  private standard: string = "\u001b[0m"; // 標準のシーケンス
  private reset: string = "\u001b[0m"; // リセットシーケンス
  private routineIdColorMap: { [routineId: string]: string } = {};

  public static getInstance(): Debugger {
    if (!Debugger.instance) {
      Debugger.instance = new Debugger();
    }
    return Debugger.instance;
  }

  public debugLog(routineName: string, caller: string, debugMsg: string) {
    const color = this.getColor(routineName);
    console.log(
      `${color}${routineName}  [${caller}]: ${debugMsg}${this.reset}`
    );
  }

  public debugLogOneshot(
    routineName: string,
    caller: string,
    debugMsg: string
  ) {
    const color = this.getColor(routineName);
    console.log(
      "=============================================================="
    );
    console.log(
      `${color}${routineName}  [${caller}]: ${debugMsg}${this.reset}`
    );
    console.log(
      "=============================================================="
    );
  }

  public debugError(routineName: string, caller: string, debugMsg: string) {
    const color = this.getColor(routineName);
    const errorColor = "\u001b[31m";
    console.log(
      `${color}${routineName} ${errorColor} ERROR ${color} [${caller}]: ${debugMsg}${this.reset}`
    );
  }

  getColor(routineName: string): string {
    // 新しいルーチン名の場合

    if (!(routineName in this.routineIdColorMap)) {
      // 色を順番に選択
      const colorKeys = Object.keys(this.colors);
      const colorIndex = Object.keys(this.routineIdColorMap).length; // 現在のルーチン数でインデックスを決定
      const color = this.colors[colorKeys[colorIndex % colorKeys.length]]; // 色をループさせる
      this.routineIdColorMap[routineName] = color; // 色をマッピングに追加
    }
    const color = this.routineIdColorMap[routineName];
    if (routineName === undefined) {
      const color = this.reset;
    }

    return color;
  }
}
