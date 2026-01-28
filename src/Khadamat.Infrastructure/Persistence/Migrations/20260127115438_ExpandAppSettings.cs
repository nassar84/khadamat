using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowUserRegistration",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableReviewAutoApproval",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxServicesPerProvider",
                table: "AppSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicy",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmailVerification",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TermsAndConditions",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TwitterUrl",
                table: "AppSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowUserRegistration",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "EnableReviewAutoApproval",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MaxServicesPerProvider",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicy",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "RequireEmailVerification",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TermsAndConditions",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "TwitterUrl",
                table: "AppSettings");
        }
    }
}
