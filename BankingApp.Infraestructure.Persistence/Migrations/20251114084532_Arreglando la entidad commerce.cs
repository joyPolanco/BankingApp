using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Arreglandolaentidadcommerce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Commerce");

            migrationBuilder.CreateTable(
                name: "CommerceUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommerceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommerceUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommerceUser_Commerce_CommerceId",
                        column: x => x.CommerceId,
                        principalTable: "Commerce",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommerceUser_CommerceId",
                table: "CommerceUser",
                column: "CommerceId");

            migrationBuilder.CreateIndex(
                name: "IX_CommerceUser_UserId_CommerceId",
                table: "CommerceUser",
                columns: new[] { "UserId", "CommerceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommerceUser");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Commerce",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
