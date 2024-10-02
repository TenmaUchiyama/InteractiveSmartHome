import { Request, Response } from 'express';
import { MongoDB } from '@database';
import { IDBDeviceBlock, IDeviceData } from '@/types/ActionBlockInterfaces';
import { DeviceType } from '@/types/ActionType';

export const connectionTest = (req: Request, res: Response) => {
  res.status(200).send('Hello from device');
};

export const getAllDevicesApi = async (req: Request, res: Response) => {
  try {
    console.log('Getting all devices');
    const devices = await MongoDB.getInstance().getAllDevices();
    return res.status(200).send(devices);
  } catch {
    return res.status(500).send('Failed to get devices');
  }
};

export const getDeviceApi = async (req: Request, res: Response) => {
  try {
    const deviceId = req.params.id;
    console.log('Getting device:', deviceId);
    if (deviceId === undefined) {
      return res.status(400).send('Device ID is required');
    }
    const device = await MongoDB.getInstance().getDevice(deviceId);
    if (device === null) {
      return res.status(404).send('Device not found');
    }
    return res.status(200).send(device);
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to get device: ' + error.message);
  }
};

export const addDeviceApi = async (req: Request, res: Response) => {
  try {
    const device = req.body;
    console.log('Adding device:', device);

    // デバイスが配列かどうかを確認し、必要に応じて配列に変換
    const deviceArray = Array.isArray(device) ? device : [device];
    for (const dev of deviceArray) {
      const isValid = validateDevice(dev);
      if (!isValid) {
        return res
          .status(400)
          .send(`Invalid device data: ${JSON.stringify(dev)}`);
      }
    }

    await MongoDB.getInstance().addDevices(deviceArray);
    return res.status(200).send('Device added');
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to add device: ' + error.message);
  }
};

export const updateDeviceApi = async (req: Request, res: Response) => {
  try {
    const device = req.body;
    console.log('Updating device:', device);
    await MongoDB.getInstance().updateDevice(device);
    return res.status(200).send('Device updated');
  } catch (error) {
    console.error(error);
    return res.status(500).send('Failed to update device: ' + error.message);
  }
};

export const deleteDeviceApi = async (req: Request, res: Response) => {
  try {
    const deviceId = req.params.id;
    console.log('Deleting device:', deviceId);
    if (deviceId === undefined) {
      return res.status(400).send('Device ID is required');
    }
    await MongoDB.getInstance().deleteDevice(deviceId);
    return res.status(200).send('Device deleted');
  } catch (error) {
    console.error(error); // エラー内容をログに記録
    return res.status(500).send('Failed to delete device: ' + error.message);
  }
};

const validateDevice = (device: any): device is IDeviceData => {
  const requiredKeys = [
    'device_id',
    'device_name',
    'device_type',
    'mqtt_topic',
    'device_location',
  ];

  // すべてのキーが存在するか確認
  const hasAllKeys = requiredKeys.every((key) => key in device);

  // device_locationがオブジェクトであり、x, y, zが含まれているか確認
  const hasValidLocation =
    typeof device.device_location === 'object' &&
    'x' in device.device_location &&
    'y' in device.device_location &&
    'z' in device.device_location;

  return hasAllKeys && hasValidLocation;
};
