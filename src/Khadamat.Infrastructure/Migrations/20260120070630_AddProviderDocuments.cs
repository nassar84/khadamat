using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateImage",
                table: "ProviderProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdCardImage",
                table: "ProviderProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateImage",
                table: "ProviderProfiles");

            migrationBuilder.DropColumn(
                name: "IdCardImage",
                table: "ProviderProfiles");
        }
    }
}
