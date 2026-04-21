import { WSS } from "../core/wss.js";

export class ChatWSS extends WSS {
    constructor(interval = 50) {
        super('v1', 'wss', interval);
    }

    onOpen(evt) {
        this.sendMessage({}, 'GetMessage');
        super.onOpen(evt);
    }


}
