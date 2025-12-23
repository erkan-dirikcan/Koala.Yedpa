using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class DuesStatisticProfile : Profile
{
    public DuesStatisticProfile()
    {
       // Source ViewModel -> Entity mapping (Import için)
        CreateMap<SourceDuesDataViewModel, DuesStatistic>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.DivCode, opt => opt.MapFrom(src => src.DivCode))
            .ForMember(dest => dest.DivName, opt => opt.MapFrom(src => src.DivName))
            .ForMember(dest => dest.DocTrackingNr, opt => opt.MapFrom(src => src.DocTrackingNr))
            .ForMember(dest => dest.ClientCode, opt => opt.MapFrom(src => src.ClientCode))
            .ForMember(dest => dest.ClientRef, opt => opt.MapFrom(src => src.ClientRef))
            .ForMember(dest => dest.January, opt => opt.MapFrom(src => src.January))
            .ForMember(dest => dest.February, opt => opt.MapFrom(src => src.February))
            .ForMember(dest => dest.March, opt => opt.MapFrom(src => src.March))
            .ForMember(dest => dest.April, opt => opt.MapFrom(src => src.April))
            .ForMember(dest => dest.May, opt => opt.MapFrom(src => src.May))
            .ForMember(dest => dest.June, opt => opt.MapFrom(src => src.June))
            .ForMember(dest => dest.July, opt => opt.MapFrom(src => src.July))
            .ForMember(dest => dest.August, opt => opt.MapFrom(src => src.August))
            .ForMember(dest => dest.September, opt => opt.MapFrom(src => src.September))
            .ForMember(dest => dest.October, opt => opt.MapFrom(src => src.October))
            .ForMember(dest => dest.November, opt => opt.MapFrom(src => src.November))
            .ForMember(dest => dest.December, opt => opt.MapFrom(src => src.December))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.BudgetType, opt => opt.MapFrom(src => BuggetTypeEnum.Budget))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusEnum.Active))
            .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Year, opt => opt.Ignore()); // Year dışarıdan set edilecek

        // DTO -> ViewModel mappings
        CreateMap<DuesStatisticSummaryViewModel, DuesStatisticSummaryViewModel>();
        CreateMap<YearOverviewViewModel, YearOverviewViewModel>();
        CreateMap<MonthlySummaryViewModel, MonthlySummaryViewModel>();
    }
}