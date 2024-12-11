using Microsoft.EntityFrameworkCore.Migrations;

namespace URLShortener.Backend.Migrations
{
    public partial class testexp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, update existing records to have a valid expiration date
            migrationBuilder.Sql(
                "UPDATE ShortUrls " +
                "SET ExpirationDate = DATEADD(day, 1, CreatedAt) " +
                "WHERE ExpirationDate = '0001-01-01 00:00:00.000'");

            // Then modify the column to use GETUTCDATE() + 1 day as default
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "ShortUrls",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "DATEADD(day, 1, GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpirationDate",
                table: "ShortUrls",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "DATEADD(day, 1, GETUTCDATE())");
        }
    }
}