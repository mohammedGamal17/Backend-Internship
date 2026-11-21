
USE InventoryDB;
GO

IF TYPE_ID(N'dbo.OrderItemList') IS NULL
    CREATE TYPE dbo.OrderItemList AS TABLE
    (
        ProductID  INT NOT NULL PRIMARY KEY,
        Quantity   INT NOT NULL CHECK (Quantity > 0)
    );
GO


CREATE Procedure AddProduct
    @Name           NVARCHAR(150),
    @UnitPrice      DECIMAL(10,2),
    @CategoryID     INT,
    @SupplierID     INT,
    @Description    NVARCHAR(500) = NULL,
    @ReorderLevel   INT           = 10,
    @AdminID        INT           = NULL,
    @InitialStock   INT           = 0,
    @UnitCost       DECIMAL(10,2),
    @NewProductID   INT           = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Channel INT;

    BEGIN TRY
        IF NULLIF(LTRIM(RTRIM(@Name)), N'') IS NULL
            THROW 50008, N'Product name is required.', 1;

        IF @UnitPrice IS NULL OR @UnitPrice < 0
            THROW 50009, N'Unit price cannot be null or negative.', 1;

        IF @ReorderLevel IS NULL OR @ReorderLevel < 0
            THROW 50015, N'Reorder level cannot be null or negative.', 1;

        IF @InitialStock IS NULL OR @InitialStock < 0
            THROW 50014, N'Initial stock cannot be null or negative.', 1;

        IF @InitialStock > 0 AND (@UnitCost IS NULL OR @UnitCost < 0)
            THROW 50016, N'A valid, non-negative unit cost is required when initial stock is greater than zero.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryID = @CategoryID)
            THROW 50010, N'The category does not exist.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE SupplierID = @SupplierID AND IsActive = 1)
            THROW 50011, N'The supplier does not exist or is inactive.', 1;

        IF @AdminID IS NOT NULL 
           AND NOT EXISTS (SELECT 1 FROM dbo.Admins WHERE AdminID = @AdminID AND IsActive = 1)
            THROW 50013, N'The admin does not exist or is inactive.', 1;

       
        BEGIN TRANSACTION;

            INSERT INTO dbo.Products (Name, Description, UnitPrice, ReorderLevel, CategoryID, SupplierID)
            VALUES (LTRIM(RTRIM(@Name)), @Description, @UnitPrice, @ReorderLevel, @CategoryID, @SupplierID);

            SET @NewProductID = CAST(SCOPE_IDENTITY() AS INT);

            IF @InitialStock > 0
            BEGIN
                INSERT INTO dbo.StockTransactions (TypeOfChange, QuantityChange, Amount, Channel, ProductID, SupplierID)
                VALUES (N'addition', @InitialStock, @InitialStock * @UnitCost, @Channel, @NewProductID, @SupplierID);
            END

            IF @AdminID IS NOT NULL
            BEGIN
                INSERT INTO dbo.ProductEdits (AdminID, ProductID)
                VALUES (@AdminID, @NewProductID);
            END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE UpdateProduct
    @ProductID     INT,
    @AdminID       INT,
    @Name          NVARCHAR(150) = NULL,
    @Description   NVARCHAR(500) = NULL,
    @UnitPrice     DECIMAL(10,2) = NULL,
    @ReorderLevel  INT           = NULL,
    @CategoryID    INT           = NULL,
    @SupplierID    INT           = NULL,
    @IsActive      BIT           = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @Name IS NOT NULL AND LTRIM(RTRIM(@Name)) = N''
            THROW 50008, N'Product name cannot be blank.', 1;

        IF @UnitPrice IS NOT NULL AND @UnitPrice < 0
            THROW 50009, N'Unit price cannot be negative.', 1;

        IF @ReorderLevel IS NOT NULL AND @ReorderLevel < 0
            THROW 50015, N'Reorder level cannot be negative.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductID = @ProductID)
            THROW 50012, N'The product does not exist.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Admins WHERE AdminID = @AdminID AND IsActive = 1)
            THROW 50013, N'The admin does not exist or is inactive.', 1;

        IF @CategoryID IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryID = @CategoryID)
            THROW 50010, N'The category does not exist.', 1;

        IF @SupplierID IS NOT NULL
           AND NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE SupplierID = @SupplierID AND IsActive = 1)
            THROW 50011, N'The supplier does not exist or is inactive.', 1;

        BEGIN TRANSACTION;

            UPDATE dbo.Products
            SET    Name         = ISNULL(@Name,         Name),
                   Description  = ISNULL(@Description,  Description),
                   UnitPrice    = ISNULL(@UnitPrice,    UnitPrice),
                   ReorderLevel = ISNULL(@ReorderLevel, ReorderLevel),
                   CategoryID   = ISNULL(@CategoryID,   CategoryID),
                   SupplierID   = ISNULL(@SupplierID,   SupplierID),
                   IsActive     = ISNULL(@IsActive,     IsActive),
                   UpdatedAt    = SYSDATETIME()
            WHERE  ProductID = @ProductID;

            IF NOT EXISTS (SELECT 1 FROM dbo.ProductEdits WHERE AdminID = @AdminID AND ProductID = @ProductID)
                INSERT INTO dbo.ProductEdits (AdminID, ProductID) VALUES (@AdminID, @ProductID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE RestockProduct
    @ProductID        INT,
    @Quantity         INT,
    @UnitCost         DECIMAL(10,2) = NULL,
    @SupplierID       INT           = NULL,
    @TransactionDate  DATETIME2(0)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Channel INT;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductID = @ProductID)
            THROW 50012, N'The product does not exist.', 1;

        IF @Quantity IS NULL OR @Quantity <= 0
            THROW 50014, N'The restock quantity must be greater than 0.', 1;

        IF @SupplierID IS NULL
            SELECT @SupplierID = SupplierID FROM dbo.Products WHERE ProductID = @ProductID;

        IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers WHERE SupplierID = @SupplierID AND IsActive = 1)
            THROW 50011, N'The supplier does not exist or is inactive.', 1;

        SELECT @Channel = TypeNumber FROM dbo.TransactionChannelTypes WHERE Type = 'Supplier Restock';

        BEGIN TRANSACTION;

            INSERT INTO dbo.StockTransactions
                   (TransactionDate, TypeOfChange, QuantityChange, Amount, Channel, ProductID, SupplierID)
            VALUES (ISNULL(@TransactionDate, SYSDATETIME()), 'addition', @Quantity,
                    @Quantity * @UnitCost, @Channel, @ProductID, @SupplierID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE GetOrderDetails
    @OrderID  INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @msg NVARCHAR(2048);

    IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE OrderID = @OrderID)
    BEGIN
        SET @msg = CONCAT(N'Order ', @OrderID, N' does not exist.');
        THROW 50005, @msg, 1;
    END

    SELECT  h.OrderID, h.OrderDate, h.Status, h.IsCancelled, h.DeliveredAt,
            h.CustomerID, h.CustomerName, h.ShippingAddress,
            h.TotalUnits, h.OrderTotal
    FROM    dbo.CustomerOrderHistory h
    WHERE   h.OrderID = @OrderID;

    SELECT  oi.ProductID,
            p.Name AS ProductName,
            oi.Quantity,
            oi.UnitPrice,
            oi.Quantity * oi.UnitPrice  AS Total
    FROM    dbo.OrderItems oi
    JOIN    dbo.Products   p ON p.ProductID = oi.ProductID
    WHERE   oi.OrderID = @OrderID
    ORDER BY oi.ProductID;
END
GO

CREATE OR ALTER PROCEDURE GenerateInventoryReport
    @CategoryID    INT = NULL,
    @LowStockOnly  BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @CategoryID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryID = @CategoryID)
        THROW 50010, N'The category does not exist.', 1;

    SELECT  v.ProductID, v.ProductName, v.CategoryName, v.SupplierName,
            v.AvailableStock, v.ReorderLevel, v.StockStatus,
            v.UnitPrice, v.StockValue, v.IsActive
    FROM    dbo.CurrentProductInventory v
    WHERE   (@CategoryID IS NULL OR v.CategoryID = @CategoryID)
      AND   (@LowStockOnly = 0 OR v.StockStatus IN ('Low stock', 'Out of stock'))
    ORDER BY CASE v.StockStatus WHEN 'Out of stock' THEN 1
                                WHEN 'Low stock'    THEN 2
                                ELSE 3 END,
             v.ProductName;

    SELECT  v.CategoryName,
            COUNT(*)                                                         AS Products,
            SUM(v.AvailableStock)                                            AS UnitsInStock,
            SUM(v.StockValue)                                                AS StockValue,
            SUM(CASE WHEN v.StockStatus = 'Low stock'    THEN 1 ELSE 0 END)  AS LowStockProducts,
            SUM(CASE WHEN v.StockStatus = 'Out of stock' THEN 1 ELSE 0 END)  AS OutOfStockProducts
    FROM    dbo.CurrentProductInventory v
    WHERE   (@CategoryID IS NULL OR v.CategoryID = @CategoryID)
    GROUP BY v.CategoryName
    ORDER BY v.CategoryName;
END
GO


CREATE OR ALTER PROCEDURE CreateOrder
    @CustomerID       INT,
    @ShippingAddress  NVARCHAR(250),
    @Items            dbo.OrderItemList READONLY,
    @OrderDate        DATETIME2(0) = NULL,    
    @NewOrderID       INT          = NULL OUTPUT
AS
BEGIN
DECLARE     @msg          NVARCHAR(2048),
            @BadProductID INT,
            @OrderTotal   DECIMAL(12,2),
            @Channel      INT,
            @When         DATETIME2(0) = ISNULL(@OrderDate, SYSDATETIME());

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerID = @CustomerID)
        BEGIN
            SET @msg = CONCAT(N'Customer ', @CustomerID, N' does not exist. The order was not created.');
            THROW 50001, @msg, 1;
        END

        IF NOT EXISTS (SELECT 1 FROM @Items)
            THROW 50002, N'An order must contain at least one product.', 1;

        SELECT TOP (1) @BadProductID = i.ProductID
        FROM   @Items i
        LEFT JOIN dbo.Products p ON p.ProductID = i.ProductID AND p.IsActive = 1
        WHERE  p.ProductID IS NULL
        ORDER BY i.ProductID;

        IF @BadProductID IS NOT NULL
        BEGIN
            SET @msg = CONCAT(N'Product ', @BadProductID, N' does not exist or is not active. The order was not created.');
            THROW 50003, @msg, 1;
        END

        SELECT @Channel = TypeNumber FROM dbo.TransactionChannelTypes WHERE Type = 'Customer Order';

        BEGIN TRANSACTION;

            SELECT TOP (1)
                   @msg = CONCAT(N'Insufficient stock for product ', i.ProductID, N' (', p.Name, N'): requested ',
                                 i.Quantity, N', available ', ISNULL(s.Stock, 0), N'. The order was not created.')
            FROM   @Items i
            JOIN   dbo.Products p ON p.ProductID = i.ProductID
            LEFT JOIN
            (
                SELECT st.ProductID, SUM(st.QuantityChange) AS Stock
                FROM   dbo.StockTransactions st WITH (UPDLOCK, HOLDLOCK)
                WHERE  st.ProductID IN (SELECT ProductID FROM @Items)
                GROUP BY st.ProductID
            ) s ON s.ProductID = i.ProductID
            WHERE  ISNULL(s.Stock, 0) < i.Quantity
            ORDER BY i.ProductID;

            IF @@ROWCOUNT > 0
                THROW 50004, @msg, 1;

            INSERT INTO dbo.Orders (OrderDate, Status, ShippingAddress, IsCancelled, CustomerID)
            VALUES (@When, 'pending', @ShippingAddress, 0, @CustomerID);

            SET @NewOrderID = SCOPE_IDENTITY();

            INSERT INTO dbo.OrderItems (OrderID, ProductID, Quantity, UnitPrice)
            SELECT @NewOrderID, i.ProductID, i.Quantity, p.UnitPrice
            FROM   @Items i
            JOIN   dbo.Products p ON p.ProductID = i.ProductID;

            INSERT INTO dbo.StockTransactions
                   (TransactionDate, TypeOfChange, QuantityChange, Amount, Channel, ProductID, OrderID)
            SELECT @When, 'subtraction', -i.Quantity, i.Quantity * p.UnitPrice, @Channel, i.ProductID, @NewOrderID
            FROM   @Items i
            JOIN   dbo.Products p ON p.ProductID = i.ProductID;

            SET @OrderTotal = dbo.GetOrderTotal(@NewOrderID);

            UPDATE dbo.Customers
            SET    TotalPurchases = TotalPurchases + @OrderTotal
            WHERE  CustomerID = @CustomerID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @NewOrderID = NULL;
        THROW;
    END CATCH
END
GO


CREATE OR ALTER PROCEDURE AddOrderItems
    @OrderID  INT,
    @Items    dbo.OrderItemList READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @msg          NVARCHAR(2048),
            @BadProductID INT,
            @Status       VARCHAR(20),
            @IsCancelled  BIT,
            @CustomerID   INT,
            @TotalBefore  DECIMAL(12,2),
            @TotalAfter   DECIMAL(12,2),
            @Channel      INT;

    BEGIN TRY
        SELECT @Status = Status, @IsCancelled = IsCancelled, @CustomerID = CustomerID
        FROM   dbo.Orders
        WHERE  OrderID = @OrderID;

        IF @CustomerID IS NULL
        BEGIN
            SET @msg = CONCAT(N'Order ', @OrderID, N' does not exist.');
            THROW 50005, @msg, 1;
        END

        IF @IsCancelled = 1 OR @Status <> 'pending'
        BEGIN
            SET @msg = CONCAT(N'Order ', @OrderID, N' is ', @Status, N'. Items can only be added to pending orders.');
            THROW 50006, @msg, 1;
        END

        IF NOT EXISTS (SELECT 1 FROM @Items)
            THROW 50002, N'No products were given to add.', 1;

        SELECT TOP (1) @BadProductID = i.ProductID
        FROM   @Items i
        LEFT JOIN dbo.Products p ON p.ProductID = i.ProductID AND p.IsActive = 1
        WHERE  p.ProductID IS NULL
        ORDER BY i.ProductID;

        IF @BadProductID IS NOT NULL
        BEGIN
            SET @msg = CONCAT(N'Product ', @BadProductID, N' does not exist or is not active. No items were added.');
            THROW 50003, @msg, 1;
        END

        SELECT @Channel = TypeNumber FROM dbo.TransactionChannelTypes WHERE Type = 'Customer Order';

        BEGIN TRANSACTION;

            SELECT TOP (1)
                   @msg = CONCAT(N'Insufficient stock for product ', i.ProductID, N' (', p.Name, N'): requested ',
                                 i.Quantity, N', available ', ISNULL(s.Stock, 0), N'. No items were added.')
            FROM   @Items i
            JOIN   Products p ON p.ProductID = i.ProductID
            LEFT JOIN
            (
                SELECT st.ProductID, SUM(st.QuantityChange) AS Stock
                FROM   StockTransactions st WITH (UPDLOCK, HOLDLOCK)
                WHERE  st.ProductID IN (SELECT ProductID FROM @Items)
                GROUP BY st.ProductID
            ) s ON s.ProductID = i.ProductID
            WHERE  ISNULL(s.Stock, 0) < i.Quantity
            ORDER BY i.ProductID;

            IF @@ROWCOUNT > 0
                THROW 50004, @msg, 1;

            SET @TotalBefore = dbo.fn_GetOrderTotal(@OrderID);

            -- products already on the order: increase the quantity
            UPDATE oi
            SET    oi.Quantity = oi.Quantity + i.Quantity
            FROM   dbo.OrderItems oi
            JOIN   @Items i ON i.ProductID = oi.ProductID
            WHERE  oi.OrderID = @OrderID;

            -- new products: add a line at today's price
            INSERT INTO dbo.OrderItems (OrderID, ProductID, Quantity, UnitPrice)
            SELECT @OrderID, i.ProductID, i.Quantity, p.UnitPrice
            FROM   @Items i
            JOIN   Products p ON p.ProductID = i.ProductID
            WHERE  NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi
                               WHERE oi.OrderID = @OrderID AND oi.ProductID = i.ProductID);

            -- one stock movement per added product, valued at the order line's price
            INSERT INTO dbo.StockTransactions (TypeOfChange, QuantityChange, Amount, Channel, ProductID, OrderID)
            SELECT 'subtraction', -i.Quantity, i.Quantity * oi.UnitPrice, @Channel, i.ProductID, @OrderID
            FROM   @Items i
            JOIN   OrderItems oi ON oi.OrderID = @OrderID AND oi.ProductID = i.ProductID;

            SET @TotalAfter = dbo.GetOrderTotal(@OrderID);

            UPDATE dbo.Customers
            SET    TotalPurchases = TotalPurchases + (@TotalAfter - @TotalBefore)
            WHERE  CustomerID = @CustomerID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_CancelOrder
    @OrderID  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @msg         NVARCHAR(2048),
            @Status      VARCHAR(20),
            @CustomerID  INT,
            @OrderTotal  DECIMAL(12,2),
            @Channel     INT;

    BEGIN TRY
        SELECT @Status = Status, @CustomerID = CustomerID
        FROM   dbo.Orders
        WHERE  OrderID = @OrderID;

        IF @CustomerID IS NULL
        BEGIN
            SET @msg = CONCAT(N'Order ', @OrderID, N' does not exist.');
            THROW 50005, @msg, 1;
        END

        IF @Status = 'delivered'
        BEGIN
            SET @msg = CONCAT(N'Order ', @OrderID, N' has been delivered and cannot be cancelled.');
            THROW 50008, @msg, 1;
        END

        SELECT @Channel = TypeNumber FROM dbo.TransactionChannelTypes WHERE Type = 'Order Cancellation';

        BEGIN TRANSACTION;

            -- the WHERE IsCancelled = 0 also protects against two cancellations at once
            UPDATE dbo.Orders
            SET    Status = 'cancelled',
                   IsCancelled = 1
            WHERE  OrderID = @OrderID
              AND  IsCancelled = 0;

            IF @@ROWCOUNT = 0
            BEGIN
                SET @msg = CONCAT(N'Order ', @OrderID, N' is already cancelled.');
                THROW 50007, @msg, 1;
            END

            -- give back exactly what this order took out, product by product
            INSERT INTO dbo.StockTransactions (TypeOfChange, QuantityChange, Amount, Channel, ProductID, OrderID)
            SELECT 'addition', -SUM(st.QuantityChange), SUM(st.Amount), @Channel, st.ProductID, @OrderID
            FROM   dbo.StockTransactions st
            WHERE  st.OrderID = @OrderID
            GROUP BY st.ProductID
            HAVING SUM(st.QuantityChange) < 0;

            SET @OrderTotal = dbo.GetOrderTotal(@OrderID);

            UPDATE dbo.Customers
            SET    TotalPurchases = CASE WHEN TotalPurchases >= @OrderTotal
                                         THEN TotalPurchases - @OrderTotal ELSE 0 END
            WHERE  CustomerID = @CustomerID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
