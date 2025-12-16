using AutoMapper;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Service.Mapping;

public class SettingsProfile: Profile
{
    public SettingsProfile()
    {
        CreateMap<Settings, AddSettingViewModel>().ReverseMap();

    }

}