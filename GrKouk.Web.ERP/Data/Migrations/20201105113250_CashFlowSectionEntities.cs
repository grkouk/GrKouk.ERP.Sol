using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class CashFlowSectionEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashFlowAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(maxLength: 20, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateLastModified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashFlowTransactionDefs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(maxLength: 20, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    FinancialTransAction = table.Column<int>(nullable: false),
                    DefaultDocSeriesId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowTransactionDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashFlowTransactionDefs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashFlowAccountCompanyMappings",
                columns: table => new
                {
                    CashFlowAccountId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowAccountCompanyMappings", x => new { x.CompanyId, x.CashFlowAccountId });
                    table.ForeignKey(
                        name: "FK_CashFlowAccountCompanyMappings_CashFlowAccounts_CashFlowAccountId",
                        column: x => x.CashFlowAccountId,
                        principalTable: "CashFlowAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountCompanyMappings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashFlowDocTypeDefs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(maxLength: 20, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    CashFlowTransactionDefId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    SectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowDocTypeDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashFlowDocTypeDefs_CashFlowTransactionDefs_CashFlowTransactionDefId",
                        column: x => x.CashFlowTransactionDefId,
                        principalTable: "CashFlowTransactionDefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowDocTypeDefs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashFlowDocSeriesDefs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(maxLength: 20, nullable: true),
                    Name = table.Column<string>(maxLength: 200, nullable: true),
                    Active = table.Column<bool>(nullable: false),
                    CashFlowDocTypeDefId = table.Column<int>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowDocSeriesDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashFlowDocSeriesDefs_CashFlowDocTypeDefs_CashFlowDocTypeDefId",
                        column: x => x.CashFlowDocTypeDefId,
                        principalTable: "CashFlowDocTypeDefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowDocSeriesDefs_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashFlowAccountTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransDate = table.Column<DateTime>(nullable: false),
                    DocumentSeriesId = table.Column<int>(nullable: false),
                    DocumentTypeId = table.Column<int>(nullable: false),
                    RefCode = table.Column<string>(nullable: true),
                    CashFlowAccountId = table.Column<int>(nullable: false),
                    SectionId = table.Column<int>(nullable: false),
                    CreatorId = table.Column<int>(nullable: false),
                    CreatorSectionId = table.Column<int>(nullable: false),
                    FiscalPeriodId = table.Column<int>(nullable: false),
                    FinancialAction = table.Column<int>(nullable: false),
                    AmountFpa = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    AmountNet = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    AmountDiscount = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    TransFpaAmount = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    TransNetAmount = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    TransDiscountAmount = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    Etiology = table.Column<string>(maxLength: 500, nullable: true),
                    CompanyId = table.Column<int>(nullable: false),
                    Timestamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlowAccountTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountTransactions_CashFlowAccounts_CashFlowAccountId",
                        column: x => x.CashFlowAccountId,
                        principalTable: "CashFlowAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountTransactions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountTransactions_CashFlowDocSeriesDefs_DocumentSeriesId",
                        column: x => x.DocumentSeriesId,
                        principalTable: "CashFlowDocSeriesDefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountTransactions_CashFlowDocTypeDefs_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "CashFlowDocTypeDefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountTransactions_FiscalPeriods_FiscalPeriodId",
                        column: x => x.FiscalPeriodId,
                        principalTable: "FiscalPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFlowAccountTransactions_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactorTransactions_CreatorSectionId",
                table: "TransactorTransactions",
                column: "CreatorSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountCompanyMappings_CashFlowAccountId",
                table: "CashFlowAccountCompanyMappings",
                column: "CashFlowAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccounts_Code",
                table: "CashFlowAccounts",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_CashFlowAccountId",
                table: "CashFlowAccountTransactions",
                column: "CashFlowAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_CompanyId",
                table: "CashFlowAccountTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_CreatorId",
                table: "CashFlowAccountTransactions",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_CreatorSectionId",
                table: "CashFlowAccountTransactions",
                column: "CreatorSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_DocumentSeriesId",
                table: "CashFlowAccountTransactions",
                column: "DocumentSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_DocumentTypeId",
                table: "CashFlowAccountTransactions",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_FiscalPeriodId",
                table: "CashFlowAccountTransactions",
                column: "FiscalPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_SectionId",
                table: "CashFlowAccountTransactions",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowAccountTransactions_TransDate",
                table: "CashFlowAccountTransactions",
                column: "TransDate");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowDocSeriesDefs_CashFlowDocTypeDefId",
                table: "CashFlowDocSeriesDefs",
                column: "CashFlowDocTypeDefId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowDocSeriesDefs_Code",
                table: "CashFlowDocSeriesDefs",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowDocSeriesDefs_CompanyId",
                table: "CashFlowDocSeriesDefs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowDocTypeDefs_CashFlowTransactionDefId",
                table: "CashFlowDocTypeDefs",
                column: "CashFlowTransactionDefId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowDocTypeDefs_Code",
                table: "CashFlowDocTypeDefs",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowDocTypeDefs_CompanyId",
                table: "CashFlowDocTypeDefs",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowTransactionDefs_Code",
                table: "CashFlowTransactionDefs",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlowTransactionDefs_CompanyId",
                table: "CashFlowTransactionDefs",
                column: "CompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashFlowAccountCompanyMappings");

            migrationBuilder.DropTable(
                name: "CashFlowAccountTransactions");

            migrationBuilder.DropTable(
                name: "CashFlowAccounts");

            migrationBuilder.DropTable(
                name: "CashFlowDocSeriesDefs");

            migrationBuilder.DropTable(
                name: "CashFlowDocTypeDefs");

            migrationBuilder.DropTable(
                name: "CashFlowTransactionDefs");

            migrationBuilder.DropIndex(
                name: "IX_TransactorTransactions_CreatorSectionId",
                table: "TransactorTransactions");
        }
    }
}
