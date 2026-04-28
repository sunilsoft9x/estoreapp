using AutoMapper;
using MyEstore.DTOs;
using MyEstore.Models;

namespace MyEstore.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserModel, UserResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));

        CreateMap<ProductModel, ProductResponseDto>()
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.isInStock))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
    }
}
