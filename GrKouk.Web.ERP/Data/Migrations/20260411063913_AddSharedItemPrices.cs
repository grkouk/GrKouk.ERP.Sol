using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedItemPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "PaymentMethods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SharedItemPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BrutPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Markup = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsOverridden = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemPrices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPrices_ItemId",
                table: "SharedItemPrices",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPrices_ModifiedAt",
                table: "SharedItemPrices",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPrices_ModifiedByShopId",
                table: "SharedItemPrices",
                column: "ModifiedByShopId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPrices_PriceLevelId",
                table: "SharedItemPrices",
                column: "PriceLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedItemPrices");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "PaymentMethods");
        }
    }
}
