using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedStockTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedStockTransfers",
                columns: table => new
                {
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DestShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaterializedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedStockTransfers", x => x.TransferId);
                });

            migrationBuilder.CreateTable(
                name: "SharedStockTransferLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CarriedUnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedStockTransferLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedStockTransferLines_SharedStockTransfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "SharedStockTransfers",
                        principalColumn: "TransferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockTransferLines_TransferId",
                table: "SharedStockTransferLines",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockTransfers_DestShopId_Status",
                table: "SharedStockTransfers",
                columns: new[] { "DestShopId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockTransfers_SourceShopId",
                table: "SharedStockTransfers",
                column: "SourceShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedStockTransferLines");

            migrationBuilder.DropTable(
                name: "SharedStockTransfers");
        }
    }
}
