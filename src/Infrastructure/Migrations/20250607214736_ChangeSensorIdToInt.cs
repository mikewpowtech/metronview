using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSensorIdToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Companies_CompanyID",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_CompanyID",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Sensors");

            // 1. Add new identity column
            migrationBuilder.AddColumn<int>(
                name: "NewId",
                table: "Sensors",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 2. (Optional) Copy data if needed, update FKs in related tables

            // 3. Drop old PK and column
            migrationBuilder.DropPrimaryKey(
                name: "PK_Sensors",
                table: "Sensors");
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Sensors");

            // 4. Rename new column to Id and add PK
            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "Sensors",
                newName: "Id");
            migrationBuilder.AddPrimaryKey(
                name: "PK_Sensors",
                table: "Sensors",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Readings",
                columns: table => new
                {
                    DateRecordedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SensorId = table.Column<int>(type: "int", nullable: false),
                    DateReceivedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value1 = table.Column<double>(type: "float", nullable: true),
                    Value2 = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => new { x.DateRecordedUtc, x.SensorId });
                    table.ForeignKey(
                        name: "FK_Readings_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Readings_SensorId",
                table: "Readings",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Readings");

            // 2. Drop the int identity Id column and add back the string Id column
            migrationBuilder.DropPrimaryKey(
                name: "PK_Sensors",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Sensors");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Sensors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sensors",
                table: "Sensors",
                column: "Id");

            migrationBuilder.AddColumn<string>(
                name: "CompanyID",
                table: "Sensors",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_CompanyID",
                table: "Sensors",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Companies_CompanyID",
                table: "Sensors",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
