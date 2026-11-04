using AutoMapper;
using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Mappings
{
    public class MappingProductProfile : Profile
    {
        public MappingProductProfile()
        {
            // 1. Mapeo para Tag
            CreateMap<ProductTag, ProductTagDto>();

            // 2. Mapeo seguro de Review → DTO público (sin UserEmail)
            CreateMap<ProductReview, ProductReviewDto>();

            // 3. MAPEO PARA EL LISTADO GENERAL (ProductDto)
            //    rating ya es decimal en Product → decimal en ProductDto (sin pérdida de precisión)
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.categoryId, opt => opt.MapFrom(src => src.categoryId))
                .ForMember(dest => dest.brandId, opt => opt.MapFrom(src => src.brandId))
                .ForMember(dest => dest.tags, opt => opt.MapFrom(src => src.tags));

            // 4. MAPEO PARA EL DETALLE COMPLETO (ProductResponseDto)
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.extraAttributes, opt => opt.MapFrom(src => src.extraAttributeValues));
        }
    }
}
