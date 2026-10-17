using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Khadamat.Infrastructure.Persistence;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(KhadamatDbContext))]
    [Migration("20261014120000_AddDefaultThemeToAppSettings")]
    public partial class AddDefaultThemeToAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AppSettings]') AND name = 'DefaultTheme') BEGIN ALTER TABLE [AppSettings] ADD [DefaultTheme] nvarchar(max) NOT NULL DEFAULT 'default'; END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultTheme",
                table: "AppSettings");
        }
    }
}
