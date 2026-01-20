using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ProviderProfiles_ProviderId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ProviderId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Images",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "YouTubeUrl",
                table: "Services",
                newName: "Work_Houers");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Services",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ProviderId",
                table: "Services",
                newName: "ViewsCount");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Services",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "IsApproved",
                table: "Services",
                newName: "Approved");

            migrationBuilder.RenameColumn(
                name: "TargetUrl",
                table: "Ads",
                newName: "UserCreated");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Ads",
                newName: "ShowText");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Ads",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "Ads",
                newName: "StartDate");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone1",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone2",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telegram",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCreated",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Services",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Work_Days",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProviderProfiles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ActivityID",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Ads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CategoryID",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "Ads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Ads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Ads",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Placement",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowImage",
                table: "Ads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryID",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Ads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProviderProfiles_UserId",
                table: "ProviderProfiles",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "AdImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AdImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdImages_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_UserId",
                table: "Services",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_CategoryID",
                table: "Ads",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_SubCategoryID",
                table: "Ads",
                column: "SubCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_AdImages_AdId",
                table: "AdImages",
                column: "AdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Categories_CategoryID",
                table: "Ads",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_SubCategories_SubCategoryID",
                table: "Ads",
                column: "SubCategoryID",
                principalTable: "SubCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ProviderProfiles_UserId",
                table: "Services",
                column: "UserId",
                principalTable: "ProviderProfiles",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Categories_CategoryID",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Ads_SubCategories_SubCategoryID",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_ProviderProfiles_UserId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "AdImages");

            migrationBuilder.DropIndex(
                name: "IX_Services_UserId",
                table: "Services");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProviderProfiles_UserId",
                table: "ProviderProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Ads_CategoryID",
                table: "Ads");

            migrationBuilder.DropIndex(
                name: "IX_Ads_SubCategoryID",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Phone1",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Phone2",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Telegram",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Work_Days",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ActivityID",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "CategoryID",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Placement",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "ShowImage",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "SubCategoryID",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Ads");

            migrationBuilder.RenameColumn(
                name: "Work_Houers",
                table: "Services",
                newName: "YouTubeUrl");

            migrationBuilder.RenameColumn(
                name: "ViewsCount",
                table: "Services",
                newName: "ProviderId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Services",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Approved",
                table: "Services",
                newName: "IsApproved");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Services",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "UserCreated",
                table: "Ads",
                newName: "TargetUrl");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Ads",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "ShowText",
                table: "Ads",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Ads",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "Images",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProviderProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ProviderId",
                table: "Services",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ProviderProfiles_ProviderId",
                table: "Services",
                column: "ProviderId",
                principalTable: "ProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
