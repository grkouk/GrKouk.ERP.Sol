using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErpFinancialAggregateDef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErpFinancialAggregateDefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseItemNature = table.Column<int>(type: "int", nullable: false),
                    FpaDefId = table.Column<int>(type: "int", nullable: false),
                    WarehouseItemId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DateLastModified = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpFinancialAggregateDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpFinancialAggregateDefs_FpaKategories_FpaDefId",
                        column: x => x.FpaDefId,
                        principalTable: "FpaKategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ErpFinancialAggregateDefs_WarehouseItems_WarehouseItemId",
                        column: x => x.WarehouseItemId,
                        principalTable: "WarehouseItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpFinancialAggregateDefs_FpaDefId",
                table: "ErpFinancialAggregateDefs",
                column: "FpaDefId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpFinancialAggregateDefs_WarehouseItemId",
                table: "ErpFinancialAggregateDefs",
                column: "WarehouseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpFinancialAggregateDefs_WarehouseItemNature_FpaDefId",
                table: "ErpFinancialAggregateDefs",
                columns: new[] { "WarehouseItemNature", "FpaDefId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpFinancialAggregateDefs");
        }
    }
}
