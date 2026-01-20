using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedAccountModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ProviderProfiles_UserId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_UserId",
                table: "Services");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProviderProfiles_UserId",
                table: "ProviderProfiles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "ProviderProfileId",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProviderProfiles",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ProviderProfileId",
                table: "Services",
                column: "ProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderProfiles_UserId",
                table: "ProviderProfiles",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderProfiles_AspNetUsers_UserId",
                table: "ProviderProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ProviderProfiles_ProviderProfileId",
                table: "Services",
                column: "ProviderProfileId",
                principalTable: "ProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderProfiles_AspNetUsers_UserId",
                table: "ProviderProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_ProviderProfiles_ProviderProfileId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ProviderProfileId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ProviderProfiles_UserId",
                table: "ProviderProfiles");

            migrationBuilder.DropColumn(
                name: "ProviderProfileId",
                table: "Services");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Services",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProviderProfiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProviderProfiles_UserId",
                table: "ProviderProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_UserId",
                table: "Services",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ProviderProfiles_UserId",
                table: "Services",
                column: "UserId",
                principalTable: "ProviderProfiles",
                principalColumn: "UserId");
        }
    }
}
