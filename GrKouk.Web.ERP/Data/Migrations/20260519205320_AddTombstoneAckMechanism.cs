using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTombstoneAckMechanism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnownShops",
                columns: table => new
                {
                    ShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownShops", x => x.ShopId);
                });

            migrationBuilder.CreateTable(
                name: "SharedTombstoneAcks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TombstoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TombstoneType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShopId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedTombstoneAcks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnownShops_IsActive",
                table: "KnownShops",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KnownShops_LastSeenAt",
                table: "KnownShops",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_SharedTombstoneAcks_ShopId",
                table: "SharedTombstoneAcks",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedTombstoneAcks_TombstoneId_TombstoneType_ShopId",
                table: "SharedTombstoneAcks",
                columns: new[] { "TombstoneId", "TombstoneType", "ShopId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedTombstoneAcks_TombstoneType_TombstoneId",
                table: "SharedTombstoneAcks",
                columns: new[] { "TombstoneType", "TombstoneId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownShops");

            migrationBuilder.DropTable(
                name: "SharedTombstoneAcks");
        }
    }
}
