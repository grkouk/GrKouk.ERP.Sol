using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedItemErpMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedItemErpMappingDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemErpMappingDeletions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedItemErpMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemErpMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappingDeletions_LocalItemId",
                table: "SharedItemErpMappingDeletions",
                column: "LocalItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappingDeletions_ModifiedAt",
                table: "SharedItemErpMappingDeletions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappingDeletions_ModifiedByShopId",
                table: "SharedItemErpMappingDeletions",
                column: "ModifiedByShopId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappings_LocalItemId",
                table: "SharedItemErpMappings",
                column: "LocalItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappings_ModifiedAt",
                table: "SharedItemErpMappings",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappings_ModifiedByShopId",
                table: "SharedItemErpMappings",
                column: "ModifiedByShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedItemErpMappingDeletions");

            migrationBuilder.DropTable(
                name: "SharedItemErpMappings");
        }
    }
}
