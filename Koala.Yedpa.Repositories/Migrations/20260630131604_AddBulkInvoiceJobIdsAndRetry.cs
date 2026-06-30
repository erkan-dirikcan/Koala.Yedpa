using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBulkInvoiceJobIdsAndRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InfoJobId",
                schema: "dbo",
                table: "BulkInvoiceSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferJobId",
                schema: "dbo",
                table: "BulkInvoiceSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanRetry",
                schema: "dbo",
                table: "BulkInvoiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "dbo",
                table: "BulkInvoiceItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestError",
                schema: "dbo",
                table: "BulkInvoiceItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "dbo",
                table: "BulkInvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfoJobId",
                schema: "dbo",
                table: "BulkInvoiceSessions");

            migrationBuilder.DropColumn(
                name: "TransferJobId",
                schema: "dbo",
                table: "BulkInvoiceSessions");

            migrationBuilder.DropColumn(
                name: "CanRetry",
                schema: "dbo",
                table: "BulkInvoiceItems");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "dbo",
                table: "BulkInvoiceItems");

            migrationBuilder.DropColumn(
                name: "RestError",
                schema: "dbo",
                table: "BulkInvoiceItems");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "dbo",
                table: "BulkInvoiceItems");
        }
    }
}
