using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── 1. Cambiar precisión de rating: de decimal(18,2) → decimal(4,2) ───────
            // El campo ya existe. Usamos AlterColumn para ajustar la precisión y
            // agregar el DEFAULT 0 que garantiza que todos los productos existentes
            // sin reviews queden en 0.00 y no en NULL.
            migrationBuilder.AlterColumn<decimal>(
                name: "rating",
                table: "Products",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            // ─── 2. Agregar columna reviewCount ──────────────────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "reviewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // ─── 3. Inicializar datos históricos ─────────────────────────────────────────
            // Para productos que ya tienen reviews en la base de datos, recalculamos
            // rating (AVG) y reviewCount (COUNT) en un único UPDATE con subquery.
            // Esto evita dejar datos en 0 tras el despliegue.
            migrationBuilder.Sql(@"
                UPDATE p
                SET
                    p.rating      = ISNULL(stats.avg_rating, 0),
                    p.reviewCount = ISNULL(stats.review_count, 0)
                FROM Products p
                LEFT JOIN (
                    SELECT
                        productId,
                        CAST(AVG(CAST(rating AS decimal(10,4))) AS decimal(4,2)) AS avg_rating,
                        COUNT(*) AS review_count
                    FROM ProductReviews
                    GROUP BY productId
                ) AS stats ON p.id = stats.productId
                WHERE stats.productId IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir reviewCount
            migrationBuilder.DropColumn(
                name: "reviewCount",
                table: "Products");

            // Revertir rating a decimal(18,2) sin default
            migrationBuilder.AlterColumn<decimal>(
                name: "rating",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,2)",
                oldPrecision: 4,
                oldScale: 2,
                oldDefaultValue: 0m);
        }
    }
}
