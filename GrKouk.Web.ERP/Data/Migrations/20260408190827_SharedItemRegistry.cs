using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SharedItemRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedItemCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedItemCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeType = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeasureUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VatClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemNature = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpcCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedMeasureUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedMeasureUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedVatClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedVatClasses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCategories_Code",
                table: "SharedItemCategories",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCategories_ModifiedAt",
                table: "SharedItemCategories",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCodes_Code",
                table: "SharedItemCodes",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCodes_ItemId",
                table: "SharedItemCodes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItems_Code",
                table: "SharedItems",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItems_ModifiedAt",
                table: "SharedItems",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItems_ModifiedByShopId",
                table: "SharedItems",
                column: "ModifiedByShopId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedMeasureUnits_Code",
                table: "SharedMeasureUnits",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedMeasureUnits_ModifiedAt",
                table: "SharedMeasureUnits",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedVatClasses_Code",
                table: "SharedVatClasses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedVatClasses_ModifiedAt",
                table: "SharedVatClasses",
                column: "ModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedItemCategories");

            migrationBuilder.DropTable(
                name: "SharedItemCodes");

            migrationBuilder.DropTable(
                name: "SharedItems");

            migrationBuilder.DropTable(
                name: "SharedMeasureUnits");

            migrationBuilder.DropTable(
                name: "SharedVatClasses");
        }
    }
}
