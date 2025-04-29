using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncDocumentsPart2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BuyDocDefId",
                table: "SyncSaleDocument",
                newName: "SaleDocDefId");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "SyncSaleDocument",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaleDocDefName",
                table: "SyncSaleDocument",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyDocDefName",
                table: "SyncBuyDocument",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "SyncBuyDocument",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "SaleDocDefName",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "BuyDocDefName",
                table: "SyncBuyDocument");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "SyncBuyDocument");

            migrationBuilder.RenameColumn(
                name: "SaleDocDefId",
                table: "SyncSaleDocument",
                newName: "BuyDocDefId");
        }
    }
}
