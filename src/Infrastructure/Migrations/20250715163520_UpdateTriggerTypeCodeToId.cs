using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTriggerTypeCodeToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Seed TriggerTypes table if it's empty
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM TriggerTypes)
                BEGIN
                    INSERT INTO TriggerTypes (Code, Name, [Order])
                    VALUES 
                        ('A', 'Above Threshold', 1),
                        ('B', 'Below Threshold', 2),
                        ('C', 'Rate of Change', 3),
                        ('S', 'Not Reported For Period', 4),
                        ('T', 'Clock Reset', 5),
                        ('U', 'Rising Edge', 6),
                        ('D', 'Falling Edge', 7)
                END
            ");

            // Step 2: Add TriggerTypeId column (nullable first)
            migrationBuilder.AddColumn<int>(
                name: "TriggerTypeId",
                table: "Triggers",
                type: "int",
                nullable: true);

            // Step 3: Update existing Triggers to map TriggerTypeCode to TriggerTypeId
            migrationBuilder.Sql(@"
                UPDATE t 
                SET t.TriggerTypeId = tt.Id
                FROM Triggers t
                INNER JOIN TriggerTypes tt ON t.TriggerTypeCode = tt.Code
                WHERE t.TriggerTypeCode IS NOT NULL
            ");

            // Step 4: Verify all triggers have been mapped
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Triggers WHERE TriggerTypeId IS NULL)
                BEGIN
                    RAISERROR('Found Triggers that could not be mapped to TriggerTypes', 16, 1)
                    RETURN
                END
            ");

            // Step 5: Make TriggerTypeId non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "TriggerTypeId",
                table: "Triggers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 6: Drop old foreign key and constraints
            migrationBuilder.DropForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeCode",
                table: "Triggers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_TriggerTypes_Code",
                table: "TriggerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Triggers_TriggerTypeCode",
                table: "Triggers");

            // Step 7: Drop old TriggerTypeCode column
            migrationBuilder.DropColumn(
                name: "TriggerTypeCode",
                table: "Triggers");

            // Step 8: Create new indexes
            migrationBuilder.CreateIndex(
                name: "IX_TriggerTypes_Code",
                table: "TriggerTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TriggerTypeId",
                table: "Triggers",
                column: "TriggerTypeId");

            // Step 9: Add new foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeId",
                table: "Triggers",
                column: "TriggerTypeId",
                principalTable: "TriggerTypes",
                principalColumn: "Id");

            // Step 10: Create CustomFields table
            migrationBuilder.CreateTable(
                name: "CustomFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignKeyId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CustomFieldType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFields", x => x.Id);
                });

            // Step 11: Create MostRecentAlarms table
            migrationBuilder.CreateTable(
                name: "MostRecentAlarms",
                columns: table => new
                {
                    SensorId = table.Column<int>(type: "int", nullable: false),
                    AlarmId = table.Column<int>(type: "int", nullable: false),
                    MostRecentSendUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MostRecentAlarms", x => new { x.SensorId, x.AlarmId });
                    table.ForeignKey(
                        name: "FK_MostRecentAlarms_Alarms_AlarmId",
                        column: x => x.AlarmId,
                        principalTable: "Alarms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MostRecentAlarms_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id");
                });

            // Step 12: Create indexes for new tables
            migrationBuilder.CreateIndex(
                name: "IX_CustomFields_ForeignKeyId_CustomFieldType",
                table: "CustomFields",
                columns: new[] { "ForeignKeyId", "CustomFieldType" });

            migrationBuilder.CreateIndex(
                name: "IX_MostRecentAlarms_AlarmId",
                table: "MostRecentAlarms",
                column: "AlarmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop new foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeId",
                table: "Triggers");

            // Step 2: Drop new tables
            migrationBuilder.DropTable(
                name: "CustomFields");

            migrationBuilder.DropTable(
                name: "MostRecentAlarms");

            // Step 3: Drop new indexes
            migrationBuilder.DropIndex(
                name: "IX_TriggerTypes_Code",
                table: "TriggerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Triggers_TriggerTypeId",
                table: "Triggers");

            // Step 4: Add back TriggerTypeCode column
            migrationBuilder.AddColumn<string>(
                name: "TriggerTypeCode",
                table: "Triggers",
                type: "char(1)",
                nullable: true);

            // Step 5: Restore TriggerTypeCode values from TriggerTypeId
            migrationBuilder.Sql(@"
                UPDATE t 
                SET t.TriggerTypeCode = tt.Code
                FROM Triggers t
                INNER JOIN TriggerTypes tt ON t.TriggerTypeId = tt.Id
            ");

            // Step 6: Make TriggerTypeCode non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "TriggerTypeCode",
                table: "Triggers",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)",
                oldNullable: true);

            // Step 7: Drop TriggerTypeId column
            migrationBuilder.DropColumn(
                name: "TriggerTypeId",
                table: "Triggers");

            // Step 8: Restore old constraints and indexes
            migrationBuilder.AddUniqueConstraint(
                name: "AK_TriggerTypes_Code",
                table: "TriggerTypes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TriggerTypeCode",
                table: "Triggers",
                column: "TriggerTypeCode");

            // Step 9: Restore old foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Triggers_TriggerTypes_TriggerTypeCode",
                table: "Triggers",
                column: "TriggerTypeCode",
                principalTable: "TriggerTypes",
                principalColumn: "Code");
        }
    }
}
