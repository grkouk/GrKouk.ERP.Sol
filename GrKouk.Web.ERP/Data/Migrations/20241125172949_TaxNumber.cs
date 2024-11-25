using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class TaxNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Transactors",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactors_TaxNumber",
                table: "Transactors",
                column: "TaxNumber",
                unique: true,
                filter: "[TaxNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactors_TaxNumber",
                table: "Transactors");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Transactors");
        }
    }
}
