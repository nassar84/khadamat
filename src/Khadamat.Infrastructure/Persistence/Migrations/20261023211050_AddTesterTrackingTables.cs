using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Khadamat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTesterTrackingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Services_Specializations_SpecializationId')
                    ALTER TABLE Services DROP CONSTRAINT FK_Services_Specializations_SpecializationId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Specializations_Categories_CategoryId')
                    ALTER TABLE Specializations DROP CONSTRAINT FK_Specializations_Categories_CategoryId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Specializations_SubCategories_SubCategoryId')
                    ALTER TABLE Specializations DROP CONSTRAINT FK_Specializations_SubCategories_SubCategoryId;
            ");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryChannel",
                table: "ReminderCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ReminderCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageBankJson",
                table: "ReminderCampaigns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageSelectionMode",
                table: "ReminderCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TesterDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TesterEmail = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TesterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastStreakDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsecutiveDaysStreak = table.Column<int>(type: "int", nullable: false),
                    TotalActiveDays = table.Column<int>(type: "int", nullable: false),
                    TargetDays = table.Column<int>(type: "int", nullable: false),
                    HasMissedStreak = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_TesterDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TesterActivityLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TesterDeviceId = table.Column<int>(type: "int", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TesterEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsNativeApp = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TesterActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TesterActivityLogs_TesterDevices_TesterDeviceId",
                        column: x => x.TesterDeviceId,
                        principalTable: "TesterDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TesterActivityLogs_DeviceIdentifier",
                table: "TesterActivityLogs",
                column: "DeviceIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TesterActivityLogs_TesterDeviceId",
                table: "TesterActivityLogs",
                column: "TesterDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TesterActivityLogs_VisitDate",
                table: "TesterActivityLogs",
                column: "VisitDate");

            migrationBuilder.CreateIndex(
                name: "IX_TesterDevices_DeviceIdentifier",
                table: "TesterDevices",
                column: "DeviceIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TesterDevices_TesterEmail",
                table: "TesterDevices",
                column: "TesterEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Specializations_Categories_CategoryId",
                table: "Specializations",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Specializations_SubCategories_SubCategoryId",
                table: "Specializations",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Specializations_Categories_CategoryId",
                table: "Specializations");

            migrationBuilder.DropForeignKey(
                name: "FK_Specializations_SubCategories_SubCategoryId",
                table: "Specializations");

            migrationBuilder.DropTable(
                name: "TesterActivityLogs");

            migrationBuilder.DropTable(
                name: "TesterDevices");

            migrationBuilder.DropColumn(
                name: "DeliveryChannel",
                table: "ReminderCampaigns");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ReminderCampaigns");

            migrationBuilder.DropColumn(
                name: "MessageBankJson",
                table: "ReminderCampaigns");

            migrationBuilder.DropColumn(
                name: "MessageSelectionMode",
                table: "ReminderCampaigns");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Specializations_Categories_CategoryId",
                table: "Specializations",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Specializations_SubCategories_SubCategoryId",
                table: "Specializations",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
