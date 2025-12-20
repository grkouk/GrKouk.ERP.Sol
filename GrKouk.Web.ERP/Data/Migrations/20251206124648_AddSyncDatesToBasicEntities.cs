using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncDatesToBasicEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "MeasureUnits",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MeasureUnits",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "MeasureUnits",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MaterialCategories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "MaterialCategories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "FpaKategories",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FpaKategories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "FpaKategories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "MeasureUnits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MeasureUnits");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "MeasureUnits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MaterialCategories");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "MaterialCategories");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "FpaKategories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FpaKategories");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "FpaKategories");
        }
    }
}
