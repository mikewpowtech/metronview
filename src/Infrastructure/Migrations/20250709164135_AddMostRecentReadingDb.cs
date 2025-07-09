using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMostRecentReadingDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitStatuses_Units_UnitId",
                table: "UnitStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UnitStatuses",
                table: "UnitStatuses");

            migrationBuilder.RenameTable(
                name: "UnitStatuses",
                newName: "UnitStatusDb");

            migrationBuilder.RenameIndex(
                name: "IX_UnitStatuses_UnitId",
                table: "UnitStatusDb",
                newName: "IX_UnitStatusDb_UnitId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UnitStatusDb",
                table: "UnitStatusDb",
                columns: new[] { "DateReceivedUtc", "UnitId" });

            migrationBuilder.CreateTable(
                name: "MostRecentReadings",
                columns: table => new
                {
                    SensorId = table.Column<int>(type: "int", nullable: false),
                    DateRecordedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DateReceivedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MostRecentReadings", x => new { x.DateRecordedUtc, x.SensorId });
                    table.ForeignKey(
                        name: "FK_MostRecentReadings_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MostRecentReadings_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MostRecentReadings_SensorId_Unique",
                table: "MostRecentReadings",
                column: "SensorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MostRecentReadings_UnitId",
                table: "MostRecentReadings",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitStatusDb_Units_UnitId",
                table: "UnitStatusDb",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitStatusDb_Units_UnitId",
                table: "UnitStatusDb");

            migrationBuilder.DropTable(
                name: "MostRecentReadings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UnitStatusDb",
                table: "UnitStatusDb");

            migrationBuilder.RenameTable(
                name: "UnitStatusDb",
                newName: "UnitStatuses");

            migrationBuilder.RenameIndex(
                name: "IX_UnitStatusDb_UnitId",
                table: "UnitStatuses",
                newName: "IX_UnitStatuses_UnitId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UnitStatuses",
                table: "UnitStatuses",
                columns: new[] { "DateReceivedUtc", "UnitId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UnitStatuses_Units_UnitId",
                table: "UnitStatuses",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
