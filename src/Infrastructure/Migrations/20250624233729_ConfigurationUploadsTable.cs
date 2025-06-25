using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationUploadsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Units_UnitId",
                table: "Sensors");

            migrationBuilder.CreateTable(
                name: "ConfigurationUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DateCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QueueingUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationUploads_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationUploads_UnitId",
                table: "ConfigurationUploads",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Units_UnitId",
                table: "Sensors",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_Units_UnitId",
                table: "Sensors");

            migrationBuilder.DropTable(
                name: "ConfigurationUploads");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_Units_UnitId",
                table: "Sensors",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
