using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class BuyDocTransAmounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountExpenses",
                table: "BuyDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransDiscountAmount",
                table: "BuyDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransExpensesAmount",
                table: "BuyDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransFpaAmount",
                table: "BuyDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransNetAmount",
                table: "BuyDocuments",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountExpenses",
                table: "BuyDocuments");

            migrationBuilder.DropColumn(
                name: "TransDiscountAmount",
                table: "BuyDocuments");

            migrationBuilder.DropColumn(
                name: "TransExpensesAmount",
                table: "BuyDocuments");

            migrationBuilder.DropColumn(
                name: "TransFpaAmount",
                table: "BuyDocuments");

            migrationBuilder.DropColumn(
                name: "TransNetAmount",
                table: "BuyDocuments");
        }
    }
}
