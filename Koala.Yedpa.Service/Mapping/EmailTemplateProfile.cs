using System;
using System.Collections.Generic;
using System.Text;

namespace Koala.Yedpa.Service.Mapping
{
    public class EmailTemplateProfile:AutoMapper.Profile
    {
        public EmailTemplateProfile()
        {
            // Source ViewModel -> Entity mapping (Import için)
            CreateMap<Core.Models.ViewModels.EmailTemplateCreateViewModel, Core.Models.EmailTemplate>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();
            // DTO -> ViewModel mappings
            CreateMap<Core.Models.EmailTemplate, Core.Models.ViewModels.EmailTemplateListViewModel>().ReverseMap();
            CreateMap<Core.Models.EmailTemplate, Core.Models.ViewModels.EmailTemplateDetailViewModel>().ReverseMap();
        }
    }
}
