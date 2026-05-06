using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using AutoMapper;

namespace ApiComponents.Services
{
    // Usamos el constructor primario para inyectar el repositorio y el mapper
    public class ProductExtraAttributeService(IProductAttributeRepository repo, IMapper mapper) : IProductExtraAttributeService
    {
        public async Task<IEnumerable<ProductExtraAttributesDto>> GetExtraAttributesByCategoryId(int categoryId)
        {
            // 1. Obtenemos las entidades puras desde el repositorio
            var entities = await repo.GetExtraAttributesByCategoryId(categoryId);

            // 2. Usamos AutoMapper para convertir la colección de Entidades a DTOs
            // Esto aplicará automáticamente la lógica del JSON que definimos en el MappingProfile
            return mapper.Map<IEnumerable<ProductExtraAttributesDto>>(entities);
        }

        // SaveExtraAttributes cumple la funcion de guardar y/o actualizar atributos extra de un producto
        public async Task SaveExtraAttributes(int categoryId, List<ProductExtraAttributesDto> attributesDto)
        {
            // 1. Obtenemos las definiciones actuales en la DB para comparar
            var existingEntities = await repo.GetExtraAttributesByCategoryId(categoryId);

            foreach (var dto in attributesDto)
            {
                // Buscamos si el atributo ya existe (ID > 0 y coincide)
                var existing = existingEntities.FirstOrDefault(e => e.id == dto.id && dto.id != 0);

                if (existing != null)
                {
                    // ACTUALIZACIÓN: Mapeamos los cambios al objeto rastreado por EF
                    mapper.Map(dto, existing);
                    repo.UpdateExtraAttributes(existing);
                }
                else
                {
                    // CREACIÓN: Mapeamos el DTO a una nueva entidad
                    var newEntity = mapper.Map<ProductExtraAttributeDefinition>(dto);
                    newEntity.categoryId = categoryId; // Forzamos la relación con la categoría

                    await repo.AddExtraAttributes(newEntity);
                }
            }

            // 2. Persistencia: Enviamos todos los cambios a la base de datos en una sola transacción
            await repo.SaveChangesAsync();
        }
    }
}