using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadBuyDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedBuyDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalBuyDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErpBuyDocId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedBuyDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedBuyDocuments_CompanyCode",
                table: "UploadedBuyDocuments",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedBuyDocuments_ErpBuyDocId",
                table: "UploadedBuyDocuments",
                column: "ErpBuyDocId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedBuyDocuments_LocalBuyDocumentId",
                table: "UploadedBuyDocuments",
                column: "LocalBuyDocumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedBuyDocuments");
        }
    }
}
