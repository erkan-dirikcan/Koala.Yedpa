using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeBatchAndQRCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "QRCodeGeneratedDate",
                table: "Workplace",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QRCodeImagePath",
                table: "Workplace",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QRCodeNumber",
                table: "Workplace",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QRCodeBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SqlQuery = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    QrCodeYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QrCodePreCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QRCodeCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRCodeBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QRCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    PartnerNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QRCodeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QRImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FolderPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QrCodeYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastUpdateUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRCodes_QRCodeBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "QRCodeBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QRCodeBatches_CreateTime",
                table: "QRCodeBatches",
                column: "CreateTime");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodeBatches_QrCodeYear",
                table: "QRCodeBatches",
                column: "QrCodeYear");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodeBatches_Status",
                table: "QRCodeBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_BatchId",
                table: "QRCodes",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_PartnerNo",
                table: "QRCodes",
                column: "PartnerNo");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_QRCodeNumber",
                table: "QRCodes",
                column: "QRCodeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_QrCodeYear",
                table: "QRCodes",
                column: "QrCodeYear");

            migrationBuilder.CreateIndex(
                name: "IX_QRCodes_Status",
                table: "QRCodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UQ_QRCodes_PartnerNo_Year",
                table: "QRCodes",
                columns: new[] { "PartnerNo", "QrCodeYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QRCodes");

            migrationBuilder.DropTable(
                name: "QRCodeBatches");

            migrationBuilder.DropColumn(
                name: "QRCodeGeneratedDate",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "QRCodeImagePath",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "QRCodeNumber",
                table: "Workplace");
        }
    }
}
