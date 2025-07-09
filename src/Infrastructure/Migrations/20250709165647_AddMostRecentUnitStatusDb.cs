using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMostRecentUnitStatusDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitStatusDb_Units_UnitId",
                table: "UnitStatusDb");

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

            migrationBuilder.CreateTable(
                name: "MostRecentUnitStatuses",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DateReceivedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mip = table.Column<bool>(type: "bit", nullable: false),
                    FailedCallout = table.Column<bool>(type: "bit", nullable: false),
                    BatteryAlarm = table.Column<bool>(type: "bit", nullable: false),
                    AutoConfig = table.Column<bool>(type: "bit", nullable: false),
                    Temperature = table.Column<float>(type: "real", nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Signal = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MostRecentUnitStatuses", x => x.UnitId);
                    table.ForeignKey(
                        name: "FK_MostRecentUnitStatuses_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.AddForeignKey(
                name: "FK_UnitStatuses_Units_UnitId",
                table: "UnitStatuses",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitStatuses_Units_UnitId",
                table: "UnitStatuses");

            migrationBuilder.DropTable(
                name: "MostRecentUnitStatuses");

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

            migrationBuilder.AddForeignKey(
                name: "FK_UnitStatusDb_Units_UnitId",
                table: "UnitStatusDb",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }
    }
}
