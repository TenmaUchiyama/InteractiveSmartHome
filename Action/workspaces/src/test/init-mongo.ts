import { MongoClient } from 'mongodb';

let client = new MongoClient('mongodb://mongodb:27017');

const main = async () => {
  try {
    await client.connect();
  } catch {
    console.error('not working');
  } finally {
    await client.close();
  }
};

main();
