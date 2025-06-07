using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitDbTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UnitTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitStatusID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaysBeforeNotReported = table.Column<int>(type: "int", nullable: true),
                    CustomFieldValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Units");
        }
    }
}
