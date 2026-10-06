IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AppSettings] (
    [Id] int NOT NULL IDENTITY,
    [ApplicationName] nvarchar(max) NOT NULL,
    [LogoUrl] nvarchar(max) NOT NULL,
    [PrimaryColor] nvarchar(max) NOT NULL,
    [SecondaryColor] nvarchar(max) NOT NULL,
    [ContactEmail] nvarchar(max) NOT NULL,
    [ContactPhone] nvarchar(max) NOT NULL,
    [IsMaintenanceMode] bit NOT NULL,
    [WelcomeMessage] nvarchar(max) NOT NULL,
    [AllowUserRegistration] bit NOT NULL,
    [RequireEmailVerification] bit NOT NULL,
    [MaxServicesPerProvider] int NOT NULL,
    [EnableReviewAutoApproval] bit NOT NULL,
    [FacebookUrl] nvarchar(max) NOT NULL,
    [TwitterUrl] nvarchar(max) NOT NULL,
    [InstagramUrl] nvarchar(max) NOT NULL,
    [TermsAndConditions] nvarchar(max) NOT NULL,
    [PrivacyPolicy] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_AppSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [EntityName] nvarchar(max) NOT NULL,
    [EntityId] nvarchar(max) NOT NULL,
    [Details] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [Governorates] (
    [Id] int NOT NULL IDENTITY,
    [Governorate_Name_AR] nvarchar(max) NOT NULL,
    [Governorate_Name_EN] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [Approved] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [UserCreated] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Governorates] PRIMARY KEY ([Id])
);

CREATE TABLE [MainCategories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Icon] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [Color] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_MainCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [Messages] (
    [Id] int NOT NULL IDENTITY,
    [SenderId] nvarchar(max) NOT NULL,
    [ReceiverId] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id])
);

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [Title] nvarchar(100) NOT NULL,
    [Message] nvarchar(500) NOT NULL,
    [RelatedLink] nvarchar(max) NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
);

CREATE TABLE [SubscriptionPlans] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [DurationInDays] int NOT NULL,
    [MaxServices] int NOT NULL,
    [IsFeatured] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Cities] (
    [Id] int NOT NULL IDENTITY,
    [GovernorateId] int NOT NULL,
    [City_Name_AR] nvarchar(max) NOT NULL,
    [City_Name_EN] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [Approved] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [UserCreated] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Cities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Cities_Governorates_GovernorateId] FOREIGN KEY ([GovernorateId]) REFERENCES [Governorates] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [MainCategoryId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_MainCategories_MainCategoryId] FOREIGN KEY ([MainCategoryId]) REFERENCES [MainCategories] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [IsActive] bit NOT NULL,
    [IsProvider] bit NOT NULL,
    [IsVerified] bit NOT NULL,
    [ProfileImageUrl] nvarchar(max) NULL,
    [Gender] nvarchar(max) NULL,
    [Bio] nvarchar(max) NULL,
    [WebsiteUrl] nvarchar(max) NULL,
    [InstagramUrl] nvarchar(max) NULL,
    [TwitterUrl] nvarchar(max) NULL,
    [FacebookUrl] nvarchar(max) NULL,
    [LinkedInUrl] nvarchar(max) NULL,
    [TikTokUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    [CityId] int NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiryTime] datetime2 NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id])
);

CREATE TABLE [SubCategories] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_SubCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProviderProfiles] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [BusinessName] nvarchar(max) NOT NULL,
    [Bio] nvarchar(max) NOT NULL,
    [Photo] nvarchar(max) NOT NULL,
    [Location] nvarchar(max) NOT NULL,
    [CityId] int NULL,
    [ContactNumber] nvarchar(max) NOT NULL,
    [WebsiteUrl] nvarchar(max) NULL,
    [InstagramUrl] nvarchar(max) NULL,
    [TwitterUrl] nvarchar(max) NULL,
    [Verified] bit NOT NULL,
    [IdCardImage] nvarchar(max) NULL,
    [CertificateImage] nvarchar(max) NULL,
    [SubscriptionId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ProviderProfiles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProviderProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_ProviderProfiles_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id])
);

CREATE TABLE [Ads] (
    [Id] int NOT NULL IDENTITY,
    [ActivityID] int NULL,
    [CategoryID] int NULL,
    [SubCategoryID] int NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [AdType] nvarchar(max) NOT NULL,
    [ImagePath] nvarchar(max) NULL,
    [VideoUrl] nvarchar(max) NULL,
    [TextContent] nvarchar(max) NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [DisplayOrder] int NOT NULL,
    [Approved] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [UserCreated] nvarchar(max) NULL,
    [RedirectUrl] nvarchar(max) NULL,
    [Placement] nvarchar(max) NULL,
    [Views] int NOT NULL,
    [Clicks] int NOT NULL,
    [City] nvarchar(max) NULL,
    [Governorate] nvarchar(max) NULL,
    [TargetGovernorates] nvarchar(max) NULL,
    [TargetCities] nvarchar(max) NULL,
    [TargetServices] nvarchar(max) NULL,
    [TargetUserGender] nvarchar(max) NULL,
    [TargetDays] nvarchar(max) NULL,
    [TargetMonths] nvarchar(max) NULL,
    [TargetTimeStart] time NULL,
    [TargetTimeEnd] time NULL,
    [TargetKeywords] nvarchar(max) NULL,
    [ShowImage] bit NOT NULL,
    [ShowText] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Ads] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ads_Categories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [Categories] ([Id]),
    CONSTRAINT [FK_Ads_SubCategories_SubCategoryID] FOREIGN KEY ([SubCategoryID]) REFERENCES [SubCategories] ([Id])
);

CREATE TABLE [Posts] (
    [Id] int NOT NULL IDENTITY,
    [ProviderId] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Posts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Posts_ProviderProfiles_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [ProviderProfiles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProviderSubscriptions] (
    [Id] int NOT NULL IDENTITY,
    [ProviderId] int NOT NULL,
    [PlanId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ProviderSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProviderSubscriptions_ProviderProfiles_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [ProviderProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProviderSubscriptions_SubscriptionPlans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Services] (
    [Id] int NOT NULL IDENTITY,
    [SubCategoryId] int NULL,
    [CategoryId] int NULL,
    [CityId] int NULL,
    [ProviderProfileId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [Phone1] nvarchar(max) NULL,
    [Phone2] nvarchar(max) NULL,
    [WhatsApp] nvarchar(max) NULL,
    [Facebook] nvarchar(max) NULL,
    [Telegram] nvarchar(max) NULL,
    [Work_Days] nvarchar(max) NULL,
    [Work_Houers] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [Price] decimal(18,2) NULL,
    [DisplayOrder] int NOT NULL,
    [Approved] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [UserCreated] nvarchar(max) NULL,
    [ViewsCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Services_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]),
    CONSTRAINT [FK_Services_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]),
    CONSTRAINT [FK_Services_ProviderProfiles_ProviderProfileId] FOREIGN KEY ([ProviderProfileId]) REFERENCES [ProviderProfiles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Services_SubCategories_SubCategoryId] FOREIGN KEY ([SubCategoryId]) REFERENCES [SubCategories] ([Id])
);

CREATE TABLE [AdImages] (
    [Id] int NOT NULL IDENTITY,
    [AdId] int NOT NULL,
    [ImagePath] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_AdImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdImages_Ads_AdId] FOREIGN KEY ([AdId]) REFERENCES [Ads] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Comments] (
    [Id] int NOT NULL IDENTITY,
    [PostId] int NOT NULL,
    [UserId] nvarchar(max) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Favorites] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [ServiceId] int NULL,
    [ProviderId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Favorites_ProviderProfiles_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [ProviderProfiles] ([Id]),
    CONSTRAINT [FK_Favorites_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id])
);

CREATE TABLE [Likes] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [ServiceId] int NULL,
    [PostId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Likes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Likes_Posts_PostId] FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]),
    CONSTRAINT [FK_Likes_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id])
);

CREATE TABLE [Ratings] (
    [Id] int NOT NULL IDENTITY,
    [ServiceId] int NOT NULL,
    [UserId] nvarchar(max) NOT NULL,
    [Stars] int NOT NULL,
    [Comment] nvarchar(max) NOT NULL,
    [Date] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ratings_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AdImages_AdId] ON [AdImages] ([AdId]);

CREATE INDEX [IX_Ads_CategoryID] ON [Ads] ([CategoryID]);

CREATE INDEX [IX_Ads_SubCategoryID] ON [Ads] ([SubCategoryID]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE INDEX [IX_AspNetUsers_CityId] ON [AspNetUsers] ([CityId]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_Categories_MainCategoryId] ON [Categories] ([MainCategoryId]);

CREATE INDEX [IX_Cities_GovernorateId] ON [Cities] ([GovernorateId]);

CREATE INDEX [IX_Comments_PostId] ON [Comments] ([PostId]);

CREATE INDEX [IX_Favorites_ProviderId] ON [Favorites] ([ProviderId]);

CREATE INDEX [IX_Favorites_ServiceId] ON [Favorites] ([ServiceId]);

CREATE INDEX [IX_Likes_PostId] ON [Likes] ([PostId]);

CREATE INDEX [IX_Likes_ServiceId] ON [Likes] ([ServiceId]);

CREATE INDEX [IX_Posts_ProviderId] ON [Posts] ([ProviderId]);

CREATE INDEX [IX_ProviderProfiles_CityId] ON [ProviderProfiles] ([CityId]);

CREATE UNIQUE INDEX [IX_ProviderProfiles_UserId] ON [ProviderProfiles] ([UserId]) WHERE [UserId] IS NOT NULL;

CREATE INDEX [IX_ProviderSubscriptions_PlanId] ON [ProviderSubscriptions] ([PlanId]);

CREATE UNIQUE INDEX [IX_ProviderSubscriptions_ProviderId] ON [ProviderSubscriptions] ([ProviderId]);

CREATE INDEX [IX_Ratings_ServiceId] ON [Ratings] ([ServiceId]);

CREATE INDEX [IX_Services_CategoryId] ON [Services] ([CategoryId]);

CREATE INDEX [IX_Services_CityId] ON [Services] ([CityId]);

CREATE INDEX [IX_Services_ProviderProfileId] ON [Services] ([ProviderProfileId]);

CREATE INDEX [IX_Services_SubCategoryId] ON [Services] ([SubCategoryId]);

CREATE INDEX [IX_SubCategories_CategoryId] ON [SubCategories] ([CategoryId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260212003913_InitialCreate', N'9.0.1');

CREATE TABLE [ServiceRequests] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [ServiceId] int NOT NULL,
    [ProviderId] int NOT NULL,
    [Status] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [ProviderNotes] nvarchar(max) NULL,
    [PreferredDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_ProviderProfiles_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [ProviderProfiles] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ServiceRequests_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ServiceRequests_ProviderId] ON [ServiceRequests] ([ProviderId]);

CREATE INDEX [IX_ServiceRequests_ServiceId] ON [ServiceRequests] ([ServiceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260213030004_AddServiceRequest', N'9.0.1');

EXEC sp_rename N'[MainCategories].[DisplayOrder]', N'Order', 'COLUMN';

CREATE TABLE [MarketplaceItems] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [SubCategoryId] int NULL,
    [CityId] int NULL,
    [SellerId] nvarchar(450) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Currency] nvarchar(max) NOT NULL,
    [Condition] nvarchar(max) NOT NULL,
    [ItemStatus] nvarchar(max) NOT NULL,
    [ContactPhone] nvarchar(max) NOT NULL,
    [Approved] bit NOT NULL,
    [ViewsCount] int NOT NULL,
    [AdminNotes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_MarketplaceItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MarketplaceItems_AspNetUsers_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MarketplaceItems_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MarketplaceItems_Cities_CityId] FOREIGN KEY ([CityId]) REFERENCES [Cities] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MarketplaceItems_SubCategories_SubCategoryId] FOREIGN KEY ([SubCategoryId]) REFERENCES [SubCategories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [MarketplaceImages] (
    [Id] int NOT NULL IDENTITY,
    [MarketplaceItemId] int NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [IsMain] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_MarketplaceImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MarketplaceImages_MarketplaceItems_MarketplaceItemId] FOREIGN KEY ([MarketplaceItemId]) REFERENCES [MarketplaceItems] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_MarketplaceImages_MarketplaceItemId] ON [MarketplaceImages] ([MarketplaceItemId]);

CREATE INDEX [IX_MarketplaceItems_CategoryId] ON [MarketplaceItems] ([CategoryId]);

CREATE INDEX [IX_MarketplaceItems_CityId] ON [MarketplaceItems] ([CityId]);

CREATE INDEX [IX_MarketplaceItems_SellerId] ON [MarketplaceItems] ([SellerId]);

CREATE INDEX [IX_MarketplaceItems_SubCategoryId] ON [MarketplaceItems] ([SubCategoryId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260224124255_AddMarketplace', N'9.0.1');

ALTER TABLE [MarketplaceItems] ADD [FeaturedUntil] datetime2 NULL;

ALTER TABLE [MarketplaceItems] ADD [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [MarketplaceItems] ADD [IsPromoted] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [MarketplaceItems] ADD [PromotedUntil] datetime2 NULL;

ALTER TABLE [Favorites] ADD [MarketplaceItemId] int NULL;

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [MarketplaceItemId] int NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(max) NOT NULL,
    [PaymentType] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [ExternalTransactionId] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [DeletedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_MarketplaceItems_MarketplaceItemId] FOREIGN KEY ([MarketplaceItemId]) REFERENCES [MarketplaceItems] ([Id]) ON DELETE SET NULL
);

CREATE INDEX [IX_Favorites_MarketplaceItemId] ON [Favorites] ([MarketplaceItemId]);

CREATE INDEX [IX_Payments_MarketplaceItemId] ON [Payments] ([MarketplaceItemId]);

ALTER TABLE [Favorites] ADD CONSTRAINT [FK_Favorites_MarketplaceItems_MarketplaceItemId] FOREIGN KEY ([MarketplaceItemId]) REFERENCES [MarketplaceItems] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260225194200_AddPaymentsAndMarketplaceUpdates', N'9.0.1');

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

