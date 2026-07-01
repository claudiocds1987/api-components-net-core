using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductAttributeDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributeDefinitions_attributeDefinitionId",
                table: "ProductAttributeValues");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributeDefinitions_attributeDefinitionId",
                table: "ProductAttributeValues",
                column: "attributeDefinitionId",
                principalTable: "ProductAttributeDefinitions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributeDefinitions_attributeDefinitionId",
                table: "ProductAttributeValues");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAttributeValues_ProductAttributeDefinitions_attributeDefinitionId",
                table: "ProductAttributeValues",
                column: "attributeDefinitionId",
                principalTable: "ProductAttributeDefinitions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
