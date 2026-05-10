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
    name NVARCHAR(100) UNIQUE NOT NULL,
    description NVARCHAR(MAX),
    is_active BIT DEFAULT 1 -- Thêm để quản lý ẩn/hiện loại SP
);
GO

CREATE TABLE product (
    product_id INT PRIMARY KEY IDENTITY(1,1),
    sku VARCHAR(50) UNIQUE NOT NULL, -- Mã định danh sản phẩm
    name NVARCHAR(200) UNIQUE NOT NULL,
    import_price DECIMAL(18, 2) NOT NULL,
    sale_price DECIMAL(18, 2) NOT NULL,
    stock_count INT DEFAULT 0,
    description NVARCHAR(MAX),
    images NVARCHAR(MAX), -- Cột lưu danh sách ảnh (nên lưu dạng JSON array hoặc string cách nhau dấu ;)
    category_id INT FOREIGN KEY REFERENCES category(category_id),
    updated_at DATETIME DEFAULT GETDATE(),
    is_active BIT DEFAULT 1
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
    note NVARCHAR(MAX),
    is_active BIT DEFAULT 1
);
GO

CREATE TABLE order_item (
    order_item_id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT FOREIGN KEY REFERENCES [order](order_id),
    product_id INT FOREIGN KEY REFERENCES product(product_id),
    quantity INT NOT NULL,
    unit_price DECIMAL(18, 2) NOT NULL, -- Giá bán tại thời điểm chốt đơn
    total_item_price AS (quantity * unit_price), -- Cột tính toán tự động
    is_active BIT DEFAULT 1
);
GO

---------------------------------------------------------
-- DỮ LIỆU MẪU (SEED DATA)
---------------------------------------------------------

INSERT INTO category (name, description) VALUES 
(N'CPU - Bộ vi xử lý', N'Các dòng CPU từ Intel và AMD'),
(N'VGA - Card màn hình', N'Card đồ họa chơi game và đồ họa'),
(N'Mainboard - Bo mạch chủ', N'Bo mạch chủ cho PC');
GO

DECLARE @cat_id INT;
DECLARE @p_idx INT;
DECLARE @p_name NVARCHAR(200);
DECLARE @sku VARCHAR(50);
DECLARE @img_str NVARCHAR(MAX);

-- Vòng lặp cho 3 Category
SET @cat_id = 1;
WHILE @cat_id <= 3
BEGIN
    SET @p_idx = 1;
    -- Mỗi Category 22 sản phẩm
    WHILE @p_idx <= 22
    BEGIN
        SET @sku = CASE @cat_id 
                    WHEN 1 THEN 'CPU-' WHEN 2 THEN 'VGA-' ELSE 'MB-' END 
                    + RIGHT('00' + CAST(@p_idx AS VARCHAR), 3);
        
        SET @p_name = CASE @cat_id 
                        WHEN 1 THEN N'CPU ' + (CASE WHEN @p_idx % 2 = 0 THEN 'Intel Core i' ELSE 'AMD Ryzen ' END) + CAST(@p_idx AS NVARCHAR)
                        WHEN 2 THEN N'VGA NVIDIA/AMD Edition ' + CAST(@p_idx AS NVARCHAR)
                        ELSE N'Mainboard Series Z/B/H ' + CAST(@p_idx AS NVARCHAR) 
                      END;

        -- Gen ảnh theo format: productid_1.png;productid_2.png;productid_3.png
        -- Vì ID tự tăng, chúng ta dùng biến đếm tạm thời để giả lập ảnh
        -- Sau khi Insert, ID thực tế sẽ khớp với logic hiển thị của ông
        SET @img_str = 'p' + CAST(((@cat_id-1)*22 + @p_idx) AS NVARCHAR) + '_1.png;' +
                       'p' + CAST(((@cat_id-1)*22 + @p_idx) AS NVARCHAR) + '_2.png;' +
                       'p' + CAST(((@cat_id-1)*22 + @p_idx) AS NVARCHAR) + '_3.png';

        INSERT INTO product (sku, name, import_price, sale_price, stock_count, description, images, category_id)
        VALUES (
            @sku, 
            @p_name, 
            2000000 + (RAND() * 5000000), -- Giá nhập 2tr - 7tr
            3000000 + (RAND() * 8000000), -- Giá bán 3tr - 11tr
            10 + (CAST(RAND() * 50 AS INT)), -- Tồn kho 10 - 60
            N'Thông tin chi tiết cho ' + @p_name,
            @img_str,
            @cat_id
        );
        SET @p_idx = @p_idx + 1;
    END
    SET @cat_id = @cat_id + 1;
END;
GO


-- Account
INSERT INTO account (username, password_hash, role) VALUES 
('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', N'Admin'), -- pass: admin123
('staff01', 'ede7f95081f9a2e37e96a40562e841f3e0980e1816e9c60e3745a557ef22ec01', N'Staff'); -- pass: staff123

-- Voucher
INSERT INTO discount_voucher (voucher_code, discount_type, discount_value, expiry_date, min_order_value) VALUES 
('BUILDPC', 1, 500000, '2026-12-31', 10000000),
('LUCIFER', 2, 10, '2026-12-31', 0);

-- Order mẫu (Để test Report)
INSERT INTO [order] (account_id, sub_total, final_total, status, payment_method, is_active) 
VALUES (1, 15000000, 14500000, 1, 1, 1);

INSERT INTO order_item (order_id, product_id, quantity, unit_price) 
VALUES (1, 1, 1, 8000000), (1, 23, 1, 7000000);
GO