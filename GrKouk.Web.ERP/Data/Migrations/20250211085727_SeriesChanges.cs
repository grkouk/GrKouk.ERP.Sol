using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeriesChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransTransactorDocSeriesDefId",
                table: "SellDocSeriesDefs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransWarehouseDocSeriesDefId",
                table: "SellDocSeriesDefs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellDocSeriesDefs_TransTransactorDocSeriesDefId",
                table: "SellDocSeriesDefs",
                column: "TransTransactorDocSeriesDefId");

            migrationBuilder.CreateIndex(
                name: "IX_SellDocSeriesDefs_TransWarehouseDocSeriesDefId",
                table: "SellDocSeriesDefs",
                column: "TransWarehouseDocSeriesDefId");

            migrationBuilder.AddForeignKey(
                name: "FK_SellDocSeriesDefs_TransTransactorDocSeriesDefs_TransTransactorDocSeriesDefId",
                table: "SellDocSeriesDefs",
                column: "TransTransactorDocSeriesDefId",
                principalTable: "TransTransactorDocSeriesDefs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SellDocSeriesDefs_TransWarehouseDocSeriesDefs_TransWarehouseDocSeriesDefId",
                table: "SellDocSeriesDefs",
                column: "TransWarehouseDocSeriesDefId",
                principalTable: "TransWarehouseDocSeriesDefs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SellDocSeriesDefs_TransTransactorDocSeriesDefs_TransTransactorDocSeriesDefId",
                table: "SellDocSeriesDefs");

            migrationBuilder.DropForeignKey(
                name: "FK_SellDocSeriesDefs_TransWarehouseDocSeriesDefs_TransWarehouseDocSeriesDefId",
                table: "SellDocSeriesDefs");

            migrationBuilder.DropIndex(
                name: "IX_SellDocSeriesDefs_TransTransactorDocSeriesDefId",
                table: "SellDocSeriesDefs");

            migrationBuilder.DropIndex(
                name: "IX_SellDocSeriesDefs_TransWarehouseDocSeriesDefId",
                table: "SellDocSeriesDefs");

            migrationBuilder.DropColumn(
                name: "TransTransactorDocSeriesDefId",
                table: "SellDocSeriesDefs");

            migrationBuilder.DropColumn(
                name: "TransWarehouseDocSeriesDefId",
                table: "SellDocSeriesDefs");
        }
    }
}
