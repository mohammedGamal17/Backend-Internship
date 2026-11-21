USE InventoryDB;
GO

CREATE FUNCTION GetOrderValue(@OrderID INT)
RETURNS DECIMAL(12,2)
AS
BEGIN
    DECLARE @Total DECIMAL(12,2);

    SELECT @Total = SUM(oi.Quantity * oi.UnitPrice)
    FROM   dbo.OrderItems oi
    WHERE  oi.OrderID = @OrderID;

     RETURN ISNULL(@Total, 0);
END
GO



CREATE FUNCTION GetProductAvailableStock(@ProductID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Stock INT;

    SELECT @Stock = SUM(st.QuantityChange)
    FROM   dbo.StockTransactions st
    WHERE  st.ProductID = @ProductID;

    RETURN ISNULL(@Stock, 0);
END
GO


CREATE FUNCTION GetCustomerTotalPurchases(@CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Total DECIMAL(12,2);

    SELECT @Total = SUM(oi.Quantity * oi.UnitPrice)
    FROM   Orders o
    JOIN   OrderItems oi ON oi.OrderID = o.OrderID
    WHERE  o.CustomerID = @CustomerID
      AND  o.IsCancelled = 0;

    RETURN ISNULL(@Total, 0);
END
GO