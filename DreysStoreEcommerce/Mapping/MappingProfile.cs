using AutoMapper;
using DreysStoreEcommerce.Models;
using DreysStoreEcommerce.Models.DTOs.ViewModels;
using DreysStoreEcommerce.Models.ViewModels;

namespace DreysStoreEcommerce.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product → ProductViewModel
            CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            // WishlistItem → WishlistItemViewModel
            CreateMap<WishlistItem, WishlistItemViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

            // Review → ReviewViewModel
            CreateMap<Review, ReviewViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));

            // Order → OrderViewModel
            CreateMap<Order, OrderViewModel>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.ApplicationUser.Email));

            // OrderItem → OrderItemViewModel
            CreateMap<OrderItem, OrderItemViewModel>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice));

            // CartItem → CartItemViewModel
            CreateMap<CartItem, CartItemViewModel>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));


            // ProductDetailsViewModel
            CreateMap<Product, ProductDetailsViewModel>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews)); // if Reviews included

            CreateMap<Review, ReviewViewModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
        }
    }
}
