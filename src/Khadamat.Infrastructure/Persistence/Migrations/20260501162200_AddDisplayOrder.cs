using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[SubCategories]') AND name = 'DisplayOrder') BEGIN ALTER TABLE [SubCategories] ADD [DisplayOrder] int NOT NULL DEFAULT 0; END");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "ServiceEditRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedAddress",
                table: "ServiceEditRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedDescription",
                table: "ServiceEditRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedName",
                table: "ServiceEditRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedPhone1",
                table: "ServiceEditRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedPrice",
                table: "ServiceEditRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProposedPhone2",
                table: "ServiceEditRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProposedWhatsApp",
                table: "ServiceEditRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderNotes",
                table: "ServiceEditRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Categories]') AND name = 'DisplayOrder') BEGIN ALTER TABLE [Categories] ADD [DisplayOrder] int NOT NULL DEFAULT 0; END");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Services]') AND name = 'DisplayOrder') BEGIN ALTER TABLE [Services] ADD [DisplayOrder] int NOT NULL DEFAULT 0; END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "SubCategories");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedAddress",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedDescription",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedName",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedPhone1",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedPrice",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ProposedPhone2",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ProposedWhatsApp",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "ProviderNotes",
                table: "ServiceEditRequests");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Services");
        }
    }
}
