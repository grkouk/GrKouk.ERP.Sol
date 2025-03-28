using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class UnitOfMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncUnitOfMeasurements",
                columns: table => new
                {
                    BusId = table.Column<int>(type: "int", nullable: false),
                    ErpId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceChecksum = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncUnitOfMeasurements", x => new { x.ErpId, x.BusId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncUnitOfMeasurements_BusId",
                table: "SyncUnitOfMeasurements",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncUnitOfMeasurements_ErpId",
                table: "SyncUnitOfMeasurements",
                column: "ErpId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncUnitOfMeasurements");
        }
    }
}
