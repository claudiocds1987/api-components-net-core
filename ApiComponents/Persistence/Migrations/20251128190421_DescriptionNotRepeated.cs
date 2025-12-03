using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionNotRepeated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Position",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Country",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Position_description",
                table: "Position",
                column: "description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Country_description",
                table: "Country",
                column: "description",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Position_description",
                table: "Position");

            migrationBuilder.DropIndex(
                name: "IX_Country_description",
                table: "Country");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Position",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Country",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
