import express, { Request, Response } from 'express';
import routineRouter from './routes/routine/routineRouter';
import deviceRouter from './routes/device/deviceRouter';
import actionRouter from './routes/action/actionRouter';

const app = express();
app.use(express.json());
const port = 3000;

app.use('/routine', routineRouter);

app.use('/action', actionRouter);

app.use('/device', deviceRouter);

app.listen(port, () => {
  console.log(`Server is running at http://0.0.0.0:${port}`);
});
