import express, { Request, Response } from 'express';
import routineRouter from './routes/routine/routineRouter';

const app = express();
const port = 3000;

app.use('/routine', routineRouter);

app.listen(port, () => {
  console.log(`Server is running at http://0.0.0.0:${port}`);
});
