using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class _003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "TransactionType",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "TransactionType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "TransactionItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "Transaction",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DuesStatistics",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DivCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DivName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DocTrackingNr = table.Column<long>(type: "bigint", nullable: false),
                    ClientCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientRef = table.Column<long>(type: "bigint", nullable: false),
                    BudgetType = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    January = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    February = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    March = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    April = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    May = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    June = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    July = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    August = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    September = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    October = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    November = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    December = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuesStatistics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuesStatistics",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "TransactionType");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "TransactionType");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "TransactionItem");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Transaction");
        }
    }
}
