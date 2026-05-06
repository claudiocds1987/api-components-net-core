using AutoMapper;
using ApiComponents.DTOs;
using ApiComponents.Models;
using System.Text.Json;

namespace ApiComponents.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 1. Mapeo de ENTIDAD (DB) -> DTO (Angular)
            // Se usa cuando consultamos los atributos existentes.
            CreateMap<ProductExtraAttributeDefinition, ProductExtraAttributesDto>()
                .ForMember(dest => dest.label, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.validations, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.validationsJson)
                        ? new AttributeValidationsDto()
                        : JsonSerializer.Deserialize<AttributeValidationsDto>(src.validationsJson, (JsonSerializerOptions?)null)));

            // 2. Mapeo de DTO (Angular) -> ENTIDAD (DB)
            // Se usa cuando recibimos datos para guardar o actualizar.
            CreateMap<ProductExtraAttributesDto, ProductExtraAttributeDefinition>()
                .ForMember(dest => dest.validationsJson, opt => opt.MapFrom(src =>
                    JsonSerializer.Serialize(src.validations, (JsonSerializerOptions?)null)));
        }
    }
}