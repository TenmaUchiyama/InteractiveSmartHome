import { createClient } from 'redis';

class DeviceSignalTransit {
    private client;

    constructor() {
        this.client = createClient();
        this.client.connect();
    }

    async triggerEvent(channel: string, message: string) {
        await this.client.publish(channel, message);
    }
}

export default DeviceSignalTransit;
