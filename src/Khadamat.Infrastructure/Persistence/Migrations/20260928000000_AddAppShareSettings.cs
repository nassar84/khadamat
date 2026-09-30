using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppShareSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AppSettings]') AND name = 'AppShareUrl') BEGIN ALTER TABLE [AppSettings] ADD [AppShareUrl] nvarchar(max) NOT NULL DEFAULT ''; END");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AppSettings]') AND name = 'AppShareText') BEGIN ALTER TABLE [AppSettings] ADD [AppShareText] nvarchar(max) NOT NULL DEFAULT ''; END");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AppSettings]') AND name = 'AppStoreUrl') BEGIN ALTER TABLE [AppSettings] ADD [AppStoreUrl] nvarchar(max) NOT NULL DEFAULT ''; END");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AppSettings]') AND name = 'GooglePlayUrl') BEGIN ALTER TABLE [AppSettings] ADD [GooglePlayUrl] nvarchar(max) NOT NULL DEFAULT ''; END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppShareUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AppShareText",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "AppStoreUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GooglePlayUrl",
                table: "AppSettings");
        }
    }
}
