using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceListingDateAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "MarketplaceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ListedAt",
                table: "MarketplaceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SoldDate",
                table: "MarketplaceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "MarketplaceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarketplaceAutoExpire",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MarketplaceDefaultListingDays",
                table: "AppSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarketplaceMaxListingsPerUser",
                table: "AppSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MarketplaceRequireApproval",
                table: "AppSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MarketplaceItemViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketplaceItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceItemViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceItemViews_MarketplaceItems_MarketplaceItemId",
                        column: x => x.MarketplaceItemId,
                        principalTable: "MarketplaceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceItemViews_MarketplaceItemId",
                table: "MarketplaceItemViews",
                column: "MarketplaceItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceItemViews");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "ListedAt",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "SoldDate",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "MarketplaceItems");

            migrationBuilder.DropColumn(
                name: "MarketplaceAutoExpire",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MarketplaceDefaultListingDays",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MarketplaceMaxListingsPerUser",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "MarketplaceRequireApproval",
                table: "AppSettings");
        }
    }
}
