using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodEntityUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentMethods",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PaymentMethods",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "PaymentMethods",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLastModified",
                table: "PaymentMethods",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Code",
                table: "PaymentMethods",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_Code",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "DateLastModified",
                table: "PaymentMethods");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
