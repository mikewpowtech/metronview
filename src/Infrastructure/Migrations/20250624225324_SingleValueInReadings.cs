using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SingleValueInReadings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value1",
                table: "Readings");

            migrationBuilder.RenameColumn(
                name: "Value2",
                table: "Readings",
                newName: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Readings",
                newName: "Value2");

            migrationBuilder.AddColumn<double>(
                name: "Value1",
                table: "Readings",
                type: "float",
                nullable: true);
        }
    }
}
