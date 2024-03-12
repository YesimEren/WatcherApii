using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatcherApi.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Adminss",
                table: "Adminss");

            migrationBuilder.RenameTable(
                name: "Adminss",
                newName: "Admins");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Admins",
                table: "Admins",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Admins",
                table: "Admins");

            migrationBuilder.RenameTable(
                name: "Admins",
                newName: "Adminss");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Adminss",
                table: "Adminss",
                column: "Id");
        }
    }
}
