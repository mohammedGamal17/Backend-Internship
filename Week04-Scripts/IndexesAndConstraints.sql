USE InventoryDB;
GO


ALTER TABLE  Categories        ADD CONSTRAINT DF_Categories_CreatedAt              DEFAULT (SYSDATETIME())  FOR CreatedAt;
ALTER TABLE  Users             ADD CONSTRAINT DF_Users_JoinedAt                    DEFAULT (SYSDATETIME())  FOR JoinedAt;
ALTER TABLE  Admins            ADD CONSTRAINT DF_Admins_IsActive                   DEFAULT (1)              FOR IsActive;
ALTER TABLE  Suppliers         ADD CONSTRAINT DF_Suppliers_IsActive                DEFAULT (1)              FOR IsActive;
ALTER TABLE  Customers         ADD CONSTRAINT DF_Customers_TotalPurchases          DEFAULT (0)              FOR TotalPurchases;
ALTER TABLE  Products          ADD CONSTRAINT DF_Products_ReorderLevel             DEFAULT (10)             FOR ReorderLevel;
ALTER TABLE  Products          ADD CONSTRAINT DF_Products_IsActive                 DEFAULT (1)              FOR IsActive;
ALTER TABLE  Products          ADD CONSTRAINT DF_Products_CreatedAt                DEFAULT (SYSDATETIME())  FOR CreatedAt;
ALTER TABLE  Orders            ADD CONSTRAINT DF_Orders_OrderDate                  DEFAULT (SYSDATETIME())  FOR OrderDate;
ALTER TABLE  Orders            ADD CONSTRAINT DF_Orders_Status                     DEFAULT ('pending')      FOR Status;
ALTER TABLE  Orders            ADD CONSTRAINT DF_Orders_IsCancelled                DEFAULT (0)              FOR IsCancelled;
ALTER TABLE  StockTransactions ADD CONSTRAINT DF_StockTransactions_TransactionDate DEFAULT (SYSDATETIME())  FOR TransactionDate;
ALTER TABLE  StockTransactions ADD CONSTRAINT DF_StockTransactions_CreatedBy       DEFAULT (SUSER_SNAME())  FOR CreatedBy;
GO


ALTER TABLE  Categories              ADD CONSTRAINT UQ_Categories_CategoryName      UNIQUE (CategoryName);
ALTER TABLE  UserTypes               ADD CONSTRAINT UQ_UserTypes_Type               UNIQUE (Type);
ALTER TABLE  TransactionChannelTypes ADD CONSTRAINT UQ_TransactionChannelTypes_Type UNIQUE (Type);
GO


/* Categories  */
ALTER TABLE  Categories ADD CONSTRAINT CK_Categories_CategoryName_NotBlank
    CHECK (LEN(LTRIM(RTRIM(CategoryName))) > 0);

/*  Users  */
ALTER TABLE  Users ADD CONSTRAINT CK_Users_Names_NotBlank
    CHECK (LEN(LTRIM(RTRIM(FirstName))) > 0 AND LEN(LTRIM(RTRIM(LastName))) > 0);

/*  UserContacts  */
ALTER TABLE  UserContacts ADD CONSTRAINT CK_UserContacts_Email_Format
    CHECK (Email LIKE '%_@_%._%');

ALTER TABLE  UserContacts ADD CONSTRAINT CK_UserContacts_PhoneNumber_Format
    CHECK (LEN(PhoneNumber) >= 6 AND PhoneNumber NOT LIKE '%[^0-9+() -]%');   -- digits, +, (, ), space, dash only

ALTER TABLE  UserContacts ADD CONSTRAINT CK_UserContacts_Address_NotBlank
    CHECK (LEN(LTRIM(RTRIM(Address))) > 0);

/*  Customers  */
ALTER TABLE  Customers ADD CONSTRAINT CK_Customers_TotalPurchases_NotNegative
    CHECK (TotalPurchases >= 0);

/*  Products  */
ALTER TABLE  Products ADD CONSTRAINT CK_Products_Name_NotBlank
    CHECK (LEN(LTRIM(RTRIM(Name))) > 0);

ALTER TABLE  Products ADD CONSTRAINT CK_Products_UnitPrice_Positive
    CHECK (UnitPrice > 0);

ALTER TABLE  Products ADD CONSTRAINT CK_Products_ReorderLevel_NotNegative
    CHECK (ReorderLevel >= 0);

ALTER TABLE  Products ADD CONSTRAINT CK_Products_UpdatedAt_AfterCreatedAt
    CHECK (UpdatedAt IS NULL OR UpdatedAt >= CreatedAt);

/*  Orders  */
ALTER TABLE  Orders ADD CONSTRAINT CK_Orders_IsCancelled_MatchesStatus
    CHECK (   (IsCancelled = 1 AND Status = 'cancelled')
           OR (IsCancelled = 0 AND Status <> 'cancelled'));

ALTER TABLE  Orders ADD CONSTRAINT CK_Orders_DeliveredAt_MatchesStatus
    CHECK (   (Status = 'delivered'  AND DeliveredAt IS NOT NULL AND DeliveredAt >= OrderDate)
           OR (Status <> 'delivered' AND DeliveredAt IS NULL));

/*  OrderItems  */
ALTER TABLE  OrderItems ADD CONSTRAINT CK_OrderItems_Quantity_Positive
    CHECK (Quantity > 0);

ALTER TABLE  OrderItems ADD CONSTRAINT CK_OrderItems_UnitPrice_NotNegative
    CHECK (UnitPrice >= 0);

/*  StockTransactions  */
ALTER TABLE  StockTransactions ADD CONSTRAINT CK_StockTransactions_Quantity_Sign
    CHECK (   (TypeOfChange = 'addition'    AND QuantityChange > 0)
           OR (TypeOfChange = 'subtraction' AND QuantityChange < 0));

ALTER TABLE  StockTransactions ADD CONSTRAINT CK_StockTransactions_Amount_NotNegative
    CHECK (Amount IS NULL OR Amount >= 0);


ALTER TABLE  StockTransactions ADD CONSTRAINT CK_StockTransactions_Supplier_Or_Order
    CHECK (SupplierID IS NULL OR OrderID IS NULL);

ALTER TABLE  StockTransactions ADD CONSTRAINT CK_StockTransactions_Supplier_OnlyAddition
    CHECK (SupplierID IS NULL OR TypeOfChange = 'addition');
GO

--Foreign keys--

ALTER TABLE  Users ADD CONSTRAINT FK_Users_UserTypes
    FOREIGN KEY (UserType) REFERENCES  UserTypes (TypeNumber);

ALTER TABLE  StockTransactions ADD CONSTRAINT FK_StockTransactions_TransactionChannelTypes
    FOREIGN KEY (Channel) REFERENCES  TransactionChannelTypes (TypeNumber);

ALTER TABLE  UserContacts ADD CONSTRAINT FK_UserContacts_Users
    FOREIGN KEY (UserID) REFERENCES  Users (UserID) ON DELETE CASCADE;

ALTER TABLE  Admins ADD CONSTRAINT FK_Admins_Users
    FOREIGN KEY (AdminID) REFERENCES  Users (UserID);

ALTER TABLE  Customers ADD CONSTRAINT FK_Customers_Users
    FOREIGN KEY (CustomerID) REFERENCES  Users (UserID);

ALTER TABLE  Suppliers ADD CONSTRAINT FK_Suppliers_Users
    FOREIGN KEY (SupplierID) REFERENCES  Users (UserID);

ALTER TABLE  Products ADD CONSTRAINT FK_Products_Categories
    FOREIGN KEY (CategoryID) REFERENCES  Categories (CategoryID);

ALTER TABLE  Products ADD CONSTRAINT FK_Products_Suppliers
    FOREIGN KEY (SupplierID) REFERENCES  Suppliers (SupplierID);

ALTER TABLE  ProductEdits ADD CONSTRAINT FK_ProductEdits_Admins
    FOREIGN KEY (AdminID) REFERENCES  Admins (AdminID);

ALTER TABLE  ProductEdits ADD CONSTRAINT FK_ProductEdits_Products
    FOREIGN KEY (ProductID) REFERENCES  Products (ProductID);

ALTER TABLE  Orders ADD CONSTRAINT FK_Orders_Customers
    FOREIGN KEY (CustomerID) REFERENCES  Customers (CustomerID);

ALTER TABLE  OrderItems ADD CONSTRAINT FK_OrderItems_Orders
    FOREIGN KEY (OrderID) REFERENCES  Orders (OrderID);

ALTER TABLE  OrderItems ADD CONSTRAINT FK_OrderItems_Products
    FOREIGN KEY (ProductID) REFERENCES  Products (ProductID);

ALTER TABLE  StockTransactions ADD CONSTRAINT FK_StockTransactions_Products
    FOREIGN KEY (ProductID) REFERENCES  Products (ProductID);

ALTER TABLE  StockTransactions ADD CONSTRAINT FK_StockTransactions_Suppliers
    FOREIGN KEY (SupplierID) REFERENCES  Suppliers (SupplierID);

ALTER TABLE  StockTransactions ADD CONSTRAINT FK_StockTransactions_Orders
    FOREIGN KEY (OrderID) REFERENCES  Orders (OrderID);
GO


/* Index*/

/* Users / UserContacts */
CREATE NONCLUSTERED INDEX IX_Users_UserType                 -- users of one type (all customers, all suppliers)
    ON  Users (UserType);

CREATE NONCLUSTERED INDEX IX_Users_LastName_FirstName       -- search by name
    ON  Users (LastName, FirstName);

CREATE NONCLUSTERED INDEX IX_UserContacts_UserID            -- a user's contact details
    ON  UserContacts (UserID);

/* ---------- Products ---------- */
CREATE NONCLUSTERED INDEX IX_Products_CategoryID            -- query 1, revenue per category (query 8)
    ON  Products (CategoryID);

CREATE NONCLUSTERED INDEX IX_Products_SupplierID            -- query 1
    ON  Products (SupplierID);

CREATE NONCLUSTERED INDEX IX_Products_Name                  -- product search by name
    ON  Products (Name);

/* ---------- ProductEdits ---------- */
CREATE NONCLUSTERED INDEX IX_ProductEdits_ProductID         -- who edited a given product
    ON  ProductEdits (ProductID);

/* ---------- Orders ---------- */
CREATE NONCLUSTERED INDEX IX_Orders_CustomerID              -- order history, customer totals, queries 4 and 6
    ON  Orders (CustomerID)
    INCLUDE (OrderDate, Status, IsCancelled);


/* ---------- OrderItems ---------- */
CREATE NONCLUSTERED INDEX IX_OrderItems_ProductID           -- best sellers (2), never sold (5), revenue (8)
    ON  OrderItems (ProductID)
    INCLUDE (Quantity, UnitPrice);

/* ---------- StockTransactions ---------- */
CREATE NONCLUSTERED INDEX IX_StockTransactions_ProductID    -- available stock = SUM(QuantityChange), low stock (7)
    ON  StockTransactions (ProductID)
    INCLUDE (QuantityChange, TypeOfChange, TransactionDate);



CREATE NONCLUSTERED INDEX IX_StockTransactions_OrderID      -- an order's movements (used to cancel it); filtered
    ON  StockTransactions (OrderID)
    INCLUDE (ProductID, QuantityChange)
    WHERE OrderID IS NOT NULL;


GO