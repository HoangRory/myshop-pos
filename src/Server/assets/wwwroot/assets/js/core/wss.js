export class WSS {
    static MAGIC = 0x4643554C; // 'LUCF'

    constructor(version = 'v1', base = 'wss', interval = 200) {
        this.connectTimeout = null;
        this.reconnectTimeout = null;
        this.active = false;

        this.lastSend = null;
        this.websocket = null;
        this.handlers = {};

        // Thay vì cộng chuỗi đơn thuần
        this.prefix = `/${version}/${base}`.replace(/\/+/g, '/').replace(/\/$/, '');

        this.queue = [];
        this.interval = interval;
        this.startQueue();
    }

    initWss() {
        this.start();
        this.autoConnect(1000);
        this.autoDisconnect(10000);
    }

    send(data) {
        if (!data) return true;
        if (!this.websocket || this.websocket.readyState !== WebSocket.OPEN) {
            console.warn("WebSocket chưa sẵn sàng, không thể gửi.");
            return false;
        }
        this.websocket.send(data);
        return true;
    }

    lockSend() {
        const now = Date.now();
        if (this.lastSend && (now - this.lastSend < this.interval)) return false; // quá gần → block
        this.lastSend = now; // cập nhật cho lần gửi này
        return true;
    }

    on(url, callback) {
        this.handlers[url] = callback;
    }

    start() {
        this.active = true;
    }

    stop() {
        this.active = false;
    }

    autoConnect(time) {
        this.connect();
        this.reconnectTimeout = setInterval(() => {
            if (!this.active) return;
            if (!this.websocket || this.websocket.readyState === WebSocket.CLOSED)
                this.connect();
        }, time);
    }

    autoDisconnect(time) {
        this.connectTimeout = setInterval(() => {
            if (this.active) return;
            if (this.websocket && this.websocket.readyState === WebSocket.CONNECTING) {
                this.closeConnect(1006);
            }
        }, time);
    }

    connect() {
        if (this.websocket && this.websocket.readyState !== WebSocket.CLOSED) {
            this.closeConnect(1000);
        }
        this.initConnect();
    }

    disconnect() {
        if (this.websocket && this.websocket.readyState === WebSocket.OPEN) {
            this.closeConnect(1000);
        }
    }

    closeConnect(code) {
        if (this.websocket && this.websocket.readyState === WebSocket.OPEN) {
            this.websocket.close(code, "Closed");
        }
    }

    initConnect() {
        this.websocket = new WebSocket(this.getConnectString());
        this.websocket.binaryType = "arraybuffer";
        this.websocket.onopen = (evt) => setTimeout(() => this.onOpen(evt), 0);
        this.websocket.onclose = (evt) => setTimeout(() => this.onClose(evt), 0);
        this.websocket.onmessage = (evt) => setTimeout(() => this.onMessage(evt), 0);
        this.websocket.onerror = (evt) => setTimeout(() => this.onError(evt), 0);
    }

    onOpen(evt) {
        console.log("WSS Connected:", evt);
    }

    onClose(evt) {
        console.log(`Disconnected (Code: ${evt.code}, Reason: ${evt.reason})`);
        this.websocket = null;
    }

    onError(evt) {
        console.error(`Error:`, evt);
    }

    onMessage(evt) {
        const parsed = WSS.parse(evt.data);
        if (!parsed) return;

        const { url, body } = parsed;

        if (this.handlers[url]) {
            setTimeout(() => {
                this.handlers[url](body);
            }, 0);
        } else {
            console.log(`[${url}] Unhandled message:`, body);
        }
    }

    sendMessage(body, url = 'Default') {
        const packet = this.toBytes(body, url);
        if (!packet) return false;

        if (!this.lockSend()) return false;

        setTimeout(() => {
            this.send(packet);
        }, 0);
    }

    // Trong WSS.js
    sendMessageAsync(body, url = 'Default') {
        if (body === undefined || body === null) return;
        this.queue.push({ body, url });
    }

    startQueue() {
        this.queueInterval = setInterval(() => {
            if (!this.active) {
                clearInterval(this.queueInterval);
                return;
            }
            if (this.queue.length === 0) return;

            const msgObj = this.queue.shift(); // Lấy 1 cái đầu tiên ra

            this.sendMessage(msgObj.body, msgObj.url);
        }, this.interval);
    }

    getConnectString() {
        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        const host = window.location.host;
        return `${protocol}//${host}`;
    }

    toBytes(data, url = 'Default') {
        url = url.startsWith('/') ? url : `/${url}`;
        url = url.endsWith('/') ? url.substring(0, url.length - 1) : url;
        const urlBytes = WSS.encodeText(this.prefix + url);

        // BODY = raw bytes
        let bodyBytes;
        if (data instanceof Uint8Array) bodyBytes = data; // raw binary
        else if (typeof data === 'string') bodyBytes = WSS.encodeText(data); // raw text
        else  bodyBytes = WSS.encodeJson(data ?? {}); // object → JSON → bytes

        return WSS.toBytes(bodyBytes, urlBytes);
    }

    static toBytes(bodyBytes, urlBytes) {
        const uLen = urlBytes.length;
        const bLen = bodyBytes.length;

        const buffer = new Uint8Array(4 + 4 + uLen + bLen);

        // MAGIC (little-endian)
        buffer[0] = this.MAGIC & 0xFF;
        buffer[1] = (this.MAGIC >> 8) & 0xFF;
        buffer[2] = (this.MAGIC >> 16) & 0xFF;
        buffer[3] = (this.MAGIC >> 24) & 0xFF;

        // UrlLength (little-endian)
        buffer[4] = uLen & 0xFF;
        buffer[5] = (uLen >> 8) & 0xFF;
        buffer[6] = (uLen >> 16) & 0xFF;
        buffer[7] = (uLen >> 24) & 0xFF;

        // URL
        buffer.set(urlBytes, 8);

        // BODY
        buffer.set(bodyBytes, 8 + uLen);

        return buffer;
    }

    static parse(data) {
        const buffer = data instanceof Uint8Array ? data : new Uint8Array(data);
        if (buffer.length < 8) return null;

        // MAGIC
        const magic =
            buffer[0] |
            (buffer[1] << 8) |
            (buffer[2] << 16) |
            (buffer[3] << 24);

        if (magic !== this.MAGIC) return null;

        // UrlLength
        const uLen =
            buffer[4] |
            (buffer[5] << 8) |
            (buffer[6] << 16) |
            (buffer[7] << 24);

        if (buffer.length < 8 + uLen) return null;

        const urlBytes = buffer.slice(8, 8 + uLen);
        const bodyBytes = buffer.slice(8 + uLen);

        const url = WSS.decodeText(urlBytes);

        return { url, body: bodyBytes };
    }

    static encodeText(text) {
        const encoder = new TextEncoder();
        return encoder.encode(text);
    }
    static decodeText(bodyBytes) {
        const decoder = new TextDecoder();
        return decoder.decode(bodyBytes);
    }

    static encodeJson(data) {
        const encoder = new TextEncoder();
        return encoder.encode(JSON.stringify(data ?? {}));
    }

    static decodeJson(bodyBytes) {
        const decoder = new TextDecoder();
        return JSON.parse(decoder.decode(bodyBytes));
    }

}

