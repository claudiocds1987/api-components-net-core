using AutoMapper;
using ApiComponents.DTOs;
using ApiComponents.Models;
using System.Text.Json;

namespace ApiComponents.Mappings
{
    public class MappingProductExtraAttributesProfile : Profile
    {
        public MappingProductExtraAttributesProfile()
        {
            // =========================================================================
            // 1. VIAJE DE IDA: DE LA BASE DE DATOS HACIA EL FRONTEND (Angular)
            // =========================================================================
            // Sirve para cuando consultamos datos de la DB y queremos mandárselos limpios a la web.
            CreateMap<ProductExtraAttributeDefinition, ProductExtraAttributesDto>()

                // Mapea el campo 'name' de la DB a la propiedad 'label' que espera Angular.
                .ForMember(dest => dest.label, opt => opt.MapFrom(src => src.name))

                // TRUCO: Como la DB guarda las validaciones como un texto plano (JSON), 
                // aquí lo "desarmamos" (Deserializamos) para convertirlo en un objeto de C# 
                // que Angular pueda entender y usar fácilmente en los formularios.
                .ForMember(dest => dest.validations, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.validationsJson)
                        ? new ExtraAttributeValidationsDto()
                        : JsonSerializer.Deserialize<ExtraAttributeValidationsDto>(src.validationsJson, (JsonSerializerOptions?)null)));


            // =========================================================================
            // 2. VIAJE DE VUELTA: DEL FRONTEND (Angular) HACIA LA BASE DE DATOS (DB)
            // =========================================================================
            // Sirve para cuando el usuario llena el formulario en Angular y le da a "Guardar".
            CreateMap<ProductExtraAttributesDto, ProductExtraAttributeDefinition>()

                // TRUCO INVERSO: Como Angular nos manda las validaciones estructuradas como un objeto,
                // pero nuestra base de datos solo acepta texto, aquí lo "empaquetamos" (Serializamos)
                // convirtiendo ese objeto en un texto plano JSON para poder guardarlo en la columna.
                .ForMember(dest => dest.validationsJson, opt => opt.MapFrom(src =>
                    JsonSerializer.Serialize(src.validations, (JsonSerializerOptions?)null)));
        }
    }
}