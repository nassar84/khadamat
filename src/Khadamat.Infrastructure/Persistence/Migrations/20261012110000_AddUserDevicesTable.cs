using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDevicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserDevices')
BEGIN
    CREATE TABLE [UserDevices] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [DeviceToken] nvarchar(450) NOT NULL,
        [Platform] nvarchar(50) NOT NULL DEFAULT 'android',
        [DeviceId] nvarchar(255) NULL,
        [AppVersion] nvarchar(50) NULL,
        [LastActiveAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_UserDevices] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_UserDevices_DeviceToken] ON [UserDevices] ([DeviceToken]);
    CREATE INDEX [IX_UserDevices_UserId] ON [UserDevices] ([UserId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserDevices')
BEGIN
    DROP TABLE [UserDevices];
END
");
        }
    }
}
