using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncErrorFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SyncSaleDocument",
                table: "SyncSaleDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SyncBuyDocument",
                table: "SyncBuyDocument");

            migrationBuilder.RenameTable(
                name: "SyncSaleDocument",
                newName: "SyncSaleDocuments");

            migrationBuilder.RenameTable(
                name: "SyncBuyDocument",
                newName: "SyncBuyDocuments");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocument_TransDate",
                table: "SyncSaleDocuments",
                newName: "IX_SyncSaleDocuments_TransDate");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocument_RefNumber",
                table: "SyncSaleDocuments",
                newName: "IX_SyncSaleDocuments_RefNumber");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocument_ErpId",
                table: "SyncSaleDocuments",
                newName: "IX_SyncSaleDocuments_ErpId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocument_CustomerId",
                table: "SyncSaleDocuments",
                newName: "IX_SyncSaleDocuments_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocument_CompanyCode",
                table: "SyncSaleDocuments",
                newName: "IX_SyncSaleDocuments_CompanyCode");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocument_BusId",
                table: "SyncSaleDocuments",
                newName: "IX_SyncSaleDocuments_BusId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocument_TransDate",
                table: "SyncBuyDocuments",
                newName: "IX_SyncBuyDocuments_TransDate");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocument_SupplierId",
                table: "SyncBuyDocuments",
                newName: "IX_SyncBuyDocuments_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocument_RefNumber",
                table: "SyncBuyDocuments",
                newName: "IX_SyncBuyDocuments_RefNumber");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocument_ErpId",
                table: "SyncBuyDocuments",
                newName: "IX_SyncBuyDocuments_ErpId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocument_CompanyCode",
                table: "SyncBuyDocuments",
                newName: "IX_SyncBuyDocuments_CompanyCode");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocument_BusId",
                table: "SyncBuyDocuments",
                newName: "IX_SyncBuyDocuments_BusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SyncSaleDocuments",
                table: "SyncSaleDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SyncBuyDocuments",
                table: "SyncBuyDocuments",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SyncSaleDocuments",
                table: "SyncSaleDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SyncBuyDocuments",
                table: "SyncBuyDocuments");

            migrationBuilder.RenameTable(
                name: "SyncSaleDocuments",
                newName: "SyncSaleDocument");

            migrationBuilder.RenameTable(
                name: "SyncBuyDocuments",
                newName: "SyncBuyDocument");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocuments_TransDate",
                table: "SyncSaleDocument",
                newName: "IX_SyncSaleDocument_TransDate");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocuments_RefNumber",
                table: "SyncSaleDocument",
                newName: "IX_SyncSaleDocument_RefNumber");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocuments_ErpId",
                table: "SyncSaleDocument",
                newName: "IX_SyncSaleDocument_ErpId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocuments_CustomerId",
                table: "SyncSaleDocument",
                newName: "IX_SyncSaleDocument_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocuments_CompanyCode",
                table: "SyncSaleDocument",
                newName: "IX_SyncSaleDocument_CompanyCode");

            migrationBuilder.RenameIndex(
                name: "IX_SyncSaleDocuments_BusId",
                table: "SyncSaleDocument",
                newName: "IX_SyncSaleDocument_BusId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocuments_TransDate",
                table: "SyncBuyDocument",
                newName: "IX_SyncBuyDocument_TransDate");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocuments_SupplierId",
                table: "SyncBuyDocument",
                newName: "IX_SyncBuyDocument_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocuments_RefNumber",
                table: "SyncBuyDocument",
                newName: "IX_SyncBuyDocument_RefNumber");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocuments_ErpId",
                table: "SyncBuyDocument",
                newName: "IX_SyncBuyDocument_ErpId");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocuments_CompanyCode",
                table: "SyncBuyDocument",
                newName: "IX_SyncBuyDocument_CompanyCode");

            migrationBuilder.RenameIndex(
                name: "IX_SyncBuyDocuments_BusId",
                table: "SyncBuyDocument",
                newName: "IX_SyncBuyDocument_BusId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SyncSaleDocument",
                table: "SyncSaleDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SyncBuyDocument",
                table: "SyncBuyDocument",
                column: "Id");
        }
    }
}
