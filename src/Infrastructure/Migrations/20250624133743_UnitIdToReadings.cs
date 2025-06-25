using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnitIdToReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitDbId",
                table: "Sensors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Readings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_UnitDbId",
                table: "Sensors",
                column: "UnitDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Readings_UnitId",
                table: "Readings",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Units_UnitId",
                table: "Readings",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Units_UnitDbId",
                table: "Sensors",
                column: "UnitDbId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Units_UnitId",
                table: "Readings");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Units_UnitDbId",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_UnitDbId",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Readings_UnitId",
                table: "Readings");

            migrationBuilder.DropColumn(
                name: "UnitDbId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Readings");
        }
    }
}
