using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyAlarmOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Order",
                table: "Alarms",
                newName: "RecipientSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_CompanyId",
                table: "Alarms",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Alarms_RecipientSetId",
                table: "Alarms",
                column: "RecipientSetId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_Companies_CompanyId",
                table: "Alarms");

            migrationBuilder.DropForeignKey(
                name: "FK_Alarms_RecipientSets_RecipientSetId",
                table: "Alarms");

            migrationBuilder.DropIndex(
                name: "IX_Alarms_CompanyId",
                table: "Alarms");

            migrationBuilder.DropIndex(
                name: "IX_Alarms_RecipientSetId",
                table: "Alarms");

            migrationBuilder.RenameColumn(
                name: "RecipientSetId",
                table: "Alarms",
                newName: "Order");
        }
    }
}
