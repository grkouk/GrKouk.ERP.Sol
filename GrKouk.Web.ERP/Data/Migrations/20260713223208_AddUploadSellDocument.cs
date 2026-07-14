using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadSellDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedSellDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalSellDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErpSellDocId = table.Column<int>(type: "int", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedSellDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedSellDocuments_CompanyCode",
                table: "UploadedSellDocuments",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedSellDocuments_ErpSellDocId",
                table: "UploadedSellDocuments",
                column: "ErpSellDocId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedSellDocuments_LocalSellDocumentId",
                table: "UploadedSellDocuments",
                column: "LocalSellDocumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedSellDocuments");
        }
    }
}
