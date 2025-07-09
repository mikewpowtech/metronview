using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMostRecentReadingDbStandalone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old composite PK if it exists
            migrationBuilder.DropPrimaryKey(
                name: "PK_MostRecentReadings",
                table: "MostRecentReadings");

            // Remove any old indexes if needed
            migrationBuilder.DropIndex(
                name: "IX_MostRecentReadings_SensorId_Unique",
                table: "MostRecentReadings");

            // Drop FK to Sensors if it already exists (SQL Server syntax)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Sensors_SensorId')
                ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Sensors_SensorId];
            ");

            // Drop FK to Units if it already exists (SQL Server syntax)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MostRecentReadings_Units_UnitId')
                ALTER TABLE [MostRecentReadings] DROP CONSTRAINT [FK_MostRecentReadings_Units_UnitId];
            ");

            // Set SensorId as the new PK
            migrationBuilder.AddPrimaryKey(
                name: "PK_MostRecentReadings",
                table: "MostRecentReadings",
                column: "SensorId");

            // Ensure foreign key to Sensors
            migrationBuilder.AddForeignKey(
                name: "FK_MostRecentReadings_Sensors_SensorId",
                table: "MostRecentReadings",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Ensure foreign key to Units
            migrationBuilder.AddForeignKey(
                name: "FK_MostRecentReadings_Units_UnitId",
                table: "MostRecentReadings",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MostRecentReadings_Sensors_SensorId",
                table: "MostRecentReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_MostRecentReadings_Units_UnitId",
                table: "MostRecentReadings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MostRecentReadings",
                table: "MostRecentReadings");

            // Restore the old composite PK if needed
            migrationBuilder.AddPrimaryKey(
                name: "PK_MostRecentReadings",
                table: "MostRecentReadings",
                columns: new[] { "DateRecordedUtc", "SensorId" });

            // Restore the unique index if needed
            migrationBuilder.CreateIndex(
                name: "IX_MostRecentReadings_SensorId_Unique",
                table: "MostRecentReadings",
                column: "SensorId",
                unique: true);
        }
    }
}