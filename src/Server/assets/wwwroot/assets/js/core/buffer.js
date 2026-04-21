/**
 * @author: Nguyen Minh Thuan (thuangf45)
 * Ported to JavaScript (ES2022+)
 */
export class LuciferBuffer {
    // #region Private fields and constants
    static MAX_BUFFER_CAPACITY = 2147483647; // int.MaxValue
    static DEFAULT_CAPACITY = 256;
    static MAX_RETAINED_CAPACITY = 1024 * 256;

    #data = new Uint8Array(0); // _data
    #size = 0;                 // Size
    #offset = 0;               // Offset
    #view = null;              // DataView để đọc ghi binary
    // #endregion

    // #region Constructors
    constructor(arg) {
        if (typeof arg === 'number') {
            this.attach(arg);
        } else if (arg instanceof Uint8Array) {
            this.attach(arg, 0, arg.length);
        } else {
            this.attach();
        }
    }
    // #endregion

    // #region Public properties
    get isEmpty() { return this.#data.length === 0 || this.#size === 0; }
    
    get isValid() { 
        return this.#data !== null && this.#size >= 0 && 
               this.#offset >= 0 && this.#offset <= this.#size && 
               this.#size <= this.#data.length; 
    }

    get data() { return this.#data; }
    
    get capacity() { return this.#data.length; }

    get size() { return this.#size; }

    get offset() { return this.#offset; }

    // Indexer [long index] trong JS dùng Proxy hoặc hàm lấy giá trị
    getAt(index) {
        if (index < 0 || index >= this.#size) throw new Error("Index out of range");
        return this.#data[index];
    }
    // #endregion

    // #region Memory buffer methods
    // Lấy một phần của mảng (tương đương AsSpan/AsMemory)
    asUint8Array(start = this.#offset, length = this.#size - this.#offset) {
        return this.#data.subarray(start, start + length);
    }

    toString() {
        return this.extractString(0, this.#size);
    }

    extractString(offset, size) {
        const decoder = new TextDecoder();
        return decoder.decode(this.#data.subarray(offset, offset + size));
    }

    remove(offset, size) {
        const remaining = this.#size - offset - size;
        if (remaining > 0) {
            // Dùng .set() để copy đè (tương đương Lucifer.Copy)
            this.#data.set(this.#data.subarray(offset + size, this.#size), offset);
        }
        this.#size -= size;
        if (this.#offset >= offset + size) this.#offset -= size;
        else if (this.#offset >= offset) this.#offset = offset;
    }

    reserve(capacity) {
        if (capacity > this.capacity) {
            const newCapacity = Math.min(
                Math.max(capacity, this.capacity * 2), 
                LuciferBuffer.MAX_BUFFER_CAPACITY
            );
            const newData = new Uint8Array(newCapacity);
            if (this.#size > 0) {
                newData.set(this.#data.subarray(0, this.#size));
            }
            // Trong JS không cần Lucifer.Return(Data) vì GC tự lo, 
            // trừ khi bạn tự viết Pool quản lý mảng.
            this.#data = newData;
            this.#view = new DataView(this.#data.buffer);
        }
    }

    resize(size) {
        this.reserve(size);
        this.#size = size;
        if (this.#offset > this.#size) this.#offset = this.#size;
    }

    shift(offset) {
        const newOffset = this.#offset + offset;
        if (newOffset < 0 || newOffset > this.#size) throw new Error("Invalid shift");
        this.#offset = newOffset;
    }

    unshift(offset) {
        const newOffset = this.#offset - offset;
        if (newOffset < 0) throw new Error("Unshift would create negative offset");
        this.#offset = newOffset;
    }

    // Reset() - Trong JS thường để GC xử lý, hoặc xóa data
    reset() {
        if (this.#data.length > LuciferBuffer.MAX_RETAINED_CAPACITY) {
            this.attach(LuciferBuffer.DEFAULT_CAPACITY);
        }
        this.#size = 0;
        this.#offset = 0;
    }
    // #endregion

    // #region Attach/Detach methods
    #updateView() {
        this.#view = new DataView(this.#data.buffer);
    }

    detach() {
        this.#data = new Uint8Array(0);
        this.#size = 0;
        this.#offset = 0;
        this.#view = null;
    }

    attach(capacityOrData = 0) {
        if (typeof capacityOrData === 'number') {
            this.#data = new Uint8Array(capacityOrData);
            this.#size = 0;
        } else {
            this.#data = capacityOrData;
            this.#size = capacityOrData.length;
        }
        this.#offset = 0;
        this.#updateView();
    }
    // #endregion

    // #region Append Bytes and Spans
    appendByte(value) {
        this.reserve(this.#size + 1);
        this.#data[this.#size] = value;
        this.#size += 1;
        return 1;
    }

    append(buffer) {
        if (buffer.length === 0) return 0;
        this.reserve(this.#size + buffer.length);
        this.#data.set(buffer, this.#size);
        this.#size += buffer.length;
        return buffer.length;
    }

    appendString(text) {
        const encoder = new TextEncoder();
        const encoded = encoder.encode(text);
        return this.append(encoded);
    }
    // #endregion

    // #region Append binary number overloads (Little Endian)
    appendInt32(value) {
        this.reserve(this.#size + 4);
        this.#view.setInt32(this.#size, value, true); // true = Little Endian
        this.#size += 4;
        return 4;
    }

    appendInt64(value) { // value phải là BigInt (ví dụ: 100n)
        this.reserve(this.#size + 8);
        this.#view.setBigInt64(this.#size, BigInt(value), true);
        this.#size += 8;
        return 8;
    }

    appendFloat32(value) {
        this.reserve(this.#size + 4);
        this.#view.setFloat32(this.#size, value, true);
        this.#size += 4;
        return 4;
    }
    // #endregion

    // #region Read binary number overloads
    readInt16(offset) { return this.#view.getInt16(offset, true); }
    readInt32(offset) { return this.#view.getInt32(offset, true); }
    readFloat32(offset) { return this.#view.getFloat32(offset, true); }
    readBigInt64(offset) { return this.#view.getBigInt64(offset, true); }
    // #endregion
}