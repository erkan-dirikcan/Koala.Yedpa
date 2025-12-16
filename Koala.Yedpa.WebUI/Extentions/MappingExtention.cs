using Koala.Yedpa.Service.Mapping;


namespace Koala.Yedpa.WebUI.Extentions;

public static class MappingExtention
{
    public static void AddMappingConfExt(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ClaimsProfile>();
            cfg.AddProfile<ExtendedPropertiesProfile>();
            cfg.AddProfile<ExtendedPropertyRecordValuesProfile>();
            cfg.AddProfile<ExtendedPropertyValuesProfile>();
            cfg.AddProfile<GeneratedIdsProfile>();
            cfg.AddProfile<ModuleProfile>();
            cfg.AddProfile<UserProfile>();
            cfg.AddProfile<SettingsProfile>();
            cfg.AddProfile<LgXt001211Profile>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
            //cfg.AddProfile<>();
        });
    }
}