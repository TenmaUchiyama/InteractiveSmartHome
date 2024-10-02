import ActionBlock from '../ActionBlock';
import * as readline from 'readline';

export default class TestBlock extends ActionBlock {
  constructor(actionBlockInitializers: any) {
    super(actionBlockInitializers);
  }

  startAction() {
    super.startAction();
    const rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
    });

    rl.question('Enter your input: ', (answer) => {
      console.log(`You entered: ${answer}`);
      rl.close();
    });
  }
}
