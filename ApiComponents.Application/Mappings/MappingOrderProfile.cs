using AutoMapper;
using ApiComponents.Application.DTOs;
using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para proyectar detalles de órdenes (OrderDetail) hacia los DTOs de compras.
/// </summary>
public class MappingOrderProfile : Profile
{
    public MappingOrderProfile()
    {
        CreateMap<OrderDetail, MyPurchaseItemDto>()
            .ForMember(dest => dest.orderId, opt => opt.MapFrom(src => src.orderId))
            .ForMember(dest => dest.productId, opt => opt.MapFrom(src => src.productId))
            .ForMember(dest => dest.productTitle, opt => opt.MapFrom(src =>
                src.product != null ? src.product.title : $"Producto #{src.productId}"))
            .ForMember(dest => dest.productImage, opt => opt.MapFrom(src => ResolveProductImage(src.product)))
            .ForMember(dest => dest.categoryId, opt => opt.MapFrom(src =>
                src.product != null ? (int?)src.product.categoryId : null))
            .ForMember(dest => dest.categoryName, opt => opt.MapFrom(src =>
                src.product != null && src.product.category != null ? src.product.category.name : null))
            .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.order.createdAt))
            .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.price))
            .ForMember(dest => dest.quantity, opt => opt.MapFrom(src => src.quantity));

        CreateMap<OrderDetail, AdminPurchasedItemDto>()
            .ForMember(dest => dest.orderId, opt => opt.MapFrom(src => src.orderId))
            .ForMember(dest => dest.productId, opt => opt.MapFrom(src => src.productId))
            .ForMember(dest => dest.productTitle, opt => opt.MapFrom(src =>
                src.product != null ? src.product.title : $"Producto #{src.productId}"))
            .ForMember(dest => dest.productImage, opt => opt.MapFrom(src => ResolveProductImage(src.product)))
            .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.order.createdAt))
            .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.price))
            .ForMember(dest => dest.quantity, opt => opt.MapFrom(src => src.quantity))
            .ForMember(dest => dest.status, opt => opt.MapFrom(src => src.order.status))
            .ForMember(dest => dest.userId, opt => opt.MapFrom(src => src.order.userId))
            .ForMember(dest => dest.customerName, opt => opt.MapFrom(src => src.order.customerName))
            .ForMember(dest => dest.customerEmail, opt => opt.MapFrom(src => src.order.customerEmail));
    }

    private static string ResolveProductImage(Product? product)
    {
        if (product == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(product.thumbnail)) return product.thumbnail;

        return product.images
            .OrderBy(i => i.id)
            .Select(i => i.imageUrl)
            .FirstOrDefault() ?? string.Empty;
    }
}
