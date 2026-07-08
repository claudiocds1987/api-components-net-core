using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "ProductBrands",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_isActive",
                table: "Products",
                column: "isActive",
                filter: "[isActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_isActive",
                table: "ProductCategories",
                column: "isActive",
                filter: "[isActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBrands_isActive",
                table: "ProductBrands",
                column: "isActive",
                filter: "[isActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_isActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_isActive",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductBrands_isActive",
                table: "ProductBrands");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "ProductBrands");
        }
    }
}
