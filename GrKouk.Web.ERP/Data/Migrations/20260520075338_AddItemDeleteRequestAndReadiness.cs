using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemDeleteRequestAndReadiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeleteRequested",
                table: "SharedItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteRequestedAt",
                table: "SharedItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteRequestedByShopId",
                table: "SharedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SharedItemDeleteReadinesses",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocalFkCount = table.Column<int>(type: "int", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemDeleteReadinesses", x => new { x.ItemId, x.ShopId });
                });

            migrationBuilder.CreateTable(
                name: "SharedItemDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemDeletions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemDeleteReadinesses_ItemId",
                table: "SharedItemDeleteReadinesses",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemDeletions_DeletedItemId",
                table: "SharedItemDeletions",
                column: "DeletedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemDeletions_ModifiedAt",
                table: "SharedItemDeletions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemDeletions_ModifiedByShopId",
                table: "SharedItemDeletions",
                column: "ModifiedByShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedItemDeleteReadinesses");

            migrationBuilder.DropTable(
                name: "SharedItemDeletions");

            migrationBuilder.DropColumn(
                name: "DeleteRequested",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "DeleteRequestedAt",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "DeleteRequestedByShopId",
                table: "SharedItems");
        }
    }
}
