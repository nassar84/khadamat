using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdTargetingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TargetCities",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetDays",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetGovernorates",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetMonths",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetServices",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TargetTimeEnd",
                table: "Ads",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "TargetTimeStart",
                table: "Ads",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetUserGender",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetCities",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetDays",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetGovernorates",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetMonths",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetServices",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetTimeEnd",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetTimeStart",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "TargetUserGender",
                table: "Ads");
        }
    }
}
