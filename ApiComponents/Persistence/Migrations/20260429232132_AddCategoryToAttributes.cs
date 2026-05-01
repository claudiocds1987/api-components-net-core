using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "categoryId",
                table: "ProductAttributeDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "dataType",
                table: "ProductAttributeDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "categoryId",
                table: "ProductAttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "dataType",
                table: "ProductAttributeDefinitions");
        }
    }
}
