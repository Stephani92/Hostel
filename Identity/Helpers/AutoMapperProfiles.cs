using AutoMapper;
using Identity.Dtos;
using Identity.Reposi;

namespace Identity.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserLoginDto>().ReverseMap();
            CreateMap<JobsDto, JobsDto>().ReverseMap();
        }
    }
}