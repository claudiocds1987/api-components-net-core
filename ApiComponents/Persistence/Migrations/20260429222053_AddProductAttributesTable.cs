using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAttributesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductAttributeDefinitions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeDefinitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProductAttributeValues",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productId = table.Column<int>(type: "int", nullable: false),
                    attributeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributeValues", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_ProductAttributeDefinitions_attributeDefinitionId",
                        column: x => x.attributeDefinitionId,
                        principalTable: "ProductAttributeDefinitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductAttributeValues_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeDefinitions_name",
                table: "ProductAttributeDefinitions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_attributeDefinitionId_value",
                table: "ProductAttributeValues",
                columns: new[] { "attributeDefinitionId", "value" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_productId_attributeDefinitionId",
                table: "ProductAttributeValues",
                columns: new[] { "productId", "attributeDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributeValues_value",
                table: "ProductAttributeValues",
                column: "value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAttributeValues");

            migrationBuilder.DropTable(
                name: "ProductAttributeDefinitions");
        }
    }
}
