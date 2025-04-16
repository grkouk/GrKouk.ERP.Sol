using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncBuyDocumentsP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PayedAmount",
                table: "SyncSaleDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "SyncSaleDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PayedAmount",
                table: "SyncBuyDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "SyncBuyDocument",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayedAmount",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "SyncSaleDocument");

            migrationBuilder.DropColumn(
                name: "PayedAmount",
                table: "SyncBuyDocument");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "SyncBuyDocument");
        }
    }
}
