USE master;
GO
IF DB_ID(N'InventoryDB') IS NOT NULL
BEGIN
    ALTER DATABASE InventoryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE InventoryDB;
END
GO
CREATE DATABASE InventoryDB;
GO
USE InventoryDB;
GO

/* Create Tables */

CREATE TABLE  Categories
(
    CategoryID      INT IDENTITY(1,1) NOT NULL,
    CategoryName    NVARCHAR(150)  NOT NULL,
    Description     NVARCHAR(500)  NULL,
    CreatedAt       DATETIME2(0)   NOT NULL,

    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (CategoryID)
);
GO

CREATE TABLE  Users
(
    UserID      INT IDENTITY(1,1) NOT NULL,
    FirstName   NVARCHAR(50)   NOT NULL,
    LastName    NVARCHAR(50)   NOT NULL,
    UserType    INT            NOT NULL,
    JoinedAt    DATETIME2(0)   NOT NULL,

    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserID)
);
GO

CREATE TABLE  UserTypes
(
    Type        VARCHAR(20)   NOT NULL,
    TypeNumber  INT IDENTITY(1,1) NOT NULL,           

    CONSTRAINT PK_UserTypes PRIMARY KEY CLUSTERED (TypeNumber)
);
GO


CREATE TABLE UserContacts
(
    UserID       INT           NOT NULL,          -- FK -> Users
    PhoneNumber  VARCHAR(20)   NOT NULL,
    Email       VARCHAR(255)   NOT NULL,
    Address     VARCHAR(300)   NOT NULL,

    CONSTRAINT PK_UserContacts PRIMARY KEY CLUSTERED (Email)
);
GO


CREATE TABLE Admins
(
    AdminID   INT  NOT NULL,                      -- PK and FK -> Users.UserID
    IsActive  BIT  NOT NULL,

    CONSTRAINT PK_Admins PRIMARY KEY CLUSTERED (AdminID)
);
GO

CREATE TABLE  Suppliers
(
    SupplierID  INT  NOT NULL,                    -- PK and FK -> Users.UserID
    IsActive    BIT  NOT NULL,

    CONSTRAINT PK_Suppliers PRIMARY KEY CLUSTERED (SupplierID)
);
GO

CREATE TABLE  Customers
(
    CustomerID  INT  NOT NULL,    -- PK and FK -> Users.UserID
    TotalPurchases Decimal(10,2),

    CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (CustomerID)
);
GO

CREATE TABLE  Products
(
    ProductID     INT            IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(150)  NOT NULL,
    Description   NVARCHAR(500)  NULL,
    UnitPrice     DECIMAL(10,2)  NOT NULL,
    ReorderLevel  INT            NOT NULL,        
    IsActive      BIT            NOT NULL,
    CreatedAt     DATETIME2(0)   NOT NULL,
    UpdatedAt     DATETIME2(0)   NULL,
    CategoryID    INT            NOT NULL,        -- FK -> Categories (Classifies, total)
    SupplierID    INT            NOT NULL,        -- FK -> Suppliers  (Supplies, total)

    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductID)
);
GO

CREATE TABLE  ProductEdits
(
    AdminID    INT  NOT NULL,                     -- FK -> Admins
    ProductID  INT  NOT NULL,                     -- FK -> Products

    CONSTRAINT PK_ProductEdits PRIMARY KEY CLUSTERED (AdminID, ProductID)
);
GO

CREATE TABLE  Orders
(
    OrderID          INT            IDENTITY(1,1) NOT NULL,
    OrderDate        DATETIME2(0)   NOT NULL,
    Status           VARCHAR(20)    NOT NULL       
        CONSTRAINT CK_Orders_Status CHECK (Status IN ('pending', 'shipped', 'delivered', 'cancelled')),    
    ShippingAddress  NVARCHAR(250)  NOT NULL,
    DeliveredAt      DATETIME2(0)   NULL,
    IsCancelled      BIT            NOT NULL,
    CustomerID       INT            NOT NULL,     -- FK -> Customers (Orders relationship, total)

    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (OrderID)
);
GO

CREATE TABLE  OrderItems
(
    OrderID    INT            NOT NULL,           -- FK -> Orders
    ProductID  INT            NOT NULL,           -- FK -> Products
    Quantity   INT            NOT NULL,
    UnitPrice  DECIMAL(10,2)  NOT NULL,

    CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (OrderID, ProductID)
);
GO

CREATE TABLE  StockTransactions
(
    TransactionID    INT            IDENTITY(1,1) NOT NULL,
    TransactionDate  DATETIME2(0)   NOT NULL,
    TypeOfChange     VARCHAR(20)    NOT NULL  
        CONSTRAINT CK_StockTransactions_TypeOfChange CHECK (TypeOfChange IN ('addition', 'subtraction')), 
    QuantityChange   INT            NOT NULL,
    Amount           DECIMAL(12,2)  NULL,         -- money value of the movement
    Channel          INT    NULL,         -- FK-->where the movement came from
    CreatedBy        NVARCHAR(128)  NOT NULL,
    ProductID        INT            NOT NULL,     -- FK -> Products (Moves, total)
    SupplierID       INT            , --FK--> SUPPLIER 
    OrderID          INT            ,

    CONSTRAINT PK_StockTransactions PRIMARY KEY CLUSTERED (TransactionID)
);
GO

CREATE TABLE  TransactionChannelTypes
(
    Type        VARCHAR(20)   NOT NULL,
    TypeNumber  INT IDENTITY(1,1) NOT NULL,           

    CONSTRAINT PK_TransactionChannelTypes PRIMARY KEY CLUSTERED (TypeNumber)
);
GO

