using AutoMapper;
using DSAGrind.Models.DTOs;
using DSAGrind.Models.Entities;

namespace DSAGrind.Auth.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UserProfile, UserProfileDto>();
        CreateMap<UserPreferences, UserPreferencesDto>();
        CreateMap<NotificationSettings, NotificationSettingsDto>();

        // Reverse mappings
        CreateMap<UserDto, User>();
        CreateMap<UserProfileDto, UserProfile>();
        CreateMap<UserPreferencesDto, UserPreferences>();
        CreateMap<NotificationSettingsDto, NotificationSettings>();

        // Registration mapping
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "user"))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}