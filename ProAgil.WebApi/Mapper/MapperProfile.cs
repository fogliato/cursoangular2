using System.Linq;
using AutoMapper;
using ProAgil.Domain;
using ProAgil.Domain.Identity;
using ProAgil.WebApi.Dtos;

namespace ProAgil.WebApi.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Event, EventDto>()
                .ForMember(dto => dto.Speakers, opt =>
                {
                    opt.MapFrom(src => src.SpeakerEvents.Select(x => x.Speaker).ToList());
                }).ReverseMap();
            CreateMap<Speaker, SpeakerDto>()
                .ForMember(dto => dto.Events, opt =>
                {
                    opt.MapFrom(src => src.SpeakerEvents.Select(x => x.Event).ToList());
                }).ReverseMap();
            CreateMap<Batch, BatchDto>().ReverseMap();
            CreateMap<SocialNetwork, SocialNetworkDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserLoginDto>().ReverseMap();
        }
    }
}
