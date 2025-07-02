using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyAlarmRD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_Companies_CompanyId",
                table: "Alarms");

            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_RecipientSets_RecipientSetId",
                table: "Alarms");

            migrationBuilder.AddForeignKey(
                name: "FK_Alarms_Companies_CompanyId",
                table: "Alarms",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Alarms_RecipientSets_RecipientSetId",
                table: "Alarms",
                column: "RecipientSetId",
                principalTable: "RecipientSets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_Companies_CompanyId",
                table: "Alarms");

            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_RecipientSets_RecipientSetId",
                table: "Alarms");

            migrationBuilder.AddForeignKey(
                name: "FK_Alarms_Companies_CompanyId",
                table: "Alarms",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Alarms_RecipientSets_RecipientSetId",
                table: "Alarms",
                column: "RecipientSetId",
                principalTable: "RecipientSets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
