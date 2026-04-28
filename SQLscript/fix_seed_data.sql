-- ============================================================
-- FIX SEED DATA: Xóa dữ liệu sai (Electronics/Clothing) và
-- thay bằng dữ liệu văn phòng phẩm đúng
-- Chạy script này trong SQL Server Management Studio
-- Kết nối tới database BaseCoreSales
-- ============================================================
USE [BaseCoreSales]
GO

-- Bước 1: Xóa dữ liệu cũ (thứ tự: OrderDetails → Orders → Products → Categories)
PRINT 'Bắt đầu xóa dữ liệu cũ...';

DELETE FROM OrderDetails;
DELETE FROM Orders;
DELETE FROM Products;

-- Xóa categories cũ (Electronics, Clothing, Books, Home & Garden, Sports)
DELETE FROM Categories;

PRINT '✓ Đã xóa toàn bộ dữ liệu cũ';
GO

-- Bước 2: Thêm cột DiscountPercent vào Products nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'DiscountPercent'
)
BEGIN
    ALTER TABLE Products ADD DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT '✓ Đã thêm cột DiscountPercent';
END
GO

-- Bước 3: Thêm cột Note vào Orders nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'Note'
)
BEGIN
    ALTER TABLE Orders ADD Note NVARCHAR(500) NULL;
    PRINT '✓ Đã thêm cột Note vào Orders';
END
GO

-- Bước 4: Chèn Categories văn phòng phẩm đúng
SET IDENTITY_INSERT Categories ON
INSERT INTO Categories (Id, Name, Description) VALUES
(1, N'Văn phòng phẩm',        N'Đồ dùng văn phòng tổng hợp'),
(2, N'Bút viết',               N'Các loại bút bi, mực, dạ'),
(3, N'Giấy in & Vở',           N'Giấy in, vở viết, sổ tay'),
(4, N'Dụng cụ bàn làm việc',   N'Kéo, thước, dập ghim, v.v.'),
(5, N'Kẹp & Ghim',             N'Kẹp giấy, ghim, dây buộc')
SET IDENTITY_INSERT Categories OFF
PRINT '✓ Đã chèn 5 danh mục văn phòng phẩm';
GO

-- Bước 5: Chèn 20 sản phẩm văn phòng phẩm đúng
INSERT INTO Products (Name, Price, Stock, ImageUrl, Description, CategoryId, DiscountPercent) VALUES
(N'Bút bi Thiên Long TL-027',      5000,   500, '', N'Bút bi mực xanh, ngòi 0.7mm, viết mượt mà',                       2, 0),
(N'Bút bi Bến Nghé xanh',           3500,   400, '', N'Bút bi giá rẻ, thông dụng văn phòng',                             2, 0),
(N'Bút mực Pilot V5',               22000,  150, '', N'Bút mực nước màu xanh, ngòi kim 0.5mm',                           2, 10),
(N'Bút dạ quang vàng',              12000,  200, '', N'Bút highlight màu vàng, dạ quang',                                2, 0),
(N'Bút xóa nước trắng',             8000,   120, '', N'Bút xóa nhanh khô, không lem',                                    2, 0),
(N'Vở ô ly 200 trang',              18000,  300, '', N'Vở học sinh 200 trang, kẻ ô ly 5mm',                             3, 0),
(N'Tập giấy A4 Bãi Bằng 70gsm',    55000,  100, '', N'500 tờ giấy A4, trắng sáng, 70gsm',                               3, 5),
(N'Giấy ghi chú Post-it 76x76mm',   25000,  80,  '', N'100 tờ/xấp, nhiều màu sắc',                                      3, 0),
(N'Sổ tay bìa cứng A5',             45000,  60,  '', N'Sổ tay 96 trang, kẻ dòng',                                       3, 0),
(N'Kéo văn phòng 21cm',             32000,  70,  '', N'Kéo inox sắc bén, cán nhựa chống trượt',                         4, 0),
(N'Thước kẻ nhựa 30cm',             10000,  150, '', N'Thước nhựa trong suốt, chia cm và mm',                           4, 0),
(N'Dập ghim Kangaro 26/6',           65000,  40,  '', N'Dập ghim văn phòng, đóng được 20 tờ',                            4, 15),
(N'Ghim bấm 26/6 hộp 1000 cái',     15000,  200, '', N'Ghim thép không gỉ, phù hợp dập ghim thông thường',             5, 0),
(N'Kẹp giấy bướm số 2 hộp',         12000,  180, '', N'Hộp 12 cái kẹp bướm màu đen, kẹp chắc',                         5, 0),
(N'Kẹp giấy kim hộp 100 cái',       8000,   250, '', N'Kẹp giấy hình chữ L kim loại, tiện dụng',                       5, 0),
(N'Băng dính trong 48mm',            18000,  90,  '', N'Cuộn băng dính trong suốt, không vàng ố',                       4, 0),
(N'Hộp đựng bút để bàn',            55000,  35,  '', N'Hộp nhựa đựng bút đa năng, 3 ngăn',                             4, 10),
(N'Máy tính Casio FX-570VN',         250000, 25,  '', N'Máy tính khoa học 417 hàm, dùng cho học sinh - sinh viên',     4, 0),
(N'Bút chì 2B Staedtler',            5000,   300, '', N'Bút chì gỗ ruột mềm 2B, vẽ và viết',                            2, 0),
(N'Tẩy nhựa Pentel Hi-Polymer',      8000,   200, '', N'Tẩy sạch, không gây xước giấy',                                 4, 0)

PRINT '✓ Đã chèn 20 sản phẩm văn phòng phẩm';
GO

-- Kiểm tra kết quả
SELECT 'Categories' AS [Bảng], COUNT(*) AS [Số lượng] FROM Categories
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Orders', COUNT(*) FROM Orders
GO
