using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortener.Backend.Migrations
{
    /// <inheritdoc />
    public partial class addidddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ShortUrls",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ShortUrls");
        }
    }
}
