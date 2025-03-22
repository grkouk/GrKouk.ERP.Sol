using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class MaterialCategoriesAddCompany2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MaterialCategories_Code",
                table: "MaterialCategories",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialCategories_CompanyId",
                table: "MaterialCategories",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialCategories_Companies_CompanyId",
                table: "MaterialCategories",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialCategories_Companies_CompanyId",
                table: "MaterialCategories");

            migrationBuilder.DropIndex(
                name: "IX_MaterialCategories_Code",
                table: "MaterialCategories");

            migrationBuilder.DropIndex(
                name: "IX_MaterialCategories_CompanyId",
                table: "MaterialCategories");
        }
    }
}
