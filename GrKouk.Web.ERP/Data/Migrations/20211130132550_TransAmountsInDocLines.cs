using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class TransAmountsInDocLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TransDiscountAmount",
                table: "SellDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransExpensesAmount",
                table: "SellDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransFpaAmount",
                table: "SellDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransNetAmount",
                table: "SellDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransDiscountAmount",
                table: "BuyDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransExpensesAmount",
                table: "BuyDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransFpaAmount",
                table: "BuyDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransNetAmount",
                table: "BuyDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransDiscountAmount",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransExpensesAmount",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransFpaAmount",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransNetAmount",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransDiscountAmount",
                table: "BuyDocLines");

            migrationBuilder.DropColumn(
                name: "TransExpensesAmount",
                table: "BuyDocLines");

            migrationBuilder.DropColumn(
                name: "TransFpaAmount",
                table: "BuyDocLines");

            migrationBuilder.DropColumn(
                name: "TransNetAmount",
                table: "BuyDocLines");
        }
    }
}
