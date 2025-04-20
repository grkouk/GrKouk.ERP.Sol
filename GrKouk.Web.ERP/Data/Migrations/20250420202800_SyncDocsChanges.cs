using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncDocsChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyDocDefId",
                table: "SyncSaleDocument",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SyncSaleDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "SyncSaleDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "SyncSaleDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BuyDocDefId",
                table: "SyncBuyDocument",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SyncBuyDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "SyncBuyDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "SyncBuyDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyDocDefId",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "BuyDocDefId",
                table: "SyncBuyDocument");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SyncBuyDocument");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "SyncBuyDocument");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "SyncBuyDocument");
        }
    }
}
