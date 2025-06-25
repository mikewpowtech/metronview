using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnitStatusesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Units_UnitDbId",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_UnitDbId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "UnitDbId",
                table: "Sensors");

            migrationBuilder.CreateTable(
                name: "UnitStatuses",
                columns: table => new
                {
                    DateReceivedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Mip = table.Column<bool>(type: "bit", nullable: false),
                    FailedCount = table.Column<bool>(type: "bit", nullable: false),
                    BatteryAlarm = table.Column<bool>(type: "bit", nullable: false),
                    AutoConfig = table.Column<bool>(type: "bit", nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signal = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitStatuses", x => new { x.DateReceivedUtc, x.UnitId });
                    table.ForeignKey(
                        name: "FK_UnitStatuses_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitStatuses_UnitId",
                table: "UnitStatuses",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnitStatuses");

            migrationBuilder.AddColumn<int>(
                name: "UnitDbId",
                table: "Sensors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_UnitDbId",
                table: "Sensors",
                column: "UnitDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Units_UnitDbId",
                table: "Sensors",
                column: "UnitDbId",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
