using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class ExtendedPropertyRecordValuesProfile:Profile
{
    public ExtendedPropertyRecordValuesProfile()
    {
        CreateMap<ExtendedPropertyRecordValues, ExtendedPropertyRecordValuesListViewModel>()
            .ForMember(dest => dest.ExtendedPropertyName, opt => opt.MapFrom(src => src.ExtendedProperty.Name))

            .ReverseMap();
        CreateMap<ExtendedPropertyRecordValues, CreateExtendedPropertyRecordValuesViewModel>()
            .ForMember(dest => dest.ExtendedPropertyName, opt => opt.MapFrom(src => src.ExtendedProperty.Name))
            .ReverseMap();

        CreateMap<UpdateExtendedPropertyRecordValuesViewModel, ExtendedPropertyRecordValues>().ReverseMap();
    }
    
}