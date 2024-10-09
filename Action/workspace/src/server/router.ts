import express, { Request, Response } from 'express';
import routineRouter from './routes/routine/routineRouter';
import deviceRouter from './routes/device/deviceRouter';
import actionRouter from './routes/action/actionRouter';
import flowNodeRouter from './routes/flow-node/flowNodeRouter';
import flowEdgeRouter from './routes/flow-edge/flowEdgeRouter';
import cors from 'cors';

const app = express();
app.use(express.json());
app.use(
  cors({
    origin: '*',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
    allowedHeaders: ['Content-Type', 'Authorization'],
  }),
);

const port = 4049;

app.use('/routine', routineRouter);

app.use('/action', actionRouter);

app.use('/device', deviceRouter);

app.use('/flow-node', flowNodeRouter);

app.use('/flow-edge', flowEdgeRouter);

app.listen(port, () => {
  console.log(`Server is running at http://0.0.0.0:${port}`);
});
