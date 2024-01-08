namespace SportData.Services.Mapper.Profiles;

using AutoMapper;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Models.Cache;
using SportData.Data.Models.Converters;
using SportData.Data.Models.OlympicGames;
using SportData.Data.Models.OlympicGames.Disciplines;

public class OlympicGamesProfile : Profile
{
    public OlympicGamesProfile()
    {
        this.CreateMap<CityCacheModel, City>().ReverseMap();

        this.CreateMap<DisciplineCacheModel, Data.Entities.OlympicGames.Discipline>().ReverseMap();

        this.CreateMap<EventCacheModel, Data.Entities.OlympicGames.Event>().ReverseMap();

        this.CreateMap<GameCacheModel, Data.Entities.OlympicGames.Game>().ReverseMap();

        this.CreateMap<NOCCacheModel, NOC>().ReverseMap();

        this.CreateMap<VenueCacheModel, Venue>().ReverseMap();

        this.CreateMap<MatchModel, TeamMatch<Basketball>>()
            .ForPath(x => x.Team1.Id, opt => opt.MapFrom(y => y.Team1.Id))
            .ForPath(x => x.Team1.Name, opt => opt.MapFrom(y => y.Team1.Name))
            .ForPath(x => x.Team1.NOC, opt => opt.MapFrom(y => y.Team1.NOC))
            .ForPath(x => x.Team1.MatchResult, opt => opt.MapFrom(y => y.Team1.MatchResult))
            .ForPath(x => x.Team1.Points, opt => opt.MapFrom(y => y.Team1.Points))
            .ForPath(x => x.Team2.Id, opt => opt.MapFrom(y => y.Team2.Id))
            .ForPath(x => x.Team2.Name, opt => opt.MapFrom(y => y.Team2.Name))
            .ForPath(x => x.Team2.NOC, opt => opt.MapFrom(y => y.Team2.NOC))
            .ForPath(x => x.Team2.MatchResult, opt => opt.MapFrom(y => y.Team2.MatchResult))
            .ForPath(x => x.Team2.Points, opt => opt.MapFrom(y => y.Team2.Points))
            .ReverseMap();

        this.CreateMap<MatchModel, TeamMatch<AlpineSkiing>>()
            .ForPath(x => x.Team1.Id, opt => opt.MapFrom(y => y.Team1.Id))
            .ForPath(x => x.Team1.Name, opt => opt.MapFrom(y => y.Team1.Name))
            .ForPath(x => x.Team1.NOC, opt => opt.MapFrom(y => y.Team1.NOC))
            .ForPath(x => x.Team1.MatchResult, opt => opt.MapFrom(y => y.Team1.MatchResult))
            .ForPath(x => x.Team1.Points, opt => opt.MapFrom(y => y.Team1.Points))
            .ForPath(x => x.Team2.Id, opt => opt.MapFrom(y => y.Team2.Id))
            .ForPath(x => x.Team2.Name, opt => opt.MapFrom(y => y.Team2.Name))
            .ForPath(x => x.Team2.NOC, opt => opt.MapFrom(y => y.Team2.NOC))
            .ForPath(x => x.Team2.MatchResult, opt => opt.MapFrom(y => y.Team2.MatchResult))
            .ForPath(x => x.Team2.Points, opt => opt.MapFrom(y => y.Team2.Points))
            .ReverseMap();

        this.CreateMap<MatchModel, TeamMatch<Archery>>()
            .ForPath(x => x.Team1.Id, opt => opt.MapFrom(y => y.Team1.Id))
            .ForPath(x => x.Team1.Name, opt => opt.MapFrom(y => y.Team1.Name))
            .ForPath(x => x.Team1.NOC, opt => opt.MapFrom(y => y.Team1.NOC))
            .ForPath(x => x.Team1.MatchResult, opt => opt.MapFrom(y => y.Team1.MatchResult))
            .ForPath(x => x.Team1.Points, opt => opt.MapFrom(y => y.Team1.Points))
            .ForPath(x => x.Team2.Id, opt => opt.MapFrom(y => y.Team2.Id))
            .ForPath(x => x.Team2.Name, opt => opt.MapFrom(y => y.Team2.Name))
            .ForPath(x => x.Team2.NOC, opt => opt.MapFrom(y => y.Team2.NOC))
            .ForPath(x => x.Team2.MatchResult, opt => opt.MapFrom(y => y.Team2.MatchResult))
            .ForPath(x => x.Team2.Points, opt => opt.MapFrom(y => y.Team2.Points))
            .ReverseMap();

        this.CreateMap<MatchModel, AthleteMatch<Archery>>()
            .ForPath(x => x.Athlete1.Id, opt => opt.MapFrom(y => y.Team1.Id))
            .ForPath(x => x.Athlete1.Name, opt => opt.MapFrom(y => y.Team1.Name))
            .ForPath(x => x.Athlete1.NOC, opt => opt.MapFrom(y => y.Team1.NOC))
            .ForPath(x => x.Athlete1.MatchResult, opt => opt.MapFrom(y => y.Team1.MatchResult))
            .ForPath(x => x.Athlete1.Points, opt => opt.MapFrom(y => y.Team1.Points))
            .ForPath(x => x.Athlete2.Id, opt => opt.MapFrom(y => y.Team2.Id))
            .ForPath(x => x.Athlete2.Name, opt => opt.MapFrom(y => y.Team2.Name))
            .ForPath(x => x.Athlete2.NOC, opt => opt.MapFrom(y => y.Team2.NOC))
            .ForPath(x => x.Athlete2.MatchResult, opt => opt.MapFrom(y => y.Team2.MatchResult))
            .ForPath(x => x.Athlete2.Points, opt => opt.MapFrom(y => y.Team2.Points))
            .ReverseMap();
    }
}