using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAuditFieldsFromWorkplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "CreateUser",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "UpdateUser",
                table: "Workplace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "Workplace",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreateUser",
                table: "Workplace",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Workplace",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "Workplace",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdateUser",
                table: "Workplace",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
