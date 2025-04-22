using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncEntitiesPart1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncBuyDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    BuyDocDefId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    RefNumber = table.Column<int>(type: "int", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceChecksum = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncBuyDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SynchronizationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyncSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynchronizationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncItemFamilies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceChecksum = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncItemFamilies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncSaleDocument",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    BuyDocDefId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    TransDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RefNumber = table.Column<int>(type: "int", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceChecksum = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncSaleDocument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncSuppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    BusCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    SourceChecksum = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncSuppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncUnitOfMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<int>(type: "int", nullable: false),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceChecksum = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncUnitOfMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncBuyDocument_BusId",
                table: "SyncBuyDocument",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncBuyDocument_CompanyCode",
                table: "SyncBuyDocument",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_SyncBuyDocument_ErpId",
                table: "SyncBuyDocument",
                column: "ErpId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncBuyDocument_RefNumber",
                table: "SyncBuyDocument",
                column: "RefNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SyncBuyDocument_SupplierId",
                table: "SyncBuyDocument",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncBuyDocument_TransDate",
                table: "SyncBuyDocument",
                column: "TransDate");

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_CompanyCode",
                table: "SynchronizationLogs",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_EntityName_EntityId",
                table: "SynchronizationLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_SyncedAt",
                table: "SynchronizationLogs",
                column: "SyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_SyncSessionId",
                table: "SynchronizationLogs",
                column: "SyncSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncItemFamilies_BusId",
                table: "SyncItemFamilies",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncItemFamilies_CompanyCode",
                table: "SyncItemFamilies",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_SyncItemFamilies_ErpId",
                table: "SyncItemFamilies",
                column: "ErpId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncItemFamilies_ErpId_BusId",
                table: "SyncItemFamilies",
                columns: new[] { "ErpId", "BusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncSaleDocument_BusId",
                table: "SyncSaleDocument",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSaleDocument_CompanyCode",
                table: "SyncSaleDocument",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSaleDocument_CustomerId",
                table: "SyncSaleDocument",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSaleDocument_ErpId",
                table: "SyncSaleDocument",
                column: "ErpId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSaleDocument_RefNumber",
                table: "SyncSaleDocument",
                column: "RefNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSaleDocument_TransDate",
                table: "SyncSaleDocument",
                column: "TransDate");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSuppliers_BusCode",
                table: "SyncSuppliers",
                column: "BusCode");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSuppliers_BusId",
                table: "SyncSuppliers",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSuppliers_CompanyCode",
                table: "SyncSuppliers",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSuppliers_ErpId",
                table: "SyncSuppliers",
                column: "ErpId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncUnitOfMeasurements_BusId",
                table: "SyncUnitOfMeasurements",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncUnitOfMeasurements_CompanyCode",
                table: "SyncUnitOfMeasurements",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_SyncUnitOfMeasurements_ErpId",
                table: "SyncUnitOfMeasurements",
                column: "ErpId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncBuyDocument");

            migrationBuilder.DropTable(
                name: "SynchronizationLogs");

            migrationBuilder.DropTable(
                name: "SyncItemFamilies");

            migrationBuilder.DropTable(
                name: "SyncSaleDocument");

            migrationBuilder.DropTable(
                name: "SyncSuppliers");

            migrationBuilder.DropTable(
                name: "SyncUnitOfMeasurements");
        }
    }
}
