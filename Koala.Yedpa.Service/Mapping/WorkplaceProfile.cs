using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class WorkplaceProfile : Profile
{
    public WorkplaceProfile()
    {
        CreateMap<Workplace, WorkplaceListViewModel>()
            .ReverseMap();

        CreateMap<Workplace, WorkplaceDetailViewModel>()
            .ReverseMap();
    }
}
