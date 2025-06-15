-- ============================================================================
-- Script tạo Schema (Cấu trúc Bảng) cho CSDL BookstoreDb
-- Yêu cầu: Chạy script này trên một database trống tên là "BookstoreDb".
-- ============================================================================

USE [BookstoreDb]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Tạo Bảng __EFMigrationsHistory
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory](
        [MigrationId] [nvarchar](150) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
    );
END
GO

-- Tạo Bảng Addresses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Addresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Addresses](
        [Id] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NOT NULL,
        [Street] [nvarchar](256) NOT NULL,
        [Village] [nvarchar](100) NOT NULL,
        [District] [nvarchar](100) NOT NULL,
        [City] [nvarchar](100) NOT NULL,
        [IsDefault] [bit] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Authors
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Authors]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Authors](
        [Id] [uniqueidentifier] NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Biography] [nvarchar](max) NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Authors] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Books
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Books]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Books](
        [Id] [uniqueidentifier] NOT NULL,
        [Title] [nvarchar](256) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [ISBN] [nvarchar](20) NULL,
        [AuthorId] [uniqueidentifier] NULL,
        [Publisher] [nvarchar](100) NULL,
        [PublicationYear] [int] NULL,
        [CoverImageUrl] [nvarchar](max) NULL,
        [Price] [decimal](18, 2) NOT NULL,
        [StockQuantity] [int] NOT NULL,
        [CategoryId] [uniqueidentifier] NOT NULL,
        [IsDeleted] [bit] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
        [RowVersion] [rowversion] NOT NULL,
    CONSTRAINT [PK_Books] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng CartItems
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CartItems](
        [UserId] [uniqueidentifier] NOT NULL,
        [BookId] [uniqueidentifier] NOT NULL,
        [Quantity] [int] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED ([UserId] ASC, [BookId] ASC)
    );
END
GO

-- Tạo Bảng Categories
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Categories](
        [Id] [uniqueidentifier] NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [ParentCategoryId] [uniqueidentifier] NULL,
        [IsDeleted] [bit] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng InventoryLogs
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InventoryLogs](
        [Id] [uniqueidentifier] NOT NULL,
        [BookId] [uniqueidentifier] NOT NULL,
        [ChangeQuantity] [int] NOT NULL,
        [Reason] [tinyint] NOT NULL,
        [TimestampUtc] [datetime2](7) NOT NULL,
        [OrderId] [uniqueidentifier] NULL,
        [UserId] [uniqueidentifier] NULL,
        [StockReceiptId] [uniqueidentifier] NULL,
    CONSTRAINT [PK_InventoryLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng OrderDetails
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderDetails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderDetails](
        [Id] [uniqueidentifier] NOT NULL,
        [OrderId] [uniqueidentifier] NOT NULL,
        [BookId] [uniqueidentifier] NOT NULL,
        [Quantity] [int] NOT NULL,
        [UnitPrice] [decimal](18, 2) NOT NULL,
    CONSTRAINT [PK_OrderDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng OrderShippingAddresses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderShippingAddresses]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderShippingAddresses](
        [Id] [uniqueidentifier] NOT NULL,
        [Street] [nvarchar](256) NOT NULL,
        [Village] [nvarchar](100) NULL,
        [District] [nvarchar](100) NOT NULL,
        [City] [nvarchar](100) NOT NULL,
        [RecipientName] [nvarchar](200) NULL,
        [PhoneNumber] [nvarchar](20) NULL,
    CONSTRAINT [PK_OrderShippingAddresses] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Orders
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Orders](
        [Id] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NULL,
        [OrderDate] [datetime2](7) NOT NULL,
        [Status] [tinyint] NOT NULL,
        [TotalAmount] [decimal](18, 2) NOT NULL,
        [OrderShippingAddressId] [uniqueidentifier] NULL,
        [TransactionId] [nvarchar](256) NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
        [RowVersion] [rowversion] NOT NULL,
        [DeliveryMethod] [tinyint] NOT NULL,
        [InvoiceNumber] [nvarchar](50) NULL,
        [OrderType] [tinyint] NOT NULL,
        [PaymentMethod] [tinyint] NULL,
        [PaymentStatus] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Promotions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Promotions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Promotions](
        [Id] [uniqueidentifier] NOT NULL,
        [Code] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](256) NULL,
        [DiscountPercentage] [decimal](5, 2) NULL,
        [DiscountAmount] [decimal](18, 2) NULL,
        [StartDate] [datetime2](7) NOT NULL,
        [EndDate] [datetime2](7) NULL,
        [MaxUsage] [int] NULL,
        [CurrentUsage] [int] NOT NULL,
        [IsActive] [bit] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Promotions] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Reviews (Đã đổi tên từ "Review")
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reviews]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Reviews](
        [Id] [uniqueidentifier] NOT NULL,
        [BookId] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NOT NULL,
        [Rating] [int] NOT NULL,
        [Comment] [nvarchar](1000) NULL,
        [IsApproved] [bit] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Roles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles](
        [Id] [uniqueidentifier] NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng StockReceipts
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockReceipts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StockReceipts](
        [Id] [uniqueidentifier] NOT NULL,
        [SupplierId] [uniqueidentifier] NULL,
        [ReceiptDate] [datetime2](7) NOT NULL,
        [Notes] [nvarchar](max) NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_StockReceipts] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng StockReceiptDetails
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockReceiptDetails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StockReceiptDetails](
        [Id] [uniqueidentifier] NOT NULL,
        [StockReceiptId] [uniqueidentifier] NOT NULL,
        [BookId] [uniqueidentifier] NOT NULL,
        [QuantityReceived] [int] NOT NULL,
        [PurchasePrice] [decimal](18, 2) NULL,
    CONSTRAINT [PK_StockReceiptDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng Suppliers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Suppliers](
        [Id] [uniqueidentifier] NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [ContactPerson] [nvarchar](100) NULL,
        [Email] [nvarchar](256) NULL,
        [Phone] [nvarchar](50) NULL,
        [Address] [nvarchar](500) NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng UserRoles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserRoles](
        [UserId] [uniqueidentifier] NOT NULL,
        [RoleId] [uniqueidentifier] NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC)
    );
END
GO

-- Tạo Bảng Users
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id] [uniqueidentifier] NOT NULL,
        [UserName] [nvarchar](256) NOT NULL,
        [Email] [nvarchar](256) NOT NULL,
        [PasswordHash] [nvarchar](max) NOT NULL,
        [FirstName] [nvarchar](100) NULL,
        [LastName] [nvarchar](100) NULL,
        [PhoneNumber] [nvarchar](50) NULL,
        [IsActive] [bit] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
        [UpdatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Tạo Bảng WishlistItems
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WishlistItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WishlistItems](
        [Id] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NOT NULL,
        [BookId] [uniqueidentifier] NOT NULL,
        [CreatedAtUtc] [datetime2](7) NOT NULL,
    CONSTRAINT [PK_WishlistItems] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Thêm các Indexes
CREATE NONCLUSTERED INDEX [IX_Addresses_UserId] ON [dbo].[Addresses] ([UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_Books_AuthorId] ON [dbo].[Books] ([AuthorId] ASC);
CREATE NONCLUSTERED INDEX [IX_Books_CategoryId] ON [dbo].[Books] ([CategoryId] ASC);
CREATE NONCLUSTERED INDEX [IX_CartItems_BookId] ON [dbo].[CartItems] ([BookId] ASC);
CREATE NONCLUSTERED INDEX [IX_Categories_ParentCategoryId] ON [dbo].[Categories] ([ParentCategoryId] ASC);
CREATE NONCLUSTERED INDEX [IX_InventoryLogs_BookId] ON [dbo].[InventoryLogs] ([BookId] ASC);
CREATE NONCLUSTERED INDEX [IX_InventoryLogs_OrderId] ON [dbo].[InventoryLogs] ([OrderId] ASC);
CREATE NONCLUSTERED INDEX [IX_InventoryLogs_StockReceiptId] ON [dbo].[InventoryLogs] ([StockReceiptId] ASC);
CREATE NONCLUSTERED INDEX [IX_InventoryLogs_UserId] ON [dbo].[InventoryLogs] ([UserId] ASC);
CREATE NONCLUSTERED INDEX [IX_OrderDetails_BookId] ON [dbo].[OrderDetails] ([BookId] ASC);
CREATE NONCLUSTERED INDEX [IX_OrderDetails_OrderId] ON [dbo].[OrderDetails] ([OrderId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Orders_InvoiceNumber] ON [dbo].[Orders] ([InvoiceNumber] ASC) WHERE ([InvoiceNumber] IS NOT NULL);
CREATE NONCLUSTERED INDEX [IX_Orders_OrderShippingAddressId] ON [dbo].[Orders] ([OrderShippingAddressId] ASC);
CREATE NONCLUSTERED INDEX [IX_Orders_UserId] ON [dbo].[Orders] ([UserId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Promotions_Code] ON [dbo].[Promotions] ([Code] ASC);
CREATE NONCLUSTERED INDEX [IX_Reviews_BookId] ON [dbo].[Reviews] ([BookId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Reviews_UserId_BookId] ON [dbo].[Reviews] ([UserId] ASC, [BookId] ASC);
CREATE NONCLUSTERED INDEX [IX_StockReceiptDetails_BookId] ON [dbo].[StockReceiptDetails] ([BookId] ASC);
CREATE NONCLUSTERED INDEX [IX_StockReceiptDetails_StockReceiptId] ON [dbo].[StockReceiptDetails] ([StockReceiptId] ASC);
CREATE NONCLUSTERED INDEX [IX_StockReceipts_SupplierId] ON [dbo].[StockReceipts] ([SupplierId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Suppliers_Email] ON [dbo].[Suppliers] ([Email] ASC) WHERE ([Email] IS NOT NULL);
CREATE NONCLUSTERED INDEX [IX_Suppliers_Name] ON [dbo].[Suppliers] ([Name] ASC);
CREATE NONCLUSTERED INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles] ([RoleId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_UserName] ON [dbo].[Users] ([UserName] ASC);
CREATE NONCLUSTERED INDEX [IX_WishlistItems_BookId] ON [dbo].[WishlistItems] ([BookId] ASC);
CREATE UNIQUE NONCLUSTERED INDEX [IX_WishlistItems_UserId_BookId] ON [dbo].[WishlistItems] ([UserId] ASC, [BookId] ASC);
GO

-- Thêm các Default Values
ALTER TABLE [dbo].[Addresses] ADD  DEFAULT (N'') FOR [Village];
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (CONVERT([tinyint],(0))) FOR [DeliveryMethod];
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (CONVERT([tinyint],(0))) FOR [OrderType];
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'Pending') FOR [PaymentStatus];
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT ((0)) FOR [CurrentUsage];
ALTER TABLE [dbo].[Promotions] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsActive];
GO

-- Thêm các Khóa ngoại (Foreign Keys)
ALTER TABLE [dbo].[Addresses]  WITH CHECK ADD  CONSTRAINT [FK_Addresses_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[Addresses] CHECK CONSTRAINT [FK_Addresses_Users];
GO

ALTER TABLE [dbo].[Books]  WITH CHECK ADD  CONSTRAINT [FK_Books_Authors] FOREIGN KEY([AuthorId])
REFERENCES [dbo].[Authors] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[Books] CHECK CONSTRAINT [FK_Books_Authors];
GO

ALTER TABLE [dbo].[Books]  WITH CHECK ADD  CONSTRAINT [FK_Books_Categories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
ON DELETE RESTRICT;
GO
ALTER TABLE [dbo].[Books] CHECK CONSTRAINT [FK_Books_Categories];
GO

ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Books];
GO

ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Users];
GO

ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [FK_Categories_ParentCategory] FOREIGN KEY([ParentCategoryId])
REFERENCES [dbo].[Categories] ([Id]);
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [FK_Categories_ParentCategory];
GO

ALTER TABLE [dbo].[InventoryLogs]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLogs_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[InventoryLogs] CHECK CONSTRAINT [FK_InventoryLogs_Books];
GO

ALTER TABLE [dbo].[InventoryLogs]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLogs_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[InventoryLogs] CHECK CONSTRAINT [FK_InventoryLogs_Orders];
GO

ALTER TABLE [dbo].[InventoryLogs]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLogs_StockReceipts] FOREIGN KEY([StockReceiptId])
REFERENCES [dbo].[StockReceipts] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[InventoryLogs] CHECK CONSTRAINT [FK_InventoryLogs_StockReceipts];
GO

ALTER TABLE [dbo].[InventoryLogs]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLogs_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[InventoryLogs] CHECK CONSTRAINT [FK_InventoryLogs_Users];
GO

ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_OrderDetails_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([Id])
ON DELETE RESTRICT;
GO
ALTER TABLE [dbo].[OrderDetails] CHECK CONSTRAINT [FK_OrderDetails_Books];
GO

ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_OrderDetails_Orders] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[OrderDetails] CHECK CONSTRAINT [FK_OrderDetails_Orders];
GO

ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_OrderShippingAddresses] FOREIGN KEY([OrderShippingAddressId])
REFERENCES [dbo].[OrderShippingAddresses] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_OrderShippingAddresses];
GO

ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Users];
GO

ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Books];
GO

ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [FK_Reviews_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [FK_Reviews_Users];
GO

ALTER TABLE [dbo].[StockReceipts]  WITH CHECK ADD  CONSTRAINT [FK_StockReceipts_Suppliers] FOREIGN KEY([SupplierId])
REFERENCES [dbo].[Suppliers] ([Id])
ON DELETE SET NULL;
GO
ALTER TABLE [dbo].[StockReceipts] CHECK CONSTRAINT [FK_StockReceipts_Suppliers];
GO

ALTER TABLE [dbo].[StockReceiptDetails]  WITH CHECK ADD  CONSTRAINT [FK_StockReceiptDetails_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([Id])
ON DELETE RESTRICT;
GO
ALTER TABLE [dbo].[StockReceiptDetails] CHECK CONSTRAINT [FK_StockReceiptDetails_Books];
GO

ALTER TABLE [dbo].[StockReceiptDetails]  WITH CHECK ADD  CONSTRAINT [FK_StockReceiptDetails_StockReceipts] FOREIGN KEY([StockReceiptId])
REFERENCES [dbo].[StockReceipts] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[StockReceiptDetails] CHECK CONSTRAINT [FK_StockReceiptDetails_StockReceipts];
GO

ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Roles];
GO

ALTER TABLE [dbo].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[UserRoles] CHECK CONSTRAINT [FK_UserRoles_Users];
GO

ALTER TABLE [dbo].[WishlistItems]  WITH CHECK ADD  CONSTRAINT [FK_WishlistItems_Books] FOREIGN KEY([BookId])
REFERENCES [dbo].[Books] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[WishlistItems] CHECK CONSTRAINT [FK_WishlistItems_Books];
GO

ALTER TABLE [dbo].[WishlistItems]  WITH CHECK ADD  CONSTRAINT [FK_WishlistItems_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[WishlistItems] CHECK CONSTRAINT [FK_WishlistItems_Users];
GO

-- Thêm các Ràng buộc CHECK (Check Constraints)
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [CK_CartItems_Quantity] CHECK  (([Quantity]>(0)));
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [CK_CartItems_Quantity];
GO

ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_Amount] CHECK  (([DiscountAmount] IS NULL OR [DiscountAmount]>(0)));
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_Amount];
GO

ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_Dates] CHECK  (([EndDate] IS NULL OR [EndDate]>=[StartDate]));
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_Dates];
GO

ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_DiscountType] CHECK  (([DiscountPercentage] IS NOT NULL AND [DiscountAmount] IS NULL OR [DiscountPercentage] IS NULL AND [DiscountAmount] IS NOT NULL OR [DiscountPercentage] IS NULL AND [DiscountAmount] IS NULL));
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_DiscountType];
GO

ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_Percentage] CHECK  (([DiscountPercentage] IS NULL OR [DiscountPercentage]>(0) AND [DiscountPercentage]<=(100)));
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_Percentage];
GO

ALTER TABLE [dbo].[Promotions]  WITH CHECK ADD  CONSTRAINT [CK_Promotions_Usage] CHECK  (([CurrentUsage]>=(0) AND ([MaxUsage] IS NULL OR [CurrentUsage]<=[MaxUsage])));
GO
ALTER TABLE [dbo].[Promotions] CHECK CONSTRAINT [CK_Promotions_Usage];
GO

ALTER TABLE [dbo].[Reviews]  WITH CHECK ADD  CONSTRAINT [CK_Reviews_Rating] CHECK  (([Rating]>=(1) AND [Rating]<=(5)));
GO
ALTER TABLE [dbo].[Reviews] CHECK CONSTRAINT [CK_Reviews_Rating];
GO

ALTER TABLE [dbo].[StockReceiptDetails]  WITH CHECK ADD  CONSTRAINT [CK_StockReceiptDetails_QuantityReceived] CHECK  (([QuantityReceived]>(0)));
GO
ALTER TABLE [dbo].[StockReceiptDetails] CHECK CONSTRAINT [CK_StockReceiptDetails_QuantityReceived];
GO

ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD  CONSTRAINT [CK_OrderDetails_Quantity] CHECK  (([Quantity]>(0)));
GO
ALTER TABLE [dbo].[OrderDetails] CHECK CONSTRAINT [CK_OrderDetails_Quantity];
GO

ALTER TABLE [dbo].[OrderDetails]  WITH CHECK ADD  CONSTRAINT [CK_OrderDetails_UnitPrice] CHECK  (([UnitPrice]>=(0)));
GO
ALTER TABLE [dbo].[OrderDetails] CHECK CONSTRAINT [CK_OrderDetails_UnitPrice];
GO

ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [CK_Orders_TotalAmount] CHECK  (([TotalAmount]>=(0)));
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [CK_Orders_TotalAmount];
GO
ALTER TABLE [dbo].[Books]  WITH CHECK ADD  CONSTRAINT [CK_Books_Price] CHECK  (([Price]>=(0)));
GO
ALTER TABLE [dbo].[Books] CHECK CONSTRAINT [CK_Books_Price];
GO

ALTER TABLE [dbo].[Books]  WITH CHECK ADD  CONSTRAINT [CK_Books_StockQuantity] CHECK  (([StockQuantity]>=(0)));
GO
ALTER TABLE [dbo].[Books] CHECK CONSTRAINT [CK_Books_StockQuantity];
GO

PRINT 'Database schema created/updated successfully.';
GO