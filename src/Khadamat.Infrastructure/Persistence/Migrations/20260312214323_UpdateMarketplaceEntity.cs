using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarketplaceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Advertisements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId",
                table: "Advertisements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Ads",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ServiceID",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetDeepSubCategories",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_ServiceId",
                table: "Advertisements",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_SubCategoryId",
                table: "Advertisements",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_ServiceID",
                table: "Ads",
                column: "ServiceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Services_ServiceID",
                table: "Ads",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Services_ServiceId",
                table: "Advertisements",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_SubCategories_SubCategoryId",
                table: "Advertisements",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Services_ServiceID",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Services_ServiceId",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_SubCategories_SubCategoryId",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_ServiceId",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_SubCategoryId",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Ads_ServiceID",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "SubCategoryId",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetDeepSubCategories",
                table: "Ads");
        }
    }
}
