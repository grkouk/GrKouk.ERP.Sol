using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class SellDocLineUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountExpenses",
                table: "SellDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransUnitPrice",
                table: "SellDocLines",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "TransactionQuantity",
                table: "SellDocLines",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<float>(
                name: "TransactionUnitFactor",
                table: "SellDocLines",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "TransactionUnitId",
                table: "SellDocLines",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountExpenses",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransUnitPrice",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransactionQuantity",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransactionUnitFactor",
                table: "SellDocLines");

            migrationBuilder.DropColumn(
                name: "TransactionUnitId",
                table: "SellDocLines");
        }
    }
}
