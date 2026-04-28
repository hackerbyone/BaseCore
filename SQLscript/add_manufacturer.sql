-- Migration: Thêm bảng Manufacturers và liên kết với Products
-- Chạy script này nếu cần migrate thủ công (Program.cs đã tự động apply)

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Manufacturers]') AND type = 'U')
BEGIN
    CREATE TABLE [Manufacturers] (
        [Id]          int           NOT NULL IDENTITY,
        [Name]        nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Website]     nvarchar(200) NULL,
        [Phone]       nvarchar(20)  NULL,
        CONSTRAINT [PK_Manufacturers] PRIMARY KEY ([Id])
    );

    INSERT INTO [Manufacturers] ([Name],[Description],[Website],[Phone]) VALUES
    (N'Thiên Long',   N'Thương hiệu bút viết hàng đầu Việt Nam', N'thienlong.com.vn', NULL),
    (N'Pilot',        N'Thương hiệu bút Nhật Bản nổi tiếng',     NULL, NULL),
    (N'Staedtler',    N'Thương hiệu dụng cụ viết của Đức',        NULL, NULL),
    (N'Pentel',       N'Thương hiệu văn phòng phẩm Nhật Bản',     NULL, NULL),
    (N'Kangaro',      N'Thương hiệu dụng cụ văn phòng Ấn Độ',     NULL, NULL),
    (N'Casio',        N'Thương hiệu điện tử Nhật Bản',             NULL, NULL),
    (N'3M / Post-it', N'Thương hiệu văn phòng phẩm Mỹ',           NULL, NULL),
    (N'Nội địa',      N'Sản phẩm sản xuất trong nước',             NULL, NULL);

    PRINT 'Manufacturers table created with seed data.';
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ManufacturerId'
)
BEGIN
    ALTER TABLE [Products] ADD [ManufacturerId] int NULL;
    ALTER TABLE [Products]
        ADD CONSTRAINT [FK_Products_Manufacturers_ManufacturerId]
        FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers]([Id])
        ON DELETE SET NULL;
    PRINT 'ManufacturerId column added to Products.';
END
