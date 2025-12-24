using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class BudgetRatioProfile : Profile
{
    public BudgetRatioProfile()
    {
        // Create ViewModel -> Entity
        CreateMap<CreateBudgetRatioViewModel, BudgetRatio>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => StatusEnum.Active))
            .ForMember(dest => dest.CreateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastUpdateTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreateUserId, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdateUserId, opt => opt.Ignore());

        // Update ViewModel -> Entity
        CreateMap<UpdateBudgetRatioViewModel, BudgetRatio>()
            .ForMember(dest => dest.CreateTime, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdateTime, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreateUserId, opt => opt.Ignore())
            .ForMember(dest => dest.LastUpdateUserId, opt => opt.Ignore());

        // Entity -> List ViewModel
        CreateMap<BudgetRatio, BudgetRatioListViewModel>();

        // Entity -> Detail ViewModel
        CreateMap<BudgetRatio, BudgetRatioDetailViewModel>();
    }
}

