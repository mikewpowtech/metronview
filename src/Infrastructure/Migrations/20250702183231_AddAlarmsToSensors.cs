using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmsToSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units");

            migrationBuilder.AddColumn<int>(
                name: "AlarmId",
                table: "Sensors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Sensors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_AlarmId",
                table: "Sensors",
                column: "AlarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_CompanyId",
                table: "Sensors",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Alarms_AlarmId",
                table: "Sensors",
                column: "AlarmId",
                principalTable: "Alarms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Companies_CompanyId",
                table: "Sensors",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Alarms_AlarmId",
                table: "Sensors");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Companies_CompanyId",
                table: "Sensors");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_AlarmId",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_Sensors_CompanyId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "AlarmId",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Sensors");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
