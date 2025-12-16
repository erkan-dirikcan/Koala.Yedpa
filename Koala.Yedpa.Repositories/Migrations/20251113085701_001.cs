using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Koala.Yedpa.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class _001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Desscription",
                table: "Settings",
                newName: "Description");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "TransactionItem",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "TransactionItem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Transaction",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Settings",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "Settings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "Settings",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "GeneratedIds",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "GeneratedIds",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "ExtendedPropertyValues",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "ExtendedPropertyValues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "ExtendedProperties",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "ExtendedProperties",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "EmailTemplate",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "EmailTemplate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "LG_XT001_211",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    LOGREF = table.Column<int>(type: "int", nullable: false),
                    PARLOGREF = table.Column<int>(type: "int", nullable: true),
                    CUSTOMER_TYPE = table.Column<short>(type: "smallint", nullable: true),
                    RESIDENCETYPEREF = table.Column<int>(type: "int", nullable: true),
                    RESIDENCEGROUPREF = table.Column<int>(type: "int", nullable: true),
                    PARCELREF = table.Column<int>(type: "int", nullable: true),
                    PHASEREF = table.Column<int>(type: "int", nullable: true),
                    CAULDRONREF = table.Column<int>(type: "int", nullable: true),
                    SHARENO = table.Column<int>(type: "int", nullable: true),
                    BEGDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    ENDDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    BLOCKREF = table.Column<int>(type: "int", nullable: true),
                    INDDIVNO = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    RESIDENCENO = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    DIMGROSS = table.Column<double>(type: "float", nullable: true),
                    DIMFIELD = table.Column<double>(type: "float", nullable: true),
                    PERSONCOUNT = table.Column<short>(type: "smallint", nullable: true),
                    WATERMETERNO = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    CALMETERNO = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    HOTWATERMETERNO = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    CHIEFREG = table.Column<short>(type: "smallint", nullable: true),
                    TAXPAYER = table.Column<short>(type: "smallint", nullable: true),
                    IDENTITYNR = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: true),
                    DEEDINFO = table.Column<short>(type: "smallint", nullable: true),
                    PROFITINGOWNER = table.Column<string>(type: "nvarchar(31)", maxLength: 31, nullable: true),
                    OFFICIALBEGDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    OFFICIALENDDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    GASCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    ACTIVERESDATE = table.Column<DateTime>(type: "datetime", nullable: true),
                    BUDGETDEPOTMETRE1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETDEPOTMETRE2 = table.Column<double>(type: "float", nullable: true),
                    BUDGETGROUNDMETRE = table.Column<double>(type: "float", nullable: true),
                    BUDGETHUNGMETRE = table.Column<double>(type: "float", nullable: true),
                    BUDGETFLOORMETRE = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGEMETRE1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGEMETRE2 = table.Column<double>(type: "float", nullable: true),
                    BUDGETDEPOTCOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETDEPOTCOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    BUDGETGROUNDCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    BUDGETHUNGCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    BUDGETFLOORCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGECOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    BUDGETPASSAGECOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTMETRE1 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTMETRE2 = table.Column<double>(type: "float", nullable: true),
                    FUELGROUNDMETRE = table.Column<double>(type: "float", nullable: true),
                    FUELHUNGMETRE = table.Column<double>(type: "float", nullable: true),
                    FUELFLOORMETRE = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGEMETRE1 = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGEMETRE2 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTCOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    FUELDEPOTCOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    FUELGROUNDCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    FUELHUNGCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    FUELFLOORCOEFFICIENT = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGECOEFFICIENT1 = table.Column<double>(type: "float", nullable: true),
                    FUELPASSAGECOEFFICIENT2 = table.Column<double>(type: "float", nullable: true),
                    TOTALBRUTCOEFFICIENTMETRE = table.Column<double>(type: "float", nullable: true),
                    TOTALNETMETRE = table.Column<double>(type: "float", nullable: true),
                    TOTALFUELMETRE = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LG_XT001_211", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LG_XT001_211");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Settings",
                newName: "Desscription");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "TransactionItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "TransactionItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Transaction",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "Settings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "Settings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateTime",
                table: "Settings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "GeneratedIds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "GeneratedIds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "ExtendedPropertyValues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "ExtendedPropertyValues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "ExtendedProperties",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "ExtendedProperties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdateTime",
                table: "EmailTemplate",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUserId",
                table: "EmailTemplate",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
