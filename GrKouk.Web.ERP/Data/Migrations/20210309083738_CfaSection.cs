using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class CfaSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultCfaId",
                table: "TransTransactorDocTypeDefs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultCfaTransSeriesId",
                table: "TransTransactorDocSeriesDefs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CfAccountId",
                table: "TransactorTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultCfaId",
                table: "TransTransactorDocTypeDefs");

            migrationBuilder.DropColumn(
                name: "DefaultCfaTransSeriesId",
                table: "TransTransactorDocSeriesDefs");

            migrationBuilder.DropColumn(
                name: "CfAccountId",
                table: "TransactorTransactions");
        }
    }
}
