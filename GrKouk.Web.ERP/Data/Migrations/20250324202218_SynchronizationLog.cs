using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrKouk.Web.ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class SynchronizationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SynchronizationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SyncSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynchronizationLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_EntityName_EntityId",
                table: "SynchronizationLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_SyncedAt",
                table: "SynchronizationLogs",
                column: "SyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SynchronizationLogs_SyncSessionId",
                table: "SynchronizationLogs",
                column: "SyncSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SynchronizationLogs");
        }
    }
}
