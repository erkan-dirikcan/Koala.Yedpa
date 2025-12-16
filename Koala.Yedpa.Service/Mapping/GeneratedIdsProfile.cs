using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class GeneratedIdsProfile:Profile
{
    public GeneratedIdsProfile()
    {
        CreateMap<GeneratedIds, CreateGeneratedIdsViewModel>().ReverseMap();
        CreateMap<GeneratedIds, UpdateGeneratedIdsViewModel>().ReverseMap();
        CreateMap<GeneratedIds, GeneratedIdsListViewModel>().ReverseMap();
        CreateMap<GeneratedIds, GetNextNumberViewModel>().ReverseMap();
    }
}