-- Thêm cột DiscountPercent vào bảng Products (chạy nếu DB đã tồn tại)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'DiscountPercent'
)
BEGIN
    ALTER TABLE Products
    ADD DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Đã thêm cột DiscountPercent vào bảng Products';
END
ELSE
BEGIN
    PRINT 'Cột DiscountPercent đã tồn tại';
END
