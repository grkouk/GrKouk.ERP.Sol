using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedItemFamilyAndBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ItemBrandId",
                table: "SharedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemFamilyId",
                table: "SharedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SharedItemBrands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SharedItemFamilies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedItemFamilies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemBrands_Code",
                table: "SharedItemBrands",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemBrands_ModifiedAt",
                table: "SharedItemBrands",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemFamilies_Code",
                table: "SharedItemFamilies",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemFamilies_ModifiedAt",
                table: "SharedItemFamilies",
                column: "ModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedItemBrands");

            migrationBuilder.DropTable(
                name: "SharedItemFamilies");

            migrationBuilder.DropColumn(
                name: "ItemBrandId",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "ItemFamilyId",
                table: "SharedItems");
        }
    }
}
