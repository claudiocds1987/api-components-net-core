using AutoMapper;
using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Mappings
{
    public class MappingProductProfile : Profile
    {
        public MappingProductProfile()
        {
            // 1. Mapeo necesario para el objeto Tag completo
            CreateMap<ProductTag, ProductTagDto>();

            // 2. MAPEO PARA EL LISTADO GENERAL (ProductDto)
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.category, opt => opt.MapFrom(src => src.category != null ? src.category.name : "General"))
                .ForMember(dest => dest.brand, opt => opt.MapFrom(src => src.brand != null ? src.brand.name : "N/A"))
                // Ahora mapeamos la lista de entidades a la lista de DTOs de Tags
                .ForMember(dest => dest.tags, opt => opt.MapFrom(src => src.tags));

            // 3. MAPEO PARA EL DETALLE COMPLETO (ProductResponseDto)
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.extraAttributes, opt => opt.MapFrom(src => src.extraAttributeValues));
        }
    }
}

