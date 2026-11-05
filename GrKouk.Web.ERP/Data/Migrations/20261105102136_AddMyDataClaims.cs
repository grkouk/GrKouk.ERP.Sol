using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMyDataClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MyDataIssuerRules",
                columns: table => new
                {
                    IssuerVat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    By = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDataIssuerRules", x => x.IssuerVat);
                });

            migrationBuilder.CreateTable(
                name: "MyDataShopClaims",
                columns: table => new
                {
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Mark = table.Column<long>(type: "bigint", nullable: false),
                    ClaimKind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssuerVat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Series = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Aa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MyDataGross = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReconStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReconCauses = table.Column<int>(type: "int", nullable: false),
                    LocalBuyDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocalRefNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LocalGross = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LocalRatesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalContentModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ErpBuyDocId = table.Column<int>(type: "int", nullable: true),
                    AddressConflict = table.Column<bool>(type: "bit", nullable: false),
                    AcceptedNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AcceptedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDataShopClaims", x => new { x.CompanyCode, x.Mark });
                });

            migrationBuilder.CreateTable(
                name: "MyDataShopPublishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    PeriodFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyDataShopPublishes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MyDataShopClaims_CompanyCode_IssueDate",
                table: "MyDataShopClaims",
                columns: new[] { "CompanyCode", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MyDataShopPublishes_CompanyCode_PeriodFrom_PeriodTo",
                table: "MyDataShopPublishes",
                columns: new[] { "CompanyCode", "PeriodFrom", "PeriodTo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyDataIssuerRules");

            migrationBuilder.DropTable(
                name: "MyDataShopClaims");

            migrationBuilder.DropTable(
                name: "MyDataShopPublishes");
        }
    }
}
