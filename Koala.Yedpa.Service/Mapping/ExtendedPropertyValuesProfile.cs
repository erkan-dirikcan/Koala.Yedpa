using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class ExtendedPropertyValuesProfile:Profile
{
    public ExtendedPropertyValuesProfile()
    {
        CreateMap<ExtendedPropertyValues, ExtendedPropertyValuesDetailViewModel>()
            .ForMember(dest => dest.ExtendedPropertyName, opt => opt.MapFrom(src => src.ExtendedProperty!.Name))
            .ReverseMap();
        CreateMap<ExtendedPropertyValues, CreateExtendedPropertyValuesViewModel>().ReverseMap();
        CreateMap<ExtendedPropertyValues, UpdateExtendedPropertyValuesViewModel>().ReverseMap();
        CreateMap<ExtendedPropertyValues, ExtendedPropertyValuesListViewModel>()
            .ForMember(dest => dest.ExtendedPropertyName, opt => opt.MapFrom(src => src.ExtendedProperty!.Name))
            .ReverseMap();
    }
}