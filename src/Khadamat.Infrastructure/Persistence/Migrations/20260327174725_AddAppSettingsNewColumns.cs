using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettingsNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApkFilename",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApkIconUrl",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationNameAr",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationNameEn",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FindServiceSound",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MessageReceivedSound",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NotificationReceivedSound",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpenAppSound",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpenDetailsSound",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApkFilename",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "ApkIconUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "ApplicationNameAr",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "ApplicationNameEn",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "FindServiceSound",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MessageReceivedSound",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "NotificationReceivedSound",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "OpenAppSound",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "OpenDetailsSound",
                table: "AppSettings");
        }
    }
}
