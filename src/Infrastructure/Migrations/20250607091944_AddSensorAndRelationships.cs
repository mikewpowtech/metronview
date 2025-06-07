using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSensorAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units");

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Channel = table.Column<byte>(type: "tinyint", nullable: false),
                    ChannelType = table.Column<byte>(type: "tinyint", nullable: true),
                    LowValue = table.Column<int>(type: "int", nullable: true),
                    HighValue = table.Column<int>(type: "int", nullable: true),
                    EngineeringUnits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sensors_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_CompanyID",
                table: "Sensors",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_UnitId",
                table: "Sensors",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "Id");
        }
    }
}
