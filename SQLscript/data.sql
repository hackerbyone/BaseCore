INSERT INTO Categories (Name, Description) VALUES
(N'Electronics', N'Thiết bị điện tử'),
(N'Laptop', N'Máy tính xách tay'),
(N'Phone', N'Điện thoại'),
(N'Accessory', N'Phụ kiện'),
(N'Camera', N'Máy ảnh'),
(N'Gaming', N'Thiết bị gaming'),
(N'Office', N'Văn phòng'),
(N'Home', N'Gia dụng'),
(N'Smart Home', N'Nhà thông minh'),
(N'Audio', N'Âm thanh'),
(N'Wearable', N'Thiết bị đeo'),
(N'Networking', N'Mạng'),
(N'Storage', N'Lưu trữ'),
(N'Monitor', N'Màn hình'),
(N'Printer', N'Máy in'),
(N'Keyboard', N'Bàn phím'),
(N'Mouse', N'Chuột'),
(N'Tablet', N'Máy tính bảng'),
(N'Software', N'Phần mềm'),
(N'Security', N'Bảo mật'),
(N'Cooling', N'Tản nhiệt'),
(N'Power', N'Nguồn'),
(N'GPU', N'Card đồ họa'),
(N'CPU', N'Vi xử lý'),
(N'RAM', N'Bộ nhớ'),
(N'Mainboard', N'Bo mạch chủ'),
(N'Case', N'Vỏ máy'),
(N'Fan', N'Quạt'),
(N'Lighting', N'LED'),
(N'VR', N'Thực tế ảo'),
(N'Drone', N'Máy bay'),
(N'AI Device', N'Thiết bị AI'),
(N'Robot', N'Robot'),
(N'Car Tech', N'Công nghệ xe'),
(N'Fitness', N'Thể thao'),
(N'Health', N'Sức khỏe'),
(N'Education', N'Giáo dục'),
(N'Server', N'Máy chủ'),
(N'Cloud', N'Điện toán đám mây'),
(N'Other', N'Khác');

INSERT INTO Products (Name, Price, Stock, ImageUrl, Description, CategoryId) VALUES
(N'Laptop Dell XPS', 30000000, 10, 'img1.jpg', N'Laptop cao cấp', 2),
(N'iPhone 15', 25000000, 20, 'img2.jpg', N'Điện thoại Apple', 3),
(N'Chuột Logitech', 500000, 50, 'img3.jpg', N'Chuột không dây', 17),
(N'Bàn phím cơ', 1500000, 30, 'img4.jpg', N'Gaming keyboard', 16),
(N'Màn hình LG', 5000000, 15, 'img5.jpg', N'4K Monitor', 14),
(N'Laptop HP', 20000000, 12, 'img6.jpg', N'Laptop văn phòng', 2),
(N'Samsung S23', 22000000, 25, 'img7.jpg', N'Android flagship', 3),
(N'Tai nghe Sony', 3000000, 40, 'img8.jpg', N'Noise cancelling', 10),
(N'Router TP-Link', 1200000, 18, 'img9.jpg', N'WiFi 6', 12),
(N'SSD Samsung', 2000000, 22, 'img10.jpg', N'1TB SSD', 13),

(N'Product 11', 100000, 10, 'img.jpg', N'Mô tả', 1),
(N'Product 12', 110000, 10, 'img.jpg', N'Mô tả', 2),
(N'Product 13', 120000, 10, 'img.jpg', N'Mô tả', 3),
(N'Product 14', 130000, 10, 'img.jpg', N'Mô tả', 4),
(N'Product 15', 140000, 10, 'img.jpg', N'Mô tả', 5),
(N'Product 16', 150000, 10, 'img.jpg', N'Mô tả', 6),
(N'Product 17', 160000, 10, 'img.jpg', N'Mô tả', 7),
(N'Product 18', 170000, 10, 'img.jpg', N'Mô tả', 8),
(N'Product 19', 180000, 10, 'img.jpg', N'Mô tả', 9),
(N'Product 20', 190000, 10, 'img.jpg', N'Mô tả', 10),
(N'Product 21', 200000, 10, 'img.jpg', N'Mô tả', 11),
(N'Product 22', 210000, 10, 'img.jpg', N'Mô tả', 12),
(N'Product 23', 220000, 10, 'img.jpg', N'Mô tả', 13),
(N'Product 24', 230000, 10, 'img.jpg', N'Mô tả', 14),
(N'Product 25', 240000, 10, 'img.jpg', N'Mô tả', 15),
(N'Product 26', 250000, 10, 'img.jpg', N'Mô tả', 16),
(N'Product 27', 260000, 10, 'img.jpg', N'Mô tả', 17),
(N'Product 28', 270000, 10, 'img.jpg', N'Mô tả', 18),
(N'Product 29', 280000, 10, 'img.jpg', N'Mô tả', 19),
(N'Product 30', 290000, 10, 'img.jpg', N'Mô tả', 20),
(N'Product 31', 300000, 10, 'img.jpg', N'Mô tả', 21),
(N'Product 32', 310000, 10, 'img.jpg', N'Mô tả', 22),
(N'Product 33', 320000, 10, 'img.jpg', N'Mô tả', 23),
(N'Product 34', 330000, 10, 'img.jpg', N'Mô tả', 24),
(N'Product 35', 340000, 10, 'img.jpg', N'Mô tả', 25),
(N'Product 36', 350000, 10, 'img.jpg', N'Mô tả', 26),
(N'Product 37', 360000, 10, 'img.jpg', N'Mô tả', 27),
(N'Product 38', 370000, 10, 'img.jpg', N'Mô tả', 28),
(N'Product 39', 380000, 10, 'img.jpg', N'Mô tả', 29),
(N'Product 40', 390000, 10, 'img.jpg', N'Mô tả', 30);

INSERT INTO Users 
(Id, Name, UserName, Password, Salt, Contact, Email, Phone, Position, Image, IsActive, UserType, Created)
VALUES
(NEWID(), N'Nguyen Van A', 'user1', 'pass11', 0x, N'HN', 'user1@gmail.com', '0900000001', 'Staff', 'img.jpg', 1, 1, GETDATE()),
(NEWID(), N'Nguyen Van B', 'user2', 'pass11', 0x, N'HN', 'user2@gmail.com', '0900000002', 'Staff', 'img.jpg', 1, 0, GETDATE());

INSERT INTO Orders (UserId, OrderDate, TotalAmount, Status, ShippingAddress)
SELECT TOP 40 
    Id,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 30, GETDATE()),
    ABS(CHECKSUM(NEWID())) % 5000000 + 100000,
    N'Completed',
    N'Ha Noi'
FROM Users;

INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, OrderId1)
SELECT TOP 40
    o.Id,
    p.Id,
    ABS(CHECKSUM(NEWID())) % 5 + 1,
    p.Price,
    NULL
FROM Orders o
CROSS JOIN Products p
ORDER BY NEWID();