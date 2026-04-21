/**
 * ChatBox.js — Self-contained chat UI module
 * - Thu lại → hình tròn FAB góc phải
 * - Mở ra → panel đầy đủ
 * - Enter gửi, Shift+Enter xuống dòng
 * - from === deviceId → bubble phải (me), ngược lại → trái (other)
 * - URL chỉ truyền ngọn, WSS tự ghép prefix
 */
export class ChatBox {
    constructor(wss, options = {}) {
        this.wss = wss;
        this.opts = Object.assign({
            title: 'Chat',
            url: 'ChatMessage',
            placeholder: 'Nhập tin nhắn... (Enter để gửi)',
            maxMessages: 200,
            collapsed: true,
        }, options);

        this.deviceId  = this._getOrCreateDeviceId();
        this.collapsed = this.opts.collapsed;

        this._injectStyles();
        this._render();
        this._bindEvents();
        this._bindWSSEvents();

        if (this.collapsed) this._applyCollapsed(false);
    }

    // ─── DeviceId ─────────────────────────────────────────────────────────────

    _getOrCreateDeviceId() {
        let id = localStorage.getItem('deviceId');
        if (!id) {
            id = crypto.randomUUID();
            localStorage.setItem('deviceId', id);
        }
        return id;
    }

    // ─── WSS ──────────────────────────────────────────────────────────────────

    _bindWSSEvents() {
        const fullUrl = this.wss.prefix + '/' + this.opts.url.replace(/^\//, '');

        this.wss.on(fullUrl, (bodyBytes) => {
            let body;
            try { body = JSON.parse(new TextDecoder().decode(bodyBytes)); }
            catch { return; }

            const who = body.from === this.deviceId ? 'me' : 'other';
            this._appendMessage(body.text, who, body.from);

            if (this.collapsed && who === 'other') this._bumpBadge();
        });
    }

    _send() {
        const text = this._input.value.trim();
        if (!text) return;

        this._input.value = '';
        this._input.style.height = 'auto';
        this._input.focus();

        this.wss.sendMessageAsync(
            { id: crypto.randomUUID(), text, from: this.deviceId },
            this.opts.url
        );
    }

    // ─── Toggle ───────────────────────────────────────────────────────────────

    _toggle() {
        this.collapsed ? this._expand() : this._collapse();
    }

    _collapse() {
        this.collapsed = true;
        this._fab.classList.remove('cb-fab-hidden');
        this._panel.classList.add('cb-panel-hidden');
    }

    _expand() {
        this.collapsed = false;
        this._clearBadge();
        this._fab.classList.add('cb-fab-hidden');
        this._panel.classList.remove('cb-panel-hidden');
        requestAnimationFrame(() => {
            this._body.scrollTop = this._body.scrollHeight;
            this._input.focus();
        });
    }

    _applyCollapsed(animate = true) {
        if (!animate) {
            this._fab.style.transition   = 'none';
            this._panel.style.transition = 'none';
            requestAnimationFrame(() => {
                this._fab.style.transition   = '';
                this._panel.style.transition = '';
            });
        }
        this._fab.classList.remove('cb-fab-hidden');
        this._panel.classList.add('cb-panel-hidden');
    }

    // ─── Badge ────────────────────────────────────────────────────────────────

    _bumpBadge() {
        const b = this._fab.querySelector('.cb-badge');
        const n = parseInt(b.dataset.count || '0') + 1;
        b.dataset.count = n;
        b.textContent = n > 99 ? '99+' : n;
        b.hidden = false;
    }

    _clearBadge() {
        const b = this._fab.querySelector('.cb-badge');
        b.dataset.count = '0';
        b.hidden = true;
    }

    // ─── DOM ──────────────────────────────────────────────────────────────────

    _appendMessage(text, who, from = '') {
        const wrap = document.createElement('div');
        wrap.className = `cb-wrap cb-${who}`;

        const bubble = document.createElement('div');
        bubble.className = 'cb-bubble';
        bubble.textContent = text;

        const meta = document.createElement('span');
        meta.className = 'cb-meta';
        const label = who === 'me' ? 'Bạn' : (from ? from.slice(0, 8) + '…' : 'Khách');
        meta.textContent = `${label} · ${this._time()}`;

        wrap.appendChild(bubble);
        wrap.appendChild(meta);
        this._body.appendChild(wrap);

        while (this._body.childElementCount > this.opts.maxMessages)
            this._body.removeChild(this._body.firstElementChild);

        requestAnimationFrame(() => { this._body.scrollTop = this._body.scrollHeight; });
    }

    _time() {
        return new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    // ─── Render ───────────────────────────────────────────────────────────────

    _render() {
        document.getElementById('chatbox-root')?.remove();

        const root = document.createElement('div');
        root.id = 'chatbox-root';
        root.innerHTML = `
            <!-- FAB tròn khi thu nhỏ -->
            <button class="cb-fab" aria-label="Mở chat">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                </svg>
                <span class="cb-badge" hidden>0</span>
            </button>

            <!-- Panel đầy đủ -->
            <div class="cb-panel">
                <div class="cb-header">
                    <span class="cb-dot"></span>
                    <span class="cb-title">${this.opts.title}</span>
                    <span class="cb-id" title="Device ID">#${this.deviceId.slice(0, 6)}</span>
                    <button class="cb-close" aria-label="Thu nhỏ">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                            <polyline points="18 15 12 9 6 15"/>
                        </svg>
                    </button>
                </div>
                <div class="cb-body"></div>
                <div class="cb-composer">
                    <textarea class="cb-input" rows="1"
                        placeholder="${this.opts.placeholder}"
                        autocomplete="off" spellcheck="false"></textarea>
                    <button class="cb-send" aria-label="Send">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2">
                            <line x1="22" y1="2" x2="11" y2="13"/>
                            <polygon points="22 2 15 22 11 13 2 9 22 2"/>
                        </svg>
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(root);

        this._fab     = root.querySelector('.cb-fab');
        this._panel   = root.querySelector('.cb-panel');
        this._body    = root.querySelector('.cb-body');
        this._input   = root.querySelector('.cb-input');
        this._sendBtn = root.querySelector('.cb-send');
    }

    _bindEvents() {
        // FAB → mở
        this._fab.addEventListener('click', () => this._expand());

        // Nút X trong header → thu
        this._panel.querySelector('.cb-close').addEventListener('click', () => this._collapse());

        // Gửi
        this._sendBtn.addEventListener('click', () => this._send());
        this._input.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this._send(); }
        });

        // Auto-resize textarea
        this._input.addEventListener('input', () => {
            this._input.style.height = 'auto';
            this._input.style.height = Math.min(this._input.scrollHeight, 120) + 'px';
        });
    }

    // ─── Styles ───────────────────────────────────────────────────────────────

    _injectStyles() {
        if (document.getElementById('chatbox-style')) return;

        const s = document.createElement('style');
        s.id = 'chatbox-style';
        s.textContent = `
#chatbox-root {
    --accent:   #3498db;
    --accent-d: #2075a8;
    --bg:       #ffffff;
    --surface:  #f7f7f7;
    --border:   #e0e0e0;
    --text:     #1a1a1a;
    --muted:    #aaa;
    --me-bg:    #3498db;
    --me-fg:    #fff;
    --other-bg: #eee;
    --other-fg: #333;
    --radius:   12px;
    --shadow:   0 6px 28px rgba(0,0,0,.18);
    --font:     'Segoe UI', system-ui, sans-serif;

    position: fixed;
    bottom: 24px;
    right: 24px;
    z-index: 9999;
    font-family: var(--font);
    box-sizing: border-box;
}
#chatbox-root *, #chatbox-root *::before, #chatbox-root *::after { box-sizing: inherit; }

/* ── FAB ── */
.cb-fab {
    position: absolute;
    bottom: 0; right: 0;
    width: 56px; height: 56px;
    border-radius: 50%;
    background: var(--accent);
    color: #fff;
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 18px rgba(52,152,219,.45);
    transition: transform .2s ease, opacity .2s ease, background .15s;
    outline: none;
}
.cb-fab:hover { background: var(--accent-d); transform: scale(1.08); }
.cb-fab svg   { width: 24px; height: 24px; }

.cb-fab.cb-fab-hidden {
    opacity: 0;
    transform: scale(.5);
    pointer-events: none;
}

.cb-badge {
    position: absolute;
    top: 4px; right: 4px;
    min-width: 18px; height: 18px;
    background: #e74c3c;
    border-radius: 9px;
    font-size: 11px;
    font-weight: 700;
    line-height: 18px;
    text-align: center;
    padding: 0 5px;
    pointer-events: none;
    animation: cb-pop .2s ease;
}

/* ── Panel ── */
.cb-panel {
    position: absolute;
    bottom: 0; right: 0;
    width: 300px;
    max-width: calc(100vw - 48px);
    background: var(--bg);
    border-radius: var(--radius);
    box-shadow: var(--shadow);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    border: 1px solid var(--border);
    transform-origin: bottom right;
    transition: transform .25s cubic-bezier(.34,1.56,.64,1), opacity .2s ease;
    transform: scale(1);
    opacity: 1;
}
.cb-panel.cb-panel-hidden {
    transform: scale(.4);
    opacity: 0;
    pointer-events: none;
}

/* Header */
.cb-header {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 14px;
    background: var(--accent);
    color: #fff;
    font-weight: 600;
    font-size: 14px;
    flex-shrink: 0;
    user-select: none;
}
.cb-dot {
    width: 8px; height: 8px;
    border-radius: 50%;
    background: #2ecc71;
    box-shadow: 0 0 0 2px rgba(255,255,255,.3);
    flex-shrink: 0;
}
.cb-title { flex: 1; }
.cb-id {
    font-size: 11px;
    font-weight: 400;
    opacity: .65;
    font-family: monospace;
}
.cb-close {
    width: 26px; height: 26px;
    background: rgba(255,255,255,.15);
    border: none;
    border-radius: 50%;
    color: #fff;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background .15s;
    flex-shrink: 0;
    padding: 0;
}
.cb-close:hover { background: rgba(255,255,255,.3); }
.cb-close svg   { width: 14px; height: 14px; }

/* Body */
.cb-body {
    height: 280px;
    overflow-y: auto;
    padding: 12px 10px;
    background: var(--surface);
    display: flex;
    flex-direction: column;
    gap: 8px;
    scroll-behavior: smooth;
}
.cb-body::-webkit-scrollbar { width: 3px; }
.cb-body::-webkit-scrollbar-thumb { background: var(--border); border-radius: 2px; }

/* Bubbles */
.cb-wrap {
    display: flex;
    flex-direction: column;
    max-width: 80%;
    animation: cb-in .15s ease;
}
.cb-wrap.cb-me    { align-self: flex-end;   align-items: flex-end; }
.cb-wrap.cb-other { align-self: flex-start; align-items: flex-start; }

.cb-bubble {
    padding: 8px 11px;
    border-radius: 12px;
    font-size: 13.5px;
    line-height: 1.5;
    white-space: pre-wrap;
    word-break: break-word;
}
.cb-me .cb-bubble {
    background: var(--me-bg);
    color: var(--me-fg);
    border-bottom-right-radius: 3px;
}
.cb-other .cb-bubble {
    background: var(--other-bg);
    color: var(--other-fg);
    border-bottom-left-radius: 3px;
    border: 1px solid var(--border);
}
.cb-meta {
    font-size: 10.5px;
    color: var(--muted);
    margin-top: 3px;
    padding: 0 2px;
}

/* Composer */
.cb-composer {
    display: flex;
    align-items: flex-end;
    gap: 8px;
    padding: 9px 10px;
    border-top: 1px solid var(--border);
    background: var(--bg);
    flex-shrink: 0;
}
.cb-input {
    flex: 1;
    resize: none;
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 8px 11px;
    font-family: var(--font);
    font-size: 13.5px;
    color: var(--text);
    background: var(--surface);
    line-height: 1.5;
    outline: none;
    overflow-y: hidden;
    transition: border-color .15s;
}
.cb-input:focus { border-color: var(--accent); }
.cb-input::placeholder { color: var(--muted); }

.cb-send {
    width: 36px; height: 36px;
    flex-shrink: 0;
    border: none;
    border-radius: 50%;
    background: var(--accent);
    color: #fff;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background .15s, transform .12s;
    outline: none;
}
.cb-send:hover { background: var(--accent-d); transform: scale(1.08); }
.cb-send svg   { width: 15px; height: 15px; }

@keyframes cb-in  { from { opacity:0; transform:translateY(5px); } to { opacity:1; transform:none; } }
@keyframes cb-pop { from { transform:scale(.6); } to { transform:scale(1); } }

@media (max-width: 400px) {
    #chatbox-root { bottom: 12px; right: 12px; }
    .cb-panel { width: calc(100vw - 24px); }
}
        `;
        document.head.appendChild(s);
    }
}