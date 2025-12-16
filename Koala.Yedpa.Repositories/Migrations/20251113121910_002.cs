using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class _002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CLIENTCODE",
                table: "LG_XT001_211",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CLIENTNAME",
                table: "LG_XT001_211",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GROUPCODE",
                table: "LG_XT001_211",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GROUPNAME",
                table: "LG_XT001_211",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CLIENTCODE",
                table: "LG_XT001_211");

            migrationBuilder.DropColumn(
                name: "CLIENTNAME",
                table: "LG_XT001_211");

            migrationBuilder.DropColumn(
                name: "GROUPCODE",
                table: "LG_XT001_211");

            migrationBuilder.DropColumn(
                name: "GROUPNAME",
                table: "LG_XT001_211");
        }
    }
}
