using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class SellDocumentTransAmounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TransDiscountAmount",
                table: "SellDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransExpensesAmount",
                table: "SellDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransFpaAmount",
                table: "SellDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransNetAmount",
                table: "SellDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransDiscountAmount",
                table: "SellDocuments");

            migrationBuilder.DropColumn(
                name: "TransExpensesAmount",
                table: "SellDocuments");

            migrationBuilder.DropColumn(
                name: "TransFpaAmount",
                table: "SellDocuments");

            migrationBuilder.DropColumn(
                name: "TransNetAmount",
                table: "SellDocuments");
        }
    }
}
