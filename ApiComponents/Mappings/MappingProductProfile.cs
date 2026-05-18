using AutoMapper;
using ApiComponents.DTOs;
using ApiComponents.Models;

namespace ApiComponents.Mappings
{
    public class MappingProductProfile : Profile
    {
        public MappingProductProfile()
        {
            // 1. MAPEO PARA EL LISTADO GENERAL (ProductDto)
            // Transforma la entidad pesada en un DTO básico y liviano sin extra atributos.
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.category, opt => opt.MapFrom(src => src.category != null ? src.category.name : "General"))
                .ForMember(dest => dest.brand, opt => opt.MapFrom(src => src.brand != null ? src.brand.name : "N/A"))
                .ForMember(dest => dest.tags, opt => opt.MapFrom(src => src.tags != null ? src.tags.Select(t => t.tagName).ToList() : new List<string>()));

            // 2. MAPEO PARA EL DETALLE COMPLETO (ProductResponseDto)
            // Se usa cuando el usuario entra a ver un solo producto y necesitas sus imágenes, reviews y extra atributos.
            CreateMap<Product, ProductResponseDto>()
                // Conectamos la lista de la DB con la lista de DTOs planos del detalle
                .ForMember(dest => dest.extraAttributes, opt => opt.MapFrom(src => src.attributeValues));
        }
    }
}
