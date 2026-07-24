using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedStockRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId",
                table: "SharedStockTransfers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestId",
                table: "SharedStockTransfers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SharedStockRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestingShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedStockRequests", x => x.RequestId);
                });

            migrationBuilder.CreateTable(
                name: "SharedStockRequestFulfillments",
                columns: table => new
                {
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FulfillingShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedStockRequestFulfillments", x => x.ClaimId);
                    table.ForeignKey(
                        name: "FK_SharedStockRequestFulfillments_SharedStockRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "SharedStockRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedStockRequestLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FulfilledQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CancelledQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedStockRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedStockRequestLines_SharedStockRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "SharedStockRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SharedStockRequestFulfillmentLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedStockRequestFulfillmentLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedStockRequestFulfillmentLines_SharedStockRequestFulfillments_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "SharedStockRequestFulfillments",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequestFulfillmentLines_ClaimId",
                table: "SharedStockRequestFulfillmentLines",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequestFulfillments_RequestId",
                table: "SharedStockRequestFulfillments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequestFulfillments_Status_CreatedAt",
                table: "SharedStockRequestFulfillments",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequestLines_RequestId",
                table: "SharedStockRequestLines",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequests_RequestingShopId_UpdatedAt",
                table: "SharedStockRequests",
                columns: new[] { "RequestingShopId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequests_Status",
                table: "SharedStockRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SharedStockRequests_UpdatedAt",
                table: "SharedStockRequests",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedStockRequestFulfillmentLines");

            migrationBuilder.DropTable(
                name: "SharedStockRequestLines");

            migrationBuilder.DropTable(
                name: "SharedStockRequestFulfillments");

            migrationBuilder.DropTable(
                name: "SharedStockRequests");

            migrationBuilder.DropColumn(
                name: "ClaimId",
                table: "SharedStockTransfers");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "SharedStockTransfers");
        }
    }
}
