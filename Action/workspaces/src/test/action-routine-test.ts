import TestBlock from '@block/test/TestBlock';
import { MongoDB } from '@/database-connector/mongodb';

async function main() {
  const testBlock = new TestBlock({
    name: 'TestBlock',
    id: 'TestBlock',
    description: 'TestBlock',
  });

  testBlock.startAction();
}

main();
