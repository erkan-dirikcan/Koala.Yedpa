using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class _005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuggetRatioId",
                schema: "dbo",
                table: "DuesStatistics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferStatus",
                schema: "dbo",
                table: "DuesStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuggetRatioId",
                schema: "dbo",
                table: "DuesStatistics");

            migrationBuilder.DropColumn(
                name: "TransferStatus",
                schema: "dbo",
                table: "DuesStatistics");
        }
    }
}
