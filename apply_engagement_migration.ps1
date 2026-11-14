$connectionString = "Data Source=SQL5065.site4now.net;Initial Catalog=db_abe0b0_khadamatdb;User Id=db_abe0b0_khadamatdb_admin;Password=Secure#OS2025;Encrypt=True;TrustServerCertificate=True;"

$sql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Services') AND name = 'DoNotContact')
BEGIN
    ALTER TABLE Services ADD DoNotContact bit NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('MarketingCampaigns') AND type = 'U')
BEGIN
    CREATE TABLE MarketingCampaigns (
        Id int IDENTITY(1,1) PRIMARY KEY,
        CampaignName nvarchar(200) NOT NULL,
        CityId int NULL,
        CityName nvarchar(100) NOT NULL,
        GovernorateId int NULL,
        GovernorateName nvarchar(100) NOT NULL,
        IsActive bit NOT NULL DEFAULT 1,
        IsDefault bit NOT NULL DEFAULT 0,
        StartDate datetime2 NULL,
        EndDate datetime2 NULL,
        Slogan nvarchar(300) NOT NULL,
        MessageTemplate nvarchar(max) NOT NULL,
        SubscriptionOfferMonths int NOT NULL DEFAULT 3,
        SubscriptionOfferText nvarchar(200) NOT NULL,
        AppDownloadUrl nvarchar(500) NULL,
        FacebookCommunityUrl nvarchar(500) NOT NULL,
        TargetStage nvarchar(50) NOT NULL DEFAULT 'Launch',
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0
    );
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ProfessionalTitleRules') AND type = 'U')
BEGIN
    CREATE TABLE ProfessionalTitleRules (
        Id int IDENTITY(1,1) PRIMARY KEY,
        ProfessionKeyword nvarchar(100) NOT NULL,
        MaleTitle nvarchar(50) NOT NULL,
        FemaleTitle nvarchar(50) NOT NULL,
        DefaultTitle nvarchar(50) NOT NULL,
        IsActive bit NOT NULL DEFAULT 1,
        Priority int NOT NULL DEFAULT 0,
        Notes nvarchar(500) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0
    );
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ProviderEngagementReminders') AND type = 'U')
BEGIN
    CREATE TABLE ProviderEngagementReminders (
        Id int IDENTITY(1,1) PRIMARY KEY,
        ReminderType nvarchar(100) NOT NULL,
        Title nvarchar(200) NOT NULL,
        Description nvarchar(500) NOT NULL,
        TargetChannel nvarchar(50) NOT NULL DEFAULT 'All',
        WhatsAppTemplate nvarchar(max) NOT NULL,
        PushTitleTemplate nvarchar(200) NOT NULL,
        PushBodyTemplate nvarchar(500) NOT NULL,
        DeepLinkRoute nvarchar(300) NOT NULL DEFAULT '/service/{ServiceId}',
        DaysAfterRegistration int NOT NULL DEFAULT 3,
        IsActive bit NOT NULL DEFAULT 1,
        DisplayOrder int NOT NULL DEFAULT 0,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0
    );
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('ProviderEngagementLogs') AND type = 'U')
BEGIN
    CREATE TABLE ProviderEngagementLogs (
        Id int IDENTITY(1,1) PRIMARY KEY,
        ServiceId int NOT NULL,
        ProviderUserId nvarchar(450) NULL,
        ProviderProfileId int NULL,
        Channel nvarchar(50) NOT NULL,
        ReminderType nvarchar(100) NOT NULL,
        RecipientName nvarchar(200) NOT NULL,
        RecipientPhone nvarchar(50) NOT NULL,
        ProfessionalTitle nvarchar(50) NOT NULL,
        CityName nvarchar(100) NOT NULL,
        CampaignId int NULL,
        CampaignName nvarchar(200) NULL,
        Title nvarchar(200) NOT NULL,
        MessageText nvarchar(max) NOT NULL,
        Status nvarchar(50) NOT NULL DEFAULT 'Prepared',
        AdminUserId nvarchar(450) NULL,
        AdminUserName nvarchar(200) NULL,
        PreparedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        SentConfirmedAt datetime2 NULL,
        AdminNotes nvarchar(max) NULL,
        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt datetime2 NULL,
        IsDeleted bit NOT NULL DEFAULT 0
    );
END
"@

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $conn.Open()
    $cmd = New-Object System.Data.SqlClient.SqlCommand($sql, $conn)
    $cmd.ExecuteNonQuery() | Out-Null
    $conn.Close()
    Write-Host "Engagement and Marketing tables created or updated successfully!" -ForegroundColor Green
} catch {
    Write-Error $_.Exception.Message
}
