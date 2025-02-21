import { messages } from './store';

export class WebSocketClient {
	private socket: WebSocket | null = null;
	private url: string;

	private reconnectInterval: number = 3000; // 再接続の間隔 (3秒)
	private isClosedManually: boolean = false; // 手動で閉じたかの判定

	constructor(url: string) {
		this.url = url;
	}

	public onMessage: ((data: string) => void) | null = null;

	// WebSocket接続
	connect(): void {
		this.isClosedManually = false;
		this.socket = new WebSocket(this.url);

		this.socket.onopen = () => {
			console.log('WebSocket Connected to', this.url);
		};

		this.socket.onmessage = (event: MessageEvent) => {
			// messages.update((msgs) => [...msgs, event.data])
			if (this.onMessage) this.onMessage(event.data);
		};

		this.socket.onerror = (error: Event) => {
			console.error('WebSocket Error:', error);
		};

		this.socket.onclose = () => {
			console.log('WebSocket Disconnected');
			if (!this.isClosedManually) {
				setTimeout(() => this.connect(), this.reconnectInterval);
			}
		};
	}

	// メッセージ送信
	send(message: string): void {
		if (this.socket && this.socket.readyState === WebSocket.OPEN) {
			this.socket.send(message);
		} else {
			console.warn('WebSocket is not open. Message not sent.');
		}
	}

	// WebSocketを手動で閉じる
	close(): void {
		this.isClosedManually = true;
		if (this.socket) {
			this.socket.close();
			this.socket = null;
		}
	}
}
