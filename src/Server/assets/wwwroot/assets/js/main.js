import { ChatBox } from './module/ChatBox.js';
import { ChatWSS } from './service/ChatWSS.js';
document.addEventListener('DOMContentLoaded', async () => {
    const session = new ChatWSS();
    session.initWss();

    new ChatBox(session);
});

