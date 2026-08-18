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
            // Name AYNEN aktarilir; modul adi ONEK OLARAK EKLENMEZ.
            //
            // Bu esleme eskiden $"{src.Module.Name}.{src.Name}" uretiyordu. Eski adlandirmada
            // (modul "Module_Management", izin "Create_Module") anlamliydi, ancak PermissionCatalog
            // ile birlikte izin adi modul onekini ZATEN iceriyor ("BudgetOrder.Calculate").
            // Sonuc "BudgetOrder.BudgetOrder.Calculate" oluyordu ve bu deger AddClaimToRole
            // ekranindaki <option value> alanina yaziliyordu. Etkisi yikiciydi:
            //   1) IsSelected karsilastirmasi hicbir zaman tutmuyor, rolun mevcut yetkileri
            //      ekranda secili gorunmuyordu,
            //   2) Kaydet'e basildiginda rolun 49 gecerli izni silinip yerine katalogda
            //      karsiligi olmayan 56 gecersiz izin yaziliyordu -> rol tamamen yetkisiz kaliyordu.
            // 18.08.2026 canlida bu sekilde yasandi. Buraya onek ekleme MANTIGI GERI GETIRILMEMELI.
            // Modul etiketi gerekiyorsa ModuleName alanindan veya IModuleService uzerinden alinir.
            CreateMap<Claims, ClaimListForRoleViewModels>()
                .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module.Name));
        }
    }
}
