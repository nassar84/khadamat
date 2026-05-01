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
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "SubCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

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



            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);
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
