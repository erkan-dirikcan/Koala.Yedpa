using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping
{
    public class ModuleProfile:Profile
    {
        public ModuleProfile()
        {
            CreateMap<Module, CreateModuleViewModel>().ReverseMap();
            CreateMap<Module, UpdateModuleViewModel>().ReverseMap();
            CreateMap<Module, ModuleListViewModel>().ReverseMap();
        }
    }
}
