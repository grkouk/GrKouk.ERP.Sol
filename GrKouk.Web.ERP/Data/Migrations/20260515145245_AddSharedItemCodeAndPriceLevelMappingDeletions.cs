using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedItemCodeAndPriceLevelMappingDeletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedItemCodeDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedItemCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemCodeDeletions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedItemPriceLevelMappingDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedMappingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemPriceLevelMappingDeletions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCodeDeletions_DeletedItemCodeId",
                table: "SharedItemCodeDeletions",
                column: "DeletedItemCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCodeDeletions_ModifiedAt",
                table: "SharedItemCodeDeletions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCodeDeletions_ModifiedByShopId",
                table: "SharedItemCodeDeletions",
                column: "ModifiedByShopId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPriceLevelMappingDeletions_DeletedMappingId",
                table: "SharedItemPriceLevelMappingDeletions",
                column: "DeletedMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPriceLevelMappingDeletions_ModifiedAt",
                table: "SharedItemPriceLevelMappingDeletions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPriceLevelMappingDeletions_ModifiedByShopId",
                table: "SharedItemPriceLevelMappingDeletions",
                column: "ModifiedByShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedItemCodeDeletions");

            migrationBuilder.DropTable(
                name: "SharedItemPriceLevelMappingDeletions");
        }
    }
}
