/* =============================================================================
   InventoryDB, script 06: sample data
   Run after 05, on the fresh database built by 01 (it inserts fixed IDs).

   What it creates
     - lookup rows : 3 user types, 4 stock-movement channels
     - 14 users    : 2 admins (1-2), 4 suppliers (3-6), 8 customers (7-14),
                     each with one contact row
     - 5 categories (Garden has no products, on purpose)
     - 12 products
     - opening stock for 11 products, through dbo.usp_RestockProduct
       (product 12, Desk Lamp, is never restocked: it stays at 0)
     - 12 orders from June to November 2026, through dbo.usp_CreateOrder,
       so every order also writes its stock movements and customer total
     - order 7 cancelled through dbo.usp_CancelOrder (its stock comes back)
     - statuses: orders 1-6, 8, 9 delivered; 10, 11 shipped; 12 pending
     - 2 product edits through dbo.usp_UpdateProduct

   Deliberate edge cases, used by the queries in script 07
     - customers 13 and 14 never ordered; customer 12 ordered only once and
       that order was cancelled
     - products 11 and 12 were never sold
     - products 7 and 9 end at or below their reorder level, product 12 at 0
   ============================================================================= */

USE InventoryDB;
GO
SET NOCOUNT ON;
GO

/* ---------- 1. Lookup tables ---------- */
SET IDENTITY_INSERT dbo.UserTypes ON;
INSERT INTO dbo.UserTypes (TypeNumber, Type) VALUES
    (1, 'Admin'), (2, 'Customer'), (3, 'Supplier');
SET IDENTITY_INSERT dbo.UserTypes OFF;

SET IDENTITY_INSERT dbo.TransactionChannelTypes ON;
INSERT INTO dbo.TransactionChannelTypes (TypeNumber, Type) VALUES
    (1, 'Supplier Restock'),        -- stock received from a supplier
    (2, 'Customer Order'),          -- stock sold on an order
    (3, 'Order Cancellation'),      -- stock returned when an order is cancelled
    (4, 'Manual Adjustment');       -- counting corrections, damage, etc.
SET IDENTITY_INSERT dbo.TransactionChannelTypes OFF;
GO

/* ---------- 2. Users, their contacts, and the sub-type tables ---------- */
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (UserID, FirstName, LastName, UserType, JoinedAt) VALUES
    ( 1, N'Ahmed',   N'Hassan',  1, '2026-01-10'),
    ( 2, N'Mona',    N'Adel',    1, '2026-02-01'),
    ( 3, N'Omar',    N'Farouk',  3, '2026-03-05'),
    ( 4, N'Laila',   N'Samir',   3, '2026-03-12'),
    ( 5, N'Youssef', N'Nabil',   3, '2026-03-20'),
    ( 6, N'Hana',    N'Mostafa', 3, '2026-04-02'),
    ( 7, N'Khaled',  N'Ali',     2, '2026-04-15'),
    ( 8, N'Sara',    N'Ibrahim', 2, '2026-04-18'),
    ( 9, N'Mahmoud', N'Tarek',   2, '2026-04-22'),
    (10, N'Nour',    N'El Din',  2, '2026-05-03'),
    (11, N'Yasmin',  N'Fathy',   2, '2026-05-09'),
    (12, N'Mostafa', N'Kamal',   2, '2026-05-20'),
    (13, N'Rania',   N'Youssef', 2, '2026-06-01'),
    (14, N'Tamer',   N'Said',    2, '2026-06-11');
SET IDENTITY_INSERT dbo.Users OFF;

INSERT INTO dbo.UserContacts (UserID, PhoneNumber, Email, Address) VALUES
    ( 1, '+20 100 111 0001', 'ahmed.hassan@inventory.local',  '5 Nile Corniche, Cairo'),
    ( 2, '+20 100 111 0002', 'mona.adel@inventory.local',     '18 Abbas El Akkad St, Nasr City'),
    ( 3, '+20 122 200 0003', 'omar@techsource.example',       'Smart Village, Building B12, Giza'),
    ( 4, '+20 122 200 0004', 'laila@displaypro.example',      '44 Industrial Zone, 6th of October'),
    ( 5, '+20 122 200 0005', 'youssef@paperline.example',     '9 Port Said St, Alexandria'),
    ( 6, '+20 122 200 0006', 'hana@homeworks.example',        '27 El Obour Industrial City'),
    ( 7, '+20 111 300 0007', 'khaled.ali@mail.example',       '12 Tahrir St, Downtown Cairo'),
    ( 8, '+20 111 300 0008', 'sara.ibrahim@mail.example',     '3 Road 9, Maadi, Cairo'),
    ( 9, '+20 111 300 0009', 'mahmoud.tarek@mail.example',    '70 Gamal Abdel Nasser St, Alexandria'),
    (10, '+20 111 300 0010', 'nour.eldin@mail.example',       '15 El Merghany St, Heliopolis'),
    (11, '+20 111 300 0011', 'yasmin.fathy@mail.example',     '8 Mohandessin Square, Giza'),
    (12, '+20 111 300 0012', 'mostafa.kamal@mail.example',    '21 University St, Mansoura'),
    (13, '+20 111 300 0013', 'rania.youssef@mail.example',    '6 Zamalek Corniche, Cairo'),
    (14, '+20 111 300 0014', 'tamer.said@mail.example',       '40 El Galaa St, Tanta');

INSERT INTO dbo.Admins (AdminID, IsActive) VALUES (1, 1), (2, 1);

INSERT INTO dbo.Suppliers (SupplierID) VALUES (3), (4), (5), (6);          -- IsActive defaults to 1

INSERT INTO dbo.Customers (CustomerID) VALUES                              -- TotalPurchases defaults to 0
    (7), (8), (9), (10), (11), (12), (13), (14);
GO

/* ---------- 3. Categories ---------- */
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (CategoryID, CategoryName, Description, CreatedAt) VALUES
    (1, N'Electronics',     N'Computer accessories and displays',  '2026-04-25'),
    (2, N'Office Supplies', N'Paper, pens and desk supplies',      '2026-04-25'),
    (3, N'Furniture',       N'Office chairs, desks and lighting',  '2026-04-25'),
    (4, N'Kitchen',         N'Office kitchen equipment',           '2026-04-25'),
    (5, N'Garden',          N'Outdoor items (no products yet)',    '2026-04-25');
SET IDENTITY_INSERT dbo.Categories OFF;
GO

/* ---------- 4. Products ---------- */
SET IDENTITY_INSERT dbo.Products ON;
INSERT INTO dbo.Products
       (ProductID, Name, Description, UnitPrice, ReorderLevel, IsActive, CreatedAt, CategoryID, SupplierID) VALUES
    ( 1, N'Wireless Mouse',            N'2.4 GHz, USB receiver',        250.00, 10, 1, '2026-05-01', 1, 3),
    ( 2, N'Mechanical Keyboard',       N'Full size, blue switches',    1200.00,  5, 1, '2026-05-01', 1, 3),
    ( 3, N'27-inch Monitor',           N'QHD IPS panel',               6500.00,  3, 1, '2026-05-01', 1, 4),
    ( 4, N'USB-C Hub',                 N'7-in-1 with HDMI',             850.00, 10, 1, '2026-05-01', 1, 4),
    ( 5, N'A4 Paper (500 sheets)',     N'80 gsm',                       180.00, 50, 1, '2026-05-01', 2, 5),
    ( 6, N'Ballpoint Pens (box of 12)',N'Blue ink',                      60.00, 30, 1, '2026-05-01', 2, 5),
    ( 7, N'Stapler',                   N'Heavy duty, 50 sheets',         95.00, 20, 1, '2026-05-01', 2, 5),
    ( 8, N'Office Chair',              N'Ergonomic, mesh back',        3200.00,  4, 1, '2026-05-01', 3, 6),
    ( 9, N'Standing Desk',             N'Electric, 140 x 70 cm',       9800.00,  5, 1, '2026-05-01', 3, 6),
    (10, N'Electric Kettle',           N'1.7 litre, stainless steel',  1450.00,  5, 1, '2026-05-01', 4, 4),
    (11, N'Coffee Mugs (set of 4)',    N'Ceramic, 350 ml',              320.00, 10, 1, '2026-05-01', 4, 5),
    (12, N'Desk Lamp',                 N'LED, adjustable arm',          700.00,  8, 1, '2026-05-01', 3, 3);
SET IDENTITY_INSERT dbo.Products OFF;
GO

/* ---------- 5. Opening stock (restocks from each product's supplier) ---------- */
EXEC dbo.usp_RestockProduct @ProductID =  1, @Quantity = 100, @UnitCost =  150.00, @TransactionDate = '2026-05-10';
EXEC dbo.usp_RestockProduct @ProductID =  2, @Quantity =  40, @UnitCost =  800.00, @TransactionDate = '2026-05-10';
EXEC dbo.usp_RestockProduct @ProductID =  3, @Quantity =  15, @UnitCost = 5000.00, @TransactionDate = '2026-05-10';
EXEC dbo.usp_RestockProduct @ProductID =  4, @Quantity =  30, @UnitCost =  600.00, @TransactionDate = '2026-05-10';
EXEC dbo.usp_RestockProduct @ProductID =  5, @Quantity = 200, @UnitCost =  120.00, @TransactionDate = '2026-05-11';
EXEC dbo.usp_RestockProduct @ProductID =  6, @Quantity = 100, @UnitCost =   35.00, @TransactionDate = '2026-05-11';
EXEC dbo.usp_RestockProduct @ProductID =  7, @Quantity =  25, @UnitCost =   60.00, @TransactionDate = '2026-05-11';
EXEC dbo.usp_RestockProduct @ProductID =  8, @Quantity =  12, @UnitCost = 2400.00, @TransactionDate = '2026-05-12';
EXEC dbo.usp_RestockProduct @ProductID =  9, @Quantity =   5, @UnitCost = 7500.00, @TransactionDate = '2026-05-12';
EXEC dbo.usp_RestockProduct @ProductID = 10, @Quantity =  20, @UnitCost = 1000.00, @TransactionDate = '2026-05-12';
EXEC dbo.usp_RestockProduct @ProductID = 11, @Quantity =  30, @UnitCost =  200.00, @TransactionDate = '2026-05-12';
-- product 12 (Desk Lamp) is never restocked
GO

/* ---------- 6. Orders (through the procedure, so stock and totals stay right) ----------
   One batch, reusing the same table variable: clear it, fill it, create the order. */
DECLARE @Items dbo.OrderItemList;

-- Order 1: Khaled, June
INSERT INTO @Items VALUES (1, 2), (2, 1);
EXEC dbo.usp_CreateOrder @CustomerID = 7,  @ShippingAddress = N'12 Tahrir St, Downtown Cairo',          @Items = @Items, @OrderDate = '2026-06-03 10:15';
DELETE FROM @Items;

-- Order 2: Sara
INSERT INTO @Items VALUES (3, 1), (4, 2);
EXEC dbo.usp_CreateOrder @CustomerID = 8,  @ShippingAddress = N'3 Road 9, Maadi, Cairo',                @Items = @Items, @OrderDate = '2026-06-15 13:40';
DELETE FROM @Items;

-- Order 3: Mahmoud
INSERT INTO @Items VALUES (5, 10), (6, 5);
EXEC dbo.usp_CreateOrder @CustomerID = 9,  @ShippingAddress = N'70 Gamal Abdel Nasser St, Alexandria',  @Items = @Items, @OrderDate = '2026-06-28 09:05';
DELETE FROM @Items;

-- Order 4: Khaled, July
INSERT INTO @Items VALUES (8, 2);
EXEC dbo.usp_CreateOrder @CustomerID = 7,  @ShippingAddress = N'12 Tahrir St, Downtown Cairo',          @Items = @Items, @OrderDate = '2026-07-05 16:20';
DELETE FROM @Items;

-- Order 5: Nour
INSERT INTO @Items VALUES (1, 3), (7, 2);
EXEC dbo.usp_CreateOrder @CustomerID = 10, @ShippingAddress = N'15 El Merghany St, Heliopolis',         @Items = @Items, @OrderDate = '2026-07-19 11:00';
DELETE FROM @Items;

-- Order 6: Yasmin, August
INSERT INTO @Items VALUES (9, 1), (8, 1);
EXEC dbo.usp_CreateOrder @CustomerID = 11, @ShippingAddress = N'8 Mohandessin Square, Giza',            @Items = @Items, @OrderDate = '2026-08-02 14:30';
DELETE FROM @Items;

-- Order 7: Mostafa (cancelled below)
INSERT INTO @Items VALUES (10, 2), (5, 20);
EXEC dbo.usp_CreateOrder @CustomerID = 12, @ShippingAddress = N'21 University St, Mansoura',            @Items = @Items, @OrderDate = '2026-08-21 10:45';
DELETE FROM @Items;

-- Order 8: Sara, September
INSERT INTO @Items VALUES (2, 2), (1, 5);
EXEC dbo.usp_CreateOrder @CustomerID = 8,  @ShippingAddress = N'3 Road 9, Maadi, Cairo',                @Items = @Items, @OrderDate = '2026-09-09 12:10';
DELETE FROM @Items;

-- Order 9: Mahmoud
INSERT INTO @Items VALUES (3, 2);
EXEC dbo.usp_CreateOrder @CustomerID = 9,  @ShippingAddress = N'70 Gamal Abdel Nasser St, Alexandria',  @Items = @Items, @OrderDate = '2026-09-30 17:55';
DELETE FROM @Items;

-- Order 10: Nour, October
INSERT INTO @Items VALUES (5, 40), (6, 20), (7, 5);
EXEC dbo.usp_CreateOrder @CustomerID = 10, @ShippingAddress = N'15 El Merghany St, Heliopolis',         @Items = @Items, @OrderDate = '2026-10-12 09:30';
DELETE FROM @Items;

-- Order 11: Khaled
INSERT INTO @Items VALUES (4, 3), (10, 1);
EXEC dbo.usp_CreateOrder @CustomerID = 7,  @ShippingAddress = N'12 Tahrir St, Downtown Cairo',          @Items = @Items, @OrderDate = '2026-10-25 15:05';
DELETE FROM @Items;

-- Order 12: Yasmin, November (stays pending)
INSERT INTO @Items VALUES (1, 4), (2, 1);
EXEC dbo.usp_CreateOrder @CustomerID = 11, @ShippingAddress = N'8 Mohandessin Square, Giza',            @Items = @Items, @OrderDate = '2026-11-05 10:00';
GO

/* ---------- 7. Cancel order 7 (its 2 kettles and 20 paper packs return to stock) ---------- */
EXEC dbo.usp_CancelOrder @OrderID = 7;
GO

/* ---------- 8. Order statuses ---------- */
UPDATE dbo.Orders
SET    Status = 'delivered',
       DeliveredAt = DATEADD(DAY, 3, OrderDate)
WHERE  OrderID IN (1, 2, 3, 4, 5, 6, 8, 9);

UPDATE dbo.Orders
SET    Status = 'shipped'
WHERE  OrderID IN (10, 11);
GO

/* ---------- 9. Product edits by admins (recorded in ProductEdits) ---------- */
EXEC dbo.usp_UpdateProduct @ProductID = 11, @AdminID = 2, @ReorderLevel = 12;
EXEC dbo.usp_UpdateProduct @ProductID = 12, @AdminID = 1, @Description = N'LED, adjustable arm, warm white';
GO

/* ---------- 10. Row counts ----------
   Expected: Users 14, UserContacts 14, Admins 2, Suppliers 4, Customers 8,
             Categories 5, Products 12, Orders 12, OrderItems 23,
             StockTransactions 36 (11 restocks + 23 sales + 2 returns),
             ProductEdits 2 */
SELECT 'Users' AS TableName, COUNT(*) AS NumberOfRows FROM dbo.Users
UNION ALL SELECT 'UserContacts',      COUNT(*) FROM dbo.UserContacts
UNION ALL SELECT 'Admins',            COUNT(*) FROM dbo.Admins
UNION ALL SELECT 'Suppliers',         COUNT(*) FROM dbo.Suppliers
UNION ALL SELECT 'Customers',         COUNT(*) FROM dbo.Customers
UNION ALL SELECT 'Categories',        COUNT(*) FROM dbo.Categories
UNION ALL SELECT 'Products',          COUNT(*) FROM dbo.Products
UNION ALL SELECT 'Orders',            COUNT(*) FROM dbo.Orders
UNION ALL SELECT 'OrderItems',        COUNT(*) FROM dbo.OrderItems
UNION ALL SELECT 'StockTransactions', COUNT(*) FROM dbo.StockTransactions
UNION ALL SELECT 'ProductEdits',      COUNT(*) FROM dbo.ProductEdits;
GO
