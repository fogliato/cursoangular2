using System.Linq;
using AutoMapper;
using ProAgil.Domain;
using ProAgil.WebApi.Dtos;
namespace ProAgil.WebApi.Mapper
{
    public class MaperProfile : Profile
    {
        public MaperProfile()
        {
            CreateMap<Evento, EventoDto>()
                .ForMember(dto => dto.Palestrantes, opt =>
                {
                    opt.MapFrom(src => src.PalestrantesEventos.Select(x => x.Palestrante).ToList());
                }).ReverseMap();
            CreateMap<Palestrante, PalestranteDto>()
                .ForMember(dto => dto.Eventos, opt =>
                {
                    opt.MapFrom(src => src.PalestrantesEventos.Select(x => x.Evento).ToList());
                }).ReverseMap();
            CreateMap<Lote, LoteDto>().ReverseMap();
            CreateMap<RedeSocial, RedeSocialDto>().ReverseMap();
        }

    }
}