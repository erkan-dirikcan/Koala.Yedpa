using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping
{
    public class ClaimsProfile:Profile
    {
        public ClaimsProfile()
        {
            CreateMap<Claims, CreateClaimsViewModel>().ReverseMap();
            CreateMap<Claims, UpdateClaimsViewModel>().ReverseMap();
            CreateMap<Claims, ClaimsListViewModel>()
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module.DisplayName))
                .ReverseMap();
            CreateMap<Claims, SearchClaimViewModel>();
            CreateMap<Claims, ClaimListForUserViewModels>().ReverseMap();
            CreateMap<Claims, ClaimListForRoleViewModels>().ForMember(dest=>dest.Name,opt=>opt.MapFrom(src=>$"{src.Module.Name}.{src.Name}"));
        }
    }
}
