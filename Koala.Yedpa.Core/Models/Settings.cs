using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class Settings : CommonProperties
    {
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Value { get; set; }
        public SettingValueTypeEnum SettingValueType { get; set; }
        public SettingsTypeEnum SettingType { get; set; }
    }
}
