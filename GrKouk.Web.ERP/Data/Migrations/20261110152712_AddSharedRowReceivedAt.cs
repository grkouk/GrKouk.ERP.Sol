using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedRowReceivedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedVatClasses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedMeasureUnits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedItemPrices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedItemFamilies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedItemErpMappings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedItemCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedItemBrands",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "SharedCashierDepartments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_SharedVatClasses_ReceivedAt",
                table: "SharedVatClasses",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedMeasureUnits_ReceivedAt",
                table: "SharedMeasureUnits",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItems_ReceivedAt",
                table: "SharedItems",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemPrices_ReceivedAt",
                table: "SharedItemPrices",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemFamilies_ReceivedAt",
                table: "SharedItemFamilies",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemErpMappings_ReceivedAt",
                table: "SharedItemErpMappings",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemCategories_ReceivedAt",
                table: "SharedItemCategories",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedItemBrands_ReceivedAt",
                table: "SharedItemBrands",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedCashierDepartments_ReceivedAt",
                table: "SharedCashierDepartments",
                column: "ReceivedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SharedVatClasses_ReceivedAt",
                table: "SharedVatClasses");

            migrationBuilder.DropIndex(
                name: "IX_SharedMeasureUnits_ReceivedAt",
                table: "SharedMeasureUnits");

            migrationBuilder.DropIndex(
                name: "IX_SharedItems_ReceivedAt",
                table: "SharedItems");

            migrationBuilder.DropIndex(
                name: "IX_SharedItemPrices_ReceivedAt",
                table: "SharedItemPrices");

            migrationBuilder.DropIndex(
                name: "IX_SharedItemFamilies_ReceivedAt",
                table: "SharedItemFamilies");

            migrationBuilder.DropIndex(
                name: "IX_SharedItemErpMappings_ReceivedAt",
                table: "SharedItemErpMappings");

            migrationBuilder.DropIndex(
                name: "IX_SharedItemCategories_ReceivedAt",
                table: "SharedItemCategories");

            migrationBuilder.DropIndex(
                name: "IX_SharedItemBrands_ReceivedAt",
                table: "SharedItemBrands");

            migrationBuilder.DropIndex(
                name: "IX_SharedCashierDepartments_ReceivedAt",
                table: "SharedCashierDepartments");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedVatClasses");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedMeasureUnits");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedItemPrices");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedItemFamilies");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedItemErpMappings");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedItemCategories");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedItemBrands");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "SharedCashierDepartments");
        }
    }
}
