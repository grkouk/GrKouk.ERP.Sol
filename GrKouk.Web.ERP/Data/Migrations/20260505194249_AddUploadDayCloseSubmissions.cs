using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadDayCloseSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedDayCloseSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ZNumber = table.Column<int>(type: "int", nullable: false),
                    ErpSellDocIdsCsv = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedDayCloseSubmissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDayCloseSubmissions_CompanyCode",
                table: "UploadedDayCloseSubmissions",
                column: "CompanyCode");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDayCloseSubmissions_SubmissionId",
                table: "UploadedDayCloseSubmissions",
                column: "SubmissionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedDayCloseSubmissions_ZNumber",
                table: "UploadedDayCloseSubmissions",
                column: "ZNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedDayCloseSubmissions");
        }
    }
}
