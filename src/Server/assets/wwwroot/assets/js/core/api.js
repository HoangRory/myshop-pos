export const API_MAP = {
  USER_INFO: '/user/info',
  LOGIN: '/auth/login',
  ORDER_LIST: '/order/list'
};

// === Base API ===
export class API {
  constructor() {
  }
  // Hàm để gửi request đến API
  static async request(method, path, options = {}) {
    // fetch để gửi request và nhận response
    const res = await fetch(path, {
      ...options, // Tham số bổ sung từ options
      method, // Phương thức HTTP (GET, POST, PUT, DELETE)
      credentials: 'include' // Gửi cookie (sessionId) kèm theo request  (nếu server dùng cookie, vẫn giữ)
    });

    return res;
  }

  // Hàm GET để lấy dữ liệu từ API
  static GET = (path, params = {}, headers = {}, body = null) => {
    const fullPath = this.buildPath(path, params);
    const options = this.buildOptions(headers, body);
    return this.request('GET', fullPath, options);
  };

  // Hàm POST để gửi dữ liệu đến API
  static POST = (path, params = {}, headers = {}, body = null) => {
    const fullPath = this.buildPath(path, params);
    const options = this.buildOptions(headers, body);
    return this.request('POST', fullPath, options);
  };

  // Hàm PUT để cập nhật dữ liệu trên API
  static PUT = (path, params = {}, headers = {}, body = null) => {
    const fullPath = this.buildPath(path, params);
    const options = this.buildOptions(headers, body);
    return this.request('PUT', fullPath, options);
  };

  // Hàm DELETE để xóa dữ liệu trên API
  static DELETE = (path, params = {}, headers = {}, body = null) => {
    const fullPath = this.buildPath(path, params);
    const options = this.buildOptions(headers, body);
    return this.request('DELETE', fullPath, options);
  };

  // Hàm CUSTOM để gửi request với phương thức tùy chỉnh
  static CUSTOM = (method, path, params = {}, headers = {}, body = null) => {
    const fullPath = this.buildPath(path, params);
    const options = this.buildOptions(headers, body);
    return this.request(method, fullPath, options);
  };

  static getData(response) {
    const contentType = response.headers.get('content-type') || '';
    const isJson = contentType.includes('application/json');
    return isJson ? response.json() : response.text();
  }

  static async getDataSafe(response) {
    const data = await this.getData(response);
    if (!response.ok) throw { status: response.status, data };
    return data;
  }


  static buildOptions(headers = {}, body = null) {
    const options = {};
    options.headers = this.buildHeader(headers);
    options.body = this.buildBody(body, options.headers);
    return options;
  }

  static buildPath(path = '', params = {}) {
    const query = this.buildQuery(params);
    const prefix = path.startsWith('/') ? path : `/${path}`;
    return query ? `${prefix}${query}` : `${prefix}`;
  }

  static buildQuery(params = {}) {
    const query = Object.entries(params)
      .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`)
      .join('&');
    return query ? `?${query}` : '';
  }

  static buildHeader(headers = {}) {
    const h = { ...headers };
    if (!h['Content-Type']) h['Content-Type'] = 'application/json';
    return h;
  }


  static buildBody(body = null, headers) {
    if (body && headers['Content-Type'] === 'application/json') {
      return JSON.stringify(body);
    }
    return body || null;
  }
}



