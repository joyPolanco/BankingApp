using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CAMBIANDONOMBREDECOLUMNA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Account",
                newName: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Account",
                newName: "ClientId");
        }
    }
}
