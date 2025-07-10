using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseTriggerTypeCodeEnumForTriggerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new nullable column for TriggerTypeCode
            migrationBuilder.AddColumn<string>(
                name: "TriggerTypeCode",
                table: "Triggers",
                type: "char(1)",
                nullable: true);

            // 2. Copy data from TriggerTypeId to TriggerTypeCode by joining TriggerTypes
            migrationBuilder.Sql(@"
                UPDATE T
                SET T.TriggerTypeCode = TT.Code
                FROM Triggers T
                INNER JOIN TriggerTypes TT ON T.TriggerTypeId = TT.Id
            ");

            // 3. Make TriggerTypeCode non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "TriggerTypeCode",
                table: "Triggers",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            // 4. Remove old FK and index, then drop TriggerTypeId
            migrationBuilder.DropForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeId",
                table: "Triggers");
            migrationBuilder.DropIndex(
                name: "IX_Triggers_TriggerTypeId",
                table: "Triggers");
            migrationBuilder.DropColumn(
                name: "TriggerTypeId",
                table: "Triggers");

            // 5. Alter TriggerTypes.Code to char(1)
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TriggerTypes",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 6. Add unique constraint, index, and FK for new column
            migrationBuilder.AddUniqueConstraint(
                name: "AK_TriggerTypes_Code",
                table: "TriggerTypes",
                column: "Code");
            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TriggerTypeCode",
                table: "Triggers",
                column: "TriggerTypeCode");
            migrationBuilder.AddForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeCode",
                table: "Triggers",
                column: "TriggerTypeCode",
                principalTable: "TriggerTypes",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeCode",
                table: "Triggers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TriggerTypes_Code",
                table: "TriggerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Triggers_TriggerTypeCode",
                table: "Triggers");

            migrationBuilder.DropColumn(
                name: "TriggerTypeCode",
                table: "Triggers");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "TriggerTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AddColumn<int>(
                name: "TriggerTypeId",
                table: "Triggers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TriggerTypeId",
                table: "Triggers",
                column: "TriggerTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeId",
                table: "Triggers",
                column: "TriggerTypeId",
                principalTable: "TriggerTypes",
                principalColumn: "Id");
        }
    }
}
