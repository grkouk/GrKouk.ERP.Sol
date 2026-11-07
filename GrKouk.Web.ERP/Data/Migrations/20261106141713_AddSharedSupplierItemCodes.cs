using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedSupplierItemCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedSupplierItemCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierVat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatchKind = table.Column<int>(type: "int", nullable: false),
                    MatchKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityFactor = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    SupplierDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedSupplierItemCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedSupplierItemCodes_ReceivedAt",
                table: "SharedSupplierItemCodes",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedSupplierItemCodes_SupplierVat_MatchKind_MatchKey",
                table: "SharedSupplierItemCodes",
                columns: new[] { "SupplierVat", "MatchKind", "MatchKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedSupplierItemCodes");
        }
    }
}
