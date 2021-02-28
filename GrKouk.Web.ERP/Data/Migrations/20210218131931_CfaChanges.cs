using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class CfaChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountDiscount",
                table: "CashFlowAccountTransactions");

            migrationBuilder.DropColumn(
                name: "AmountFpa",
                table: "CashFlowAccountTransactions");

            migrationBuilder.DropColumn(
                name: "AmountNet",
                table: "CashFlowAccountTransactions");

            migrationBuilder.DropColumn(
                name: "TransDiscountAmount",
                table: "CashFlowAccountTransactions");

            migrationBuilder.RenameColumn(
                name: "FinancialTransAction",
                table: "CashFlowTransactionDefs",
                newName: "CfaAction");

            migrationBuilder.RenameColumn(
                name: "TransNetAmount",
                table: "CashFlowAccountTransactions",
                newName: "TransAmount");

            migrationBuilder.RenameColumn(
                name: "TransFpaAmount",
                table: "CashFlowAccountTransactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "FinancialAction",
                table: "CashFlowAccountTransactions",
                newName: "CfaAction");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CfaAction",
                table: "CashFlowTransactionDefs",
                newName: "FinancialTransAction");

            migrationBuilder.RenameColumn(
                name: "TransAmount",
                table: "CashFlowAccountTransactions",
                newName: "TransNetAmount");

            migrationBuilder.RenameColumn(
                name: "CfaAction",
                table: "CashFlowAccountTransactions",
                newName: "FinancialAction");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "CashFlowAccountTransactions",
                newName: "TransFpaAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountDiscount",
                table: "CashFlowAccountTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountFpa",
                table: "CashFlowAccountTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountNet",
                table: "CashFlowAccountTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransDiscountAmount",
                table: "CashFlowAccountTransactions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
