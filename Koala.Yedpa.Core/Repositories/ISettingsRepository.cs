using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface ISettingsRepository
{
    Task<List<Settings>> GetSettings(SettingsTypeEnum type);
    Task<Settings?> GetSetting(string id);
    Task<Settings?> GetSettingByName(string name, SettingsTypeEnum type);
    Task<List<Settings>> GetSettingsByNames(IEnumerable<string> names, SettingsTypeEnum type);
    void UpdateSettings(Settings settings);
    Task AddSettingsAsync(List<Settings> settings);
    Task AddSettingsAsync(Settings setting);


}