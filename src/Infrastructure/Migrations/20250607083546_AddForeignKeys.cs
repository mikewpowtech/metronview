using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CompanyDb_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CompanyDb");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyID",
                table: "Units",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_CompanyID",
                table: "Units",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Companies_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Companies_CompanyID",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_CompanyID",
                table: "Units");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyID",
                table: "Units",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyDb",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentCompanyId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDb", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CompanyDb_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId",
                principalTable: "CompanyDb",
                principalColumn: "Id");
        }
    }
}
