using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class BuyDocLineChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnitExpenses",
                table: "BuyDocLines",
                newName: "TransUnitPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountExpenses",
                table: "WarehouseTransactions",
                type: "decimal(18, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransExpensesAmount",
                table: "WarehouseTransactions",
                type: "decimal(18, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountExpenses",
                table: "BuyDocLines",
                type: "decimal(18, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "TransactionQuantity",
                table: "BuyDocLines",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<float>(
                name: "TransactionUnitFactor",
                table: "BuyDocLines",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "TransactionUnitId",
                table: "BuyDocLines",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountExpenses",
                table: "WarehouseTransactions");

            migrationBuilder.DropColumn(
                name: "TransExpensesAmount",
                table: "WarehouseTransactions");

            migrationBuilder.DropColumn(
                name: "AmountExpenses",
                table: "BuyDocLines");

            migrationBuilder.DropColumn(
                name: "TransactionQuantity",
                table: "BuyDocLines");

            migrationBuilder.DropColumn(
                name: "TransactionUnitFactor",
                table: "BuyDocLines");

            migrationBuilder.DropColumn(
                name: "TransactionUnitId",
                table: "BuyDocLines");

            migrationBuilder.RenameColumn(
                name: "TransUnitPrice",
                table: "BuyDocLines",
                newName: "UnitExpenses");
        }
    }
}
