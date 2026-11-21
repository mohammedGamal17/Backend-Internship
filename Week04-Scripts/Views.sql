Use InventoryDB
GO

CREATE OR ALTER VIEW ViewCurrentInventory
AS 
SELECT	p.ProductID,
		p.Name AS ProductName,
		p.IsActive,
        c.CategoryName,
        p.SupplierID,
        u.FirstName + N' ' + u.LastName   AS SupplierName,
        p.UnitPrice,
        p.ReorderLevel,
        ISNULL(t.AvailableStock, 0) AS AvailableStock,
        CASE
            WHEN ISNULL(t.AvailableStock, 0) <= 0              THEN 'Out of stock'
            WHEN ISNULL(t.AvailableStock, 0) <= p.ReorderLevel THEN 'Low stock'
            ELSE 'In stock'
        END                                        AS StockStatus
FROM Products p
JOIN Categories c on c.CategoryID = p.CategoryID
JOIN Suppliers s on s.SupplierID = p.SupplierID
JOIN Users u on u.UserID = s.SupplierID
LEFT JOIN
(
    SELECT  ProductID, SUM(QuantityChange) AS AvailableStock
    FROM    dbo.StockTransactions
    GROUP BY ProductID
) t ON t.ProductID = p.ProductID;
GO



CREATE VIEW ViewCustomerOrderHistory
AS
SELECT  cu.CustomerID,
        u.FirstName + N' ' + u.LastName   AS CustomerName,
        o.OrderID,
        o.OrderDate,
        o.Status,
        o.IsCancelled,
        o.ShippingAddress,
        o.DeliveredAt,
        ISNULL(t.TotalUnits, 0) AS TotalUnits,
        ISNULL(t.OrderTotal, 0) AS OrderTotal
FROM    Orders    o
JOIN    Customers cu ON cu.CustomerID = o.CustomerID
JOIN    Users     u  ON u.UserID = cu.CustomerID
LEFT JOIN
(
    SELECT  OrderID,
            SUM(Quantity) AS TotalUnits,
            SUM(Quantity * UnitPrice)  AS OrderTotal
    FROM    OrderItems
    GROUP BY OrderID
) t ON t.OrderID = o.OrderID;
GO



CREATE VIEW ViewMonthlySales
AS
SELECT YEAR(o.OrderDate) AS SalesYear,
        MONTH(o.OrderDate) AS SalesMonth,
        DATEFROMPARTS(YEAR(o.OrderDate), MONTH(o.OrderDate), 1) AS MonthStart,
        COUNT(DISTINCT o.OrderID) AS OrderCount,
        SUM(oi.Quantity) AS UnitsSold,
        SUM(oi.Quantity * oi.UnitPrice) AS TotalSales
FROM Orders o
JOIN OrderItems oi ON oi.OrderID = o.OrderID
WHERE o.IsCancelled=0
GROUP BY YEAR(o.OrderDate), MONTH(o.OrderDate);
GO


CREATE VIEW ViewBestSellingProducts
AS
SELECT  p.ProductID,
        p.Name AS ProductName,
        c.CategoryName,
        SUM(oi.Quantity) AS UnitsSold,
        SUM(oi.Quantity * oi.UnitPrice)AS Revenue,
        COUNT(DISTINCT o.OrderID)  AS OrderCount,
        DENSE_RANK() OVER (ORDER BY SUM(oi.Quantity) DESC) AS SalesRank
FROM    OrderItems oi
JOIN    Orders     o ON o.OrderID = oi.OrderID
JOIN    Products   p ON p.ProductID = oi.ProductID
JOIN    Categories c ON c.CategoryID = p.CategoryID
WHERE   o.IsCancelled = 0
GROUP BY p.ProductID, p.Name, c.CategoryName;
GO