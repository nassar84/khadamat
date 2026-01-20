using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities_20260117 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Governorates",
                newName: "Governorate_Name_EN");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Cities",
                newName: "City_Name_EN");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Governorates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Governorates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Governorate_Name_AR",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCreated",
                table: "Governorates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Cities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "City_Name_AR",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Cities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCreated",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "Governorate_Name_AR",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "City_Name_AR",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UserCreated",
                table: "Cities");

            migrationBuilder.RenameColumn(
                name: "Governorate_Name_EN",
                table: "Governorates",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "City_Name_EN",
                table: "Cities",
                newName: "Name");
        }
    }
}
