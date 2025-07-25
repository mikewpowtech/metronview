using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MopUpDatabaseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BatteryAlarm",
                table: "UnitStatuses",
                newName: "BattAlarm");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Companies",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Companies",
                newName: "CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "Carrier",
                table: "UnitStatuses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Companies",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AlarmEmailFromAddress",
                table: "Companies",
                type: "varchar(254)",
                unicode: false,
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlarmEmailReplyToAddress",
                table: "Companies",
                type: "varchar(254)",
                unicode: false,
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlarmSmsBodyTemplate",
                table: "Companies",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlarmSmsSubjectTemplate",
                table: "Companies",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlarmSmsToAddressTemplate",
                table: "Companies",
                type: "varchar(254)",
                unicode: false,
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomFieldDefinitions",
                table: "Companies",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dashboard",
                table: "Companies",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysBeforeRTUDataDeletion",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultDaysBeforeNotReported",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultDaysHistory",
                table: "Companies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostHeader",
                table: "Companies",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ParentCompanyId",
                table: "Companies",
                column: "ParentCompanyId");

            migrationBuilder.CreateIndex(
                name: "Two companies cannot have the same name",
                table: "Companies",
                column: "CompanyName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "Cannot delete a company that manages other companies",
                table: "Companies",
                column: "ParentCompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Cannot delete a company that manages other companies",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ParentCompanyId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "Two companies cannot have the same name",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AlarmEmailFromAddress",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AlarmEmailReplyToAddress",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AlarmSmsBodyTemplate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AlarmSmsSubjectTemplate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AlarmSmsToAddressTemplate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CustomFieldDefinitions",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Dashboard",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DaysBeforeRTUDataDeletion",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DefaultDaysBeforeNotReported",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DefaultDaysHistory",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "HostHeader",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "BattAlarm",
                table: "UnitStatuses",
                newName: "BatteryAlarm");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Companies",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "Companies",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Carrier",
                table: "UnitStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }
    }
}
