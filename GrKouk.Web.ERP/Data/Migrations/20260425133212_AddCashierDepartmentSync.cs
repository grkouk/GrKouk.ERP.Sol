using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierDepartmentSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CashierDepartmentId",
                table: "SharedItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseBatchTracking",
                table: "SharedItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SharedCashierDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VatClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedByShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedCashierDepartments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedCashierDepartments_Code",
                table: "SharedCashierDepartments",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SharedCashierDepartments_ModifiedAt",
                table: "SharedCashierDepartments",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedCashierDepartments_ModifiedByShopId",
                table: "SharedCashierDepartments",
                column: "ModifiedByShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedCashierDepartments");

            migrationBuilder.DropColumn(
                name: "CashierDepartmentId",
                table: "SharedItems");

            migrationBuilder.DropColumn(
                name: "UseBatchTracking",
                table: "SharedItems");
        }
    }
}
