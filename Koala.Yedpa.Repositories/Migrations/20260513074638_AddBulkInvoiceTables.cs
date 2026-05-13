using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkInvoiceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulkInvoiceSessions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkInvoiceSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BulkInvoiceItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    OrficheRef = table.Column<int>(type: "int", nullable: false),
                    Orflineref = table.Column<int>(type: "int", nullable: false),
                    ClientCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LogoInvoiceRef = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkInvoiceItems_BulkInvoiceSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "dbo",
                        principalTable: "BulkInvoiceSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BulkInvoiceItems_SessionId",
                schema: "dbo",
                table: "BulkInvoiceItems",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulkInvoiceItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BulkInvoiceSessions",
                schema: "dbo");
        }
    }
}
