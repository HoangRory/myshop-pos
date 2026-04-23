CREATE DATABASE MyShop;
GO

USE MyShop;
GO

CREATE TABLE account(
	account_id INT PRIMARY KEY IDENTITY(1,1),
	username VARCHAR(50) UNIQUE NOT NULL,
	password_hash VARCHAR(256) NOT NULL, -- Hash nhận từ Client
	email VARCHAR(100),
	phone VARCHAR(20),
	role NVARCHAR(20) DEFAULT N'Staff', -- Admin, Staff
	avatar_url NVARCHAR(500),
	is_active BIT DEFAULT 1,
	created_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE category (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    is_active BIT DEFAULT 1 -- Thêm để quản lý ẩn/hiện loại SP
);
GO

CREATE TABLE product (
    product_id INT PRIMARY KEY IDENTITY(1,1),
    sku VARCHAR(50) UNIQUE NOT NULL, -- Mã định danh sản phẩm
    name NVARCHAR(200) NOT NULL,
    import_price DECIMAL(18, 2) NOT NULL,
    sale_price DECIMAL(18, 2) NOT NULL,
    stock_count INT DEFAULT 0,
    description NVARCHAR(MAX),
    images NVARCHAR(MAX), -- Cột lưu danh sách ảnh (nên lưu dạng JSON array hoặc string cách nhau dấu ;)
    category_id INT FOREIGN KEY REFERENCES category(category_id),
    updated_at DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE discount_voucher (
    voucher_code VARCHAR(20) PRIMARY KEY,
    discount_type TINYINT NOT NULL, -- 1: Số tiền cố định, 2: Phần trăm (%)
    discount_value DECIMAL(18, 2) NOT NULL,
    min_order_value DECIMAL(18, 2) DEFAULT 0, -- Điều kiện đơn hàng tối thiểu
    max_discount_amount DECIMAL(18, 2), -- Giới hạn giảm tối đa nếu dùng %
    expiry_date DATETIME NOT NULL,
    is_active BIT DEFAULT 1
);
GO

CREATE TABLE [order] (
    order_id INT PRIMARY KEY IDENTITY(1,1),
    account_id INT FOREIGN KEY REFERENCES account(account_id),
    created_at DATETIME DEFAULT GETDATE(),
    status TINYINT DEFAULT 0, -- Trạng thái: 0: Mới tạo (Pending), 1: Đã thanh toán (Paid), 2: Đã hủy (Cancelled)
    payment_method TINYINT DEFAULT 0,  -- PaymentMethod: 0: Tiền mặt, 1: Chuyển khoản, 2: Thẻ...
    sub_total DECIMAL(18, 2) NOT NULL, -- Tổng tiền trước giảm giá
    voucher_code VARCHAR(20) FOREIGN KEY REFERENCES discount_voucher(voucher_code),
    discount_amount DECIMAL(18, 2) DEFAULT 0, -- Số tiền thực tế được giảm
    final_total DECIMAL(18, 2) NOT NULL, -- Tổng tiền khách trả thực tế
    note NVARCHAR(MAX)
);
GO

CREATE TABLE order_item (
    order_item_id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT FOREIGN KEY REFERENCES [order](order_id),
    product_id INT FOREIGN KEY REFERENCES product(product_id),
    quantity INT NOT NULL,
    unit_price DECIMAL(18, 2) NOT NULL, -- Giá bán tại thời điểm chốt đơn
    total_item_price AS (quantity * unit_price) -- Cột tính toán tự động
);
GO

---------------------------------------------------------
-- DỮ LIỆU MẪU (SEED DATA)
---------------------------------------------------------

-- Seed Account
INSERT INTO account (username, password_hash, role) VALUES 
('admin', 'hash_of_admin_password', N'Admin'),
('staff01', 'hash_of_staff_password', N'Staff');

-- Seed Category
INSERT INTO category (name, description) VALUES 
(N'Laptops', N'High performance laptops'), (N'Phones', N'Smartphones'),
(N'Audio', N'Headphones'), (N'Gaming', N'Consoles'), (N'Mice', N'Mice');

-- Seed Product (50 record mẫu với images giả lập)
DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
    INSERT INTO product (sku, name, import_price, sale_price, stock_count, category_id, images)
    VALUES (
        'SKU-' + CAST(@i AS VARCHAR), 
        N'Sản phẩm mẫu số ' + CAST(@i AS NVARCHAR), 
        100.00 + (@i * 2), 
        150.00 + (@i * 3), 
        10 + @i, 
        (CAST(RAND() * 4 AS INT) + 1),
        N'https://img.myshop.com/p' + CAST(@i AS NVARCHAR) + '_1.jpg;https://img.myshop.com/p' + CAST(@i AS NVARCHAR) + '_2.jpg'
    );
    SET @i = @i + 1;
END;

-- Seed Voucher
INSERT INTO discount_voucher (voucher_code, discount_type, discount_value, expiry_date) VALUES 
('KHAIXUAN', 1, 50000, '2026-12-31'),
('GIAM20', 2, 20, '2026-12-31');

-- Seed Order & OrderItem mẫu (status=1: Đã thanh toán, payment_method=0: Tiền mặt)
INSERT INTO [order] (account_id, sub_total, final_total, status, payment_method) 
VALUES (1, 300.00, 300.00, 1, 0);

INSERT INTO order_item (order_id, product_id, quantity, unit_price) 
VALUES (1, 1, 2, 150.00);