using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace ApiComponents.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationsToProductAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductAttributeDefinitions_name",
                table: "ProductAttributeDefinitions");

            migrationBuilder.AddColumn<string>(
                name: "validationsJson",
                table: "ProductAttributeDefinitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeDefinitions_name_categoryId",
                table: "ProductAttributeDefinitions",
                columns: new[] { "name", "categoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductAttributeDefinitions_name_categoryId",
                table: "ProductAttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "validationsJson",
                table: "ProductAttributeDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeDefinitions_name",
                table: "ProductAttributeDefinitions",
                column: "name",
                unique: true);
        }
    }
}
