using Microsoft.EntityFrameworkCore.Migrations;

namespace GrKouk.Web.ERP.Data.Migrations
{
    public partial class addsectionintransactortypedef : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "TransTransactorDocTypeDefs",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "TransTransactorDocTypeDefs");
        }
    }
}
