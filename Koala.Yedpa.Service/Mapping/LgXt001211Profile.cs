using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping
{
    public class LgXt001211Profile : Profile
    {
        public LgXt001211Profile()
        {
            // LgXt001211 -> LgXt001211ListViewModel
            CreateMap<LgXt001211, LgXt001211ListViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LogRef, opt => opt.MapFrom(src => src.LogRef))
                .ForMember(dest => dest.ParLogRef, opt => opt.MapFrom(src => src.ParLogRef))
                .ForMember(dest => dest.GroupCode, opt => opt.MapFrom(src => src.GroupCode))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.ClientCode, opt => opt.MapFrom(src => src.ClientCode))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.ClientName))
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType))
                .ForMember(dest => dest.BegDate, opt => opt.MapFrom(src => src.BegDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.TotalBrutCoefficientMetre, opt => opt.MapFrom(src => src.TotalBrutCoefficientMetre))
                .ForMember(dest => dest.TotalNetMetre, opt => opt.MapFrom(src => src.TotalNetMetre))
                .ForMember(dest => dest.TotalFuelMetre, opt => opt.MapFrom(src => src.TotalFuelMetre))
                .ReverseMap();

            // LgXt001211 -> LgXt001211UpdateViewModel
            CreateMap<LgXt001211, LgXt001211UpdateViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.LogRef, opt => opt.MapFrom(src => src.LogRef))
                .ForMember(dest => dest.ParLogRef, opt => opt.MapFrom(src => src.ParLogRef))
                .ForMember(dest => dest.GroupCode, opt => opt.MapFrom(src => src.GroupCode))
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName))
                .ForMember(dest => dest.ClientCode, opt => opt.MapFrom(src => src.ClientCode))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.ClientName))
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType))
                .ForMember(dest => dest.PersonCount, opt => opt.MapFrom(src => src.PersonCount))
                .ForMember(dest => dest.ChiefReg, opt => opt.MapFrom(src => src.ChiefReg))
                .ForMember(dest => dest.TaxPayer, opt => opt.MapFrom(src => src.TaxPayer))
                .ForMember(dest => dest.IdentityNr, opt => opt.MapFrom(src => src.IdentityNr))
                .ForMember(dest => dest.DeedInfo, opt => opt.MapFrom(src => src.DeedInfo))
                .ForMember(dest => dest.ProfitingOwner, opt => opt.MapFrom(src => src.ProfitingOwner))
                .ForMember(dest => dest.BegDate, opt => opt.MapFrom(src => src.BegDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.OfficialBegDate, opt => opt.MapFrom(src => src.OfficialBegDate))
                .ForMember(dest => dest.OfficialEndDate, opt => opt.MapFrom(src => src.OfficialEndDate))
                .ForMember(dest => dest.GasCoefficient, opt => opt.MapFrom(src => src.GasCoefficient))
                .ForMember(dest => dest.ActiveResDate, opt => opt.MapFrom(src => src.ActiveResDate))
                .ForMember(dest => dest.BudgetDepotMetre1, opt => opt.MapFrom(src => src.BudgetDepotMetre1))
                .ForMember(dest => dest.BudgetDepotMetre2, opt => opt.MapFrom(src => src.BudgetDepotMetre2))
                .ForMember(dest => dest.BudgetGroundMetre, opt => opt.MapFrom(src => src.BudgetGroundMetre))
                .ForMember(dest => dest.BudgetHungMetre, opt => opt.MapFrom(src => src.BudgetHungMetre))
                .ForMember(dest => dest.BudgetFloorMetre, opt => opt.MapFrom(src => src.BudgetFloorMetre))
                .ForMember(dest => dest.BudgetPassageMetre1, opt => opt.MapFrom(src => src.BudgetPassageMetre1))
                .ForMember(dest => dest.BudgetPassageMetre2, opt => opt.MapFrom(src => src.BudgetPassageMetre2))
                .ForMember(dest => dest.BudgetDepotCoefficient1, opt => opt.MapFrom(src => src.BudgetDepotCoefficient1))
                .ForMember(dest => dest.BudgetDepotCoefficient2, opt => opt.MapFrom(src => src.BudgetDepotCoefficient2))
                .ForMember(dest => dest.BudgetGroundCoefficient, opt => opt.MapFrom(src => src.BudgetGroundCoefficient))
                .ForMember(dest => dest.BudgetHungCoefficient, opt => opt.MapFrom(src => src.BudgetHungCoefficient))
                .ForMember(dest => dest.BudgetFloorCoefficient, opt => opt.MapFrom(src => src.BudgetFloorCoefficient))
                .ForMember(dest => dest.BudgetPassageCoefficient1, opt => opt.MapFrom(src => src.BudgetPassageCoefficient1))
                .ForMember(dest => dest.BudgetPassageCoefficient2, opt => opt.MapFrom(src => src.BudgetPassageCoefficient2))
                .ForMember(dest => dest.FuelDepotMetre1, opt => opt.MapFrom(src => src.FuelDepotMetre1))
                .ForMember(dest => dest.FuelDepotMetre2, opt => opt.MapFrom(src => src.FuelDepotMetre2))
                .ForMember(dest => dest.FuelGroundMetre, opt => opt.MapFrom(src => src.FuelGroundMetre))
                .ForMember(dest => dest.FuelHungMetre, opt => opt.MapFrom(src => src.FuelHungMetre))
                .ForMember(dest => dest.FuelFloorMetre, opt => opt.MapFrom(src => src.FuelFloorMetre))
                .ForMember(dest => dest.FuelPassageMetre1, opt => opt.MapFrom(src => src.FuelPassageMetre1))
                .ForMember(dest => dest.FuelPassageMetre2, opt => opt.MapFrom(src => src.FuelPassageMetre2))
                .ForMember(dest => dest.FuelDepotCoefficient1, opt => opt.MapFrom(src => src.FuelDepotCoefficient1))
                .ForMember(dest => dest.FuelDepotCoefficient2, opt => opt.MapFrom(src => src.FuelDepotCoefficient2))
                .ForMember(dest => dest.FuelGroundCoefficient, opt => opt.MapFrom(src => src.FuelGroundCoefficient))
                .ForMember(dest => dest.FuelHungCoefficient, opt => opt.MapFrom(src => src.FuelHungCoefficient))
                .ForMember(dest => dest.FuelFloorCoefficient, opt => opt.MapFrom(src => src.FuelFloorCoefficient))
                .ForMember(dest => dest.FuelPassageCoefficient1, opt => opt.MapFrom(src => src.FuelPassageCoefficient1))
                .ForMember(dest => dest.FuelPassageCoefficient2, opt => opt.MapFrom(src => src.FuelPassageCoefficient2))
                .ForMember(dest => dest.TotalBrutCoefficientMetre, opt => opt.MapFrom(src => src.TotalBrutCoefficientMetre))
                .ForMember(dest => dest.TotalNetMetre, opt => opt.MapFrom(src => src.TotalNetMetre))
                .ForMember(dest => dest.TotalFuelMetre, opt => opt.MapFrom(src => src.TotalFuelMetre))
                .ReverseMap();
        }
    }
}

