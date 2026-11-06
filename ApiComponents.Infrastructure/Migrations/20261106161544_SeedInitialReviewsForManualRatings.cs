using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiComponents.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialReviewsForManualRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── 1. Sembrar sets realistas de reviews para productos de catálogo ────────────
            // Incluye productos con puntuación perfecta 5.00 (Pattern 0) para que funcionen
            // los filtros por 5 estrellas, junto con decimales realistas (4.80, 4.67, 4.50, etc.).
            migrationBuilder.Sql(@"
                ;WITH PatternDef AS (
                    -- Pattern 0: 4 reviews de 5 estrellas -> PROMEDIO EXACTO 5.00 ⭐ (para filtro de 5 estrellas)
                    SELECT 0 AS Pattern, 1 AS Seq, 5 AS Rating, N'Lucas Moreno' AS UserName, N'lucas.moreno@reviews.catalogo.local' AS UserEmail, N'Excelente calidad, superó mis expectativas. Muy conforme con la compra.' AS Comment, 12 AS DaysAgo UNION ALL
                    SELECT 0, 2, 5, N'Camila Rossi', N'camila.rossi@reviews.catalogo.local', N'Materiales resistentes, buen diseño y terminaciones prolijas.', 25 UNION ALL
                    SELECT 0, 3, 5, N'Matías Gómez', N'matias.gomez@reviews.catalogo.local', N'Todo en orden, entrega rápida y tal cual las imágenes.', 40 UNION ALL
                    SELECT 0, 4, 5, N'Valentina Torres', N'valen.torres@reviews.catalogo.local', N'Producto impecable, 100% recomendado.', 62 UNION ALL

                    -- Pattern 1: 4 reviews -> PROMEDIO 4.50 ⭐
                    SELECT 1, 1, 5, N'Joaquín Benítez', N'joaco.benitez@reviews.catalogo.local', N'Muy buen producto, buena relación precio-calidad. Llegó rapidísimo.', 8 UNION ALL
                    SELECT 1, 2, 4, N'Lucía Fernández', N'lucia.f@reviews.catalogo.local', N'Cumple perfectamente con lo descripto en la publicación. Lo recomiendo.', 19 UNION ALL
                    SELECT 1, 3, 5, N'Gonzalo Morales', N'gonza.morales@reviews.catalogo.local', N'Muy conforme con el producto, volvería a comprar sin dudas.', 35 UNION ALL
                    SELECT 1, 4, 4, N'Agustina Romero', N'agus.romero@reviews.catalogo.local', N'El producto funciona muy bien, el empaque vino impecable.', 50 UNION ALL

                    -- Pattern 2: 3 reviews -> PROMEDIO 4.33 ⭐
                    SELECT 2, 1, 4, N'Martín Silva', N'martin.silva@reviews.catalogo.local', N'Práctico y funcional, buena experiencia de compra.', 14 UNION ALL
                    SELECT 2, 2, 4, N'Federico Álvarez', N'fede.alvarez@reviews.catalogo.local', N'Buena compra, satisfecho con el rendimiento general.', 28 UNION ALL
                    SELECT 2, 3, 5, N'Mariana Castro', N'mari.castro@reviews.catalogo.local', N'Llegó antes de lo esperado y en perfectas condiciones.', 45 UNION ALL

                    -- Pattern 3: 3 reviews -> PROMEDIO 4.67 ⭐
                    SELECT 3, 1, 5, N'Santiago Díaz', N'santi.diaz@reviews.catalogo.local', N'Excelente compra, lo uso a diario y funciona de 10.', 5 UNION ALL
                    SELECT 3, 2, 5, N'Florencia Ruiz', N'flor.ruiz@reviews.catalogo.local', N'Gran producto, muy cómodo y fácil de usar.', 22 UNION ALL
                    SELECT 3, 3, 4, N'Nicolás Herrera', N'nico.herrera@reviews.catalogo.local', N'Buena terminación, se nota de buen material.', 39 UNION ALL

                    -- Pattern 4: 4 reviews -> PROMEDIO 4.25 ⭐
                    SELECT 4, 1, 4, N'Ignacio Medina', N'nacho.medina@reviews.catalogo.local', N'Buena calidad en general, acorde al precio pagado.', 10 UNION ALL
                    SELECT 4, 2, 4, N'Sofía Benítez', N'sofi.benitez@reviews.catalogo.local', N'Cumple perfectamente con lo descripto. Lo recomiendo.', 26 UNION ALL
                    SELECT 4, 3, 4, N'Tomás Peralta', N'tomi.peralta@reviews.catalogo.local', N'Práctico y funcional, buena experiencia de compra.', 48 UNION ALL
                    SELECT 4, 4, 5, N'Julieta Vega', N'juli.vega@reviews.catalogo.local', N'Excelente calidad, superó mis expectativas.', 70 UNION ALL

                    -- Pattern 5: 4 reviews -> PROMEDIO 3.75 ⭐
                    SELECT 5, 1, 4, N'Esteban Ríos', N'esteban.rios@reviews.catalogo.local', N'Buen producto, funciona según lo esperado.', 15 UNION ALL
                    SELECT 5, 2, 3, N'Daniela Ortiz', N'dani.ortiz@reviews.catalogo.local', N'Aceptable por el precio, cumple su función básica.', 31 UNION ALL
                    SELECT 5, 3, 4, N'Facundo Luna', N'facu.luna@reviews.catalogo.local', N'Buena compra, satisfecho con el rendimiento.', 55 UNION ALL
                    SELECT 5, 4, 4, N'Micaela Sosa', N'mica.sosa@reviews.catalogo.local', N'Todo en orden, entrega rápida y buen empaque.', 75 UNION ALL

                    -- Pattern 6: 5 reviews -> PROMEDIO 4.60 ⭐
                    SELECT 6, 1, 5, N'Ramiro Cabrera', N'ramiro.cabrera@reviews.catalogo.local', N'Muy conforme con el producto, volvería a comprar sin dudas.', 7 UNION ALL
                    SELECT 6, 2, 5, N'Victoria Ramos', N'vicky.ramos@reviews.catalogo.local', N'Materiales resistentes, buen diseño y terminaciones prolijas.', 18 UNION ALL
                    SELECT 6, 3, 4, N'Lautaro Flores', N'lauti.flores@reviews.catalogo.local', N'Muy buen producto, buena relación precio-calidad.', 34 UNION ALL
                    SELECT 6, 4, 5, N'Paula Giménez', N'paula.gimenez@reviews.catalogo.local', N'Llegó antes de lo esperado y en perfectas condiciones.', 52 UNION ALL
                    SELECT 6, 5, 4, N'Manuel Acosta', N'manu.acosta@reviews.catalogo.local', N'El producto funciona muy bien, el empaque vino impecable.', 68 UNION ALL

                    -- Pattern 7: 5 reviews -> PROMEDIO 4.40 ⭐
                    SELECT 7, 1, 4, N'Gastón Navarro', N'gaston.navarro@reviews.catalogo.local', N'Buena relación precio y calidad inmejorable.', 11 UNION ALL
                    SELECT 7, 2, 5, N'Milagros Suárez', N'mili.suarez@reviews.catalogo.local', N'Excelente calidad, superó mis expectativas.', 24 UNION ALL
                    SELECT 7, 3, 4, N'Bruno Domínguez', N'bruno.dominguez@reviews.catalogo.local', N'Buena terminación, se nota de buen material.', 42 UNION ALL
                    SELECT 7, 4, 5, N'Candela Blanco', N'cande.blanco@reviews.catalogo.local', N'Gran producto, muy cómodo y fácil de usar.', 58 UNION ALL
                    SELECT 7, 5, 4, N'Franco Carrizo', N'franco.carrizo@reviews.catalogo.local', N'Todo en orden, entrega rápida y tal cual las imágenes.', 73 UNION ALL

                    -- Pattern 8: 5 reviews -> PROMEDIO 4.20 ⭐
                    SELECT 8, 1, 4, N'Enzo Quiroga', N'enzo.quiroga@reviews.catalogo.local', N'Práctico y funcional, buena experiencia de compra.', 9 UNION ALL
                    SELECT 8, 2, 4, N'Brisa Maldonado', N'brisa.maldonado@reviews.catalogo.local', N'Cumple perfectamente con lo descripto en la publicación.', 21 UNION ALL
                    SELECT 8, 3, 4, N'Rodrigo Méndez', N'rodrigo.mendez@reviews.catalogo.local', N'Buena compra, satisfecho con el rendimiento general.', 38 UNION ALL
                    SELECT 8, 4, 4, N'Zoe Castillo', N'zoe.castillo@reviews.catalogo.local', N'El producto funciona muy bien, buen empaque.', 54 UNION ALL
                    SELECT 8, 5, 5, N'Ezequiel Paz', N'eze.paz@reviews.catalogo.local', N'Excelente compra, lo uso a diario y funciona de 10.', 69 UNION ALL

                    -- Pattern 9: 5 reviews -> PROMEDIO 4.80 ⭐
                    SELECT 9, 1, 5, N'Álvaro Figueroa', N'alvaro.f@reviews.catalogo.local', N'Excelente calidad, superó mis expectativas. Muy conforme.', 6 UNION ALL
                    SELECT 9, 2, 5, N'Sol Villalba', N'sol.villalba@reviews.catalogo.local', N'Muy buen producto, buena relación precio-calidad. Llegó rápido.', 17 UNION ALL
                    SELECT 9, 3, 5, N'Guido Bustos', N'guido.bustos@reviews.catalogo.local', N'Materiales resistentes, buen diseño y terminaciones prolijas.', 33 UNION ALL
                    SELECT 9, 4, 5, N'Delfina Cárdenas', N'delfi.cardenas@reviews.catalogo.local', N'Llegó antes de lo esperado y en perfectas condiciones.', 49 UNION ALL
                    SELECT 9, 5, 4, N'Simón Roldán', N'simon.roldan@reviews.catalogo.local', N'Buena calidad en general, acorde al precio pagado.', 65
                )
                INSERT INTO ProductReviews (productId, rating, comment, userName, userEmail, date)
                SELECT 
                    p.id,
                    pat.Rating,
                    pat.Comment,
                    pat.UserName,
                    pat.UserEmail,
                    DATEADD(day, -pat.DaysAgo, GETUTCDATE())
                FROM Products p
                CROSS JOIN PatternDef pat
                WHERE pat.Pattern = (p.id % 10)
                  AND NOT EXISTS (
                      SELECT 1 FROM ProductReviews pr WHERE pr.productId = p.id
                  );
            ");

            // ─── 2. Sincronizar rating y reviewCount en Products ────────────────────────────
            // Recalculamos rating (AVG) y reviewCount (COUNT) sobre todos los productos
            // que recibieron su set de reviews, garantizando coherencia matemática total.
            migrationBuilder.Sql(@"
                UPDATE p
                SET
                    p.rating      = ISNULL(stats.avg_rating, 0),
                    p.reviewCount = ISNULL(stats.review_count, 0)
                FROM Products p
                INNER JOIN (
                    SELECT 
                        productId,
                        CAST(AVG(CAST(rating AS decimal(10,4))) AS decimal(4,2)) AS avg_rating,
                        COUNT(*) AS review_count
                    FROM ProductReviews
                    GROUP BY productId
                ) AS stats ON p.id = stats.productId
                WHERE p.reviewCount = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ─── 1. Eliminar únicamente las reviews sembradas por esta migración ──────────
            migrationBuilder.Sql(@"
                DELETE FROM ProductReviews
                WHERE userEmail LIKE '%@reviews.catalogo.local';
            ");

            // ─── 2. Recalcular las estadísticas de Products a partir de reviews reales ─────
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
                ) AS stats ON p.id = stats.productId;
            ");
        }
    }
}
