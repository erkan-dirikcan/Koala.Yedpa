using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardWidgetPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardWidgetPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WidgetId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    GridX = table.Column<int>(type: "int", nullable: false),
                    GridY = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardWidgetPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardWidgetPreferences_UserId_WidgetId",
                table: "DashboardWidgetPreferences",
                columns: new[] { "UserId", "WidgetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardWidgetPreferences");
        }
    }
}
