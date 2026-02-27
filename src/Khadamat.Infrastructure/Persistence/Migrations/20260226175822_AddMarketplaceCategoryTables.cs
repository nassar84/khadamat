using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceCategoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarketplaceItems_Categories_CategoryId",
                table: "MarketplaceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MarketplaceItems_SubCategories_SubCategoryId",
                table: "MarketplaceItems");

            migrationBuilder.CreateTable(
                name: "MarketplaceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceSubCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceSubCategories_MarketplaceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MarketplaceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceSubCategories_CategoryId",
                table: "MarketplaceSubCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MarketplaceItems_MarketplaceCategories_CategoryId",
                table: "MarketplaceItems",
                column: "CategoryId",
                principalTable: "MarketplaceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MarketplaceItems_MarketplaceSubCategories_SubCategoryId",
                table: "MarketplaceItems",
                column: "SubCategoryId",
                principalTable: "MarketplaceSubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MarketplaceItems_MarketplaceCategories_CategoryId",
                table: "MarketplaceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MarketplaceItems_MarketplaceSubCategories_SubCategoryId",
                table: "MarketplaceItems");

            migrationBuilder.DropTable(
                name: "MarketplaceSubCategories");

            migrationBuilder.DropTable(
                name: "MarketplaceCategories");

            migrationBuilder.AddForeignKey(
                name: "FK_MarketplaceItems_Categories_CategoryId",
                table: "MarketplaceItems",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MarketplaceItems_SubCategories_SubCategoryId",
                table: "MarketplaceItems",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
