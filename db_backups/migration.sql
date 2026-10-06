BEGIN TRANSACTION;
ALTER TABLE [MarketplaceItems] DROP CONSTRAINT [FK_MarketplaceItems_Categories_CategoryId];

ALTER TABLE [MarketplaceItems] DROP CONSTRAINT [FK_MarketplaceItems_SubCategories_SubCategoryId];

CREATE TABLE [MarketplaceCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Icon] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_MarketplaceCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [MarketplaceSubCategories] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_MarketplaceSubCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MarketplaceSubCategories_MarketplaceCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [MarketplaceCategories] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_MarketplaceSubCategories_CategoryId] ON [MarketplaceSubCategories] ([CategoryId]);

ALTER TABLE [MarketplaceItems] ADD CONSTRAINT [FK_MarketplaceItems_MarketplaceCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [MarketplaceCategories] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [MarketplaceItems] ADD CONSTRAINT [FK_MarketplaceItems_MarketplaceSubCategories_SubCategoryId] FOREIGN KEY ([SubCategoryId]) REFERENCES [MarketplaceSubCategories] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260226175822_AddMarketplaceCategoryTables', N'9.0.1');

COMMIT;
GO

