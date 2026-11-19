using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <summary>
    /// Expiry settings travel between shops: the item's expiry rule and shelf life, the
    /// category's "requires expiry" and shelf life, and the "estimated expiry" mark on transfer
    /// lines. Pure DDL. The item/category columns are nullable on purpose: null = no shop that
    /// knows the fields has pushed the row yet, so receiving shops keep their own values.
    /// </summary>
    public partial class AddSharedExpirySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpiryRequirement",
                table: "SharedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "SharedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresExpiry",
                table: "SharedItemCategories",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "SharedItemCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExpiryEstimated",
                table: "SharedStockTransferLines",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryRequirement",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "RequiresExpiry",
                table: "SharedItemCategories");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "SharedItemCategories");

            migrationBuilder.DropColumn(
                name: "IsExpiryEstimated",
                table: "SharedStockTransferLines");
        }
    }
}
