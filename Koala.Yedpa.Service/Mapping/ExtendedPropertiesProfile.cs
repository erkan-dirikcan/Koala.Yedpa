using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping
{
    public class ExtendedPropertiesProfile : Profile
    {
        public ExtendedPropertiesProfile()
        {
            CreateMap<ExtendedProperties, ExtendedPropertiesListViewModel>()
                .ForMember(dest => dest.ModeleName, opt => opt.MapFrom(src => src.Module.DisplayName))
                .ReverseMap();

            CreateMap<ExtendedProperties, ExtendedPropertiesDetailViewModel>().ReverseMap();
            
            CreateMap<ExtendedProperties, ExtendedPropertiesGetValueViewModel>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Values.FirstOrDefault().Value))
                .ReverseMap();
            CreateMap<ExtendedProperties, CreateExtendedPropertiesViewModel>().ReverseMap();
            
            CreateMap<ExtendedProperties, UpdateExtendedPropertiesViewModel>().ReverseMap();
            
            CreateMap<ExtendedProperties, ExtendedPropertyValuesViewModel>()
                .ForMember(dest => dest.ModuleName,opt=>opt.MapFrom(src =>src.Module.Name))
                .ForMember(dest=>dest.ValuesList,opt=>opt.MapFrom(x=>x.Values))
                .ReverseMap();

        }
    }
}
