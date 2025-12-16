using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Koala.Yedpa.Service.Services;

public class SettingsService(ISettingsRepository repository, IUnitOfWork<AppDbContext> unitOfWork, ICryptoService cryptoService, IMapper mapper)
    : ISettingsService
{
    private readonly IMapper _mapper = mapper;

    public async Task<ResponseDto> AddEmailSettings(List<AddSettingViewModel> model)
    {
        try
        {
            if (model == null || !model.Any())
            {
                return ResponseDto.Fail(400, "Herhangibir Ayar Bilgisi Girilmemiş",
                    "Eklenecek Herhangi Bir Ayar Bilgisi Bulunmuyor", true);
            }
            var entities = _mapper.Map<List<Settings>>(model);
            foreach (var entity in entities)
            {
                entity.SettingType = SettingsTypeEnum.EmailSettings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "E-Posta Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "E-Posta Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddLogoSettings(List<AddSettingViewModel> model)
    {
        try
        {
            if (model == null || !model.Any())
            {
                return ResponseDto.Fail(400, "Herhangibir Ayar Bilgisi Girilmemiş",
                    "Eklenecek Herhangi Bir Ayar Bilgisi Bulunmuyor", true);
            }
            var entities = _mapper.Map<List<Settings>>(model);
            foreach (var entity in entities)
            {
                entity.SettingType = SettingsTypeEnum.LogoUserSettings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Logo Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Logo Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddLogoSqlSettings(List<AddSettingViewModel> model)
    {
        try
        {
            if (model == null || !model.Any())
            {
                return ResponseDto.Fail(400, "Herhangibir Ayar Bilgisi Girilmemiş",
                    "Eklenecek Herhangi Bir Ayar Bilgisi Bulunmuyor", true);
            }
            var entities = _mapper.Map<List<Settings>>(model);
            foreach (var entity in entities)
            {
                entity.SettingType = SettingsTypeEnum.LogoSqlSettings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Logo SQL Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Logo SQL Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddLogoRestServiceSettings(List<AddSettingViewModel> model)
    {
        try
        {
            if (model == null || !model.Any())
            {
                return ResponseDto.Fail(400, "Herhangibir Ayar Bilgisi Girilmemiş",
                    "Eklenecek Herhangi Bir Ayar Bilgisi Bulunmuyor", true);
            }
            var entities = _mapper.Map<List<Settings>>(model);
            foreach (var entity in entities)
            {
                entity.SettingType = SettingsTypeEnum.LogoRestServiceSettings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Logo Rest Service Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Logo Rest Service Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }

    }

    public async Task<ResponseDto> UpdateEmailSettingsAsync(EmailSettingViewModel model)
    {
        try
        {
            var errors = new List<string>();

            foreach (var prop in model.GetType().GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // TÜM ALANLARI ŞİFRELE (boş string bile gelse)
                string encryptedValue = propertyValue;

                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (Exception cryptoEx)
                {
                    errors.Add($"'{propertyName}' şifrelenirken hata: {cryptoEx.Message}");
                    continue;
                }

                var entity = await repository.GetSettingByName(propertyName, SettingsTypeEnum.EmailSettings);
                if (entity == null)
                {
                    errors.Add($"'{propertyName}' isimli ayar bulunamadı.");
                    continue;
                }

                entity.Value = encryptedValue;
                repository.UpdateSettings(entity);
            }

            if (errors.Any())
            {
                return ResponseDto.Fail(400, "E-Posta Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();

            return ResponseDto.Success(200, "E-Posta Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "E-Posta Ayarları Güncellenirken Beklenmeyen Hata", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateLogoSettingsAsync(LogoSettingViewModel model)
    {

        try
        {
            var errors = new List<string>();

            foreach (var prop in model.GetType().GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // TÜM ALANLARI ŞİFRELE (boş string bile gelse)
                string encryptedValue = propertyValue;

                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (Exception cryptoEx)
                {
                    errors.Add($"'{propertyName}' şifrelenirken hata: {cryptoEx.Message}");
                    continue;
                }

                var entity = await repository.GetSettingByName(propertyName, SettingsTypeEnum.LogoUserSettings);
                if (entity == null)
                {
                    errors.Add($"'{propertyName}' isimli ayar bulunamadı.");
                    continue;
                }

                entity.Value = encryptedValue;
                repository.UpdateSettings(entity);
            }

            if (errors.Any())
            {
                return ResponseDto.Fail(400, "Logo Kullanıcı Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();

            return ResponseDto.Success(200, "Logo Kullanıcı Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "Logo Kullanıcı Ayarları Güncellenirken Beklenmeyen Hata", ex.Message, true);
        }

    }

    public async Task<ResponseDto> UpdateLogoSqlSettingsAsync(LogoSqlSettingViewModel model)
    {
        try
        {
            var errors = new List<string>();

            // Tüm LogoSqlSettings'leri bir kereye mahsus çekip dictionary'ye alalım
            var entities = await repository.GetSettings(SettingsTypeEnum.LogoSqlSettings);
            var entityDict = entities.ToDictionary(e => e.Name, e => e);

            foreach (var prop in typeof(LogoSqlSettingViewModel).GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // Boş string bile gelse şifrele (Logo boş kabul etmiyor olabilir)
                string encryptedValue;
                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (Exception cryptoEx)
                {
                    errors.Add($"'{propertyName}' şifrelenirken hata: {cryptoEx.Message}");
                    continue;
                    continue;
                }

                if (!entityDict.TryGetValue(propertyName, out var entity))
                {
                    errors.Add($"'{propertyName}' isimli ayar veritabanında bulunamadı.");
                    continue;
                }

                entity.Value = encryptedValue;
                repository.UpdateSettings(entity);
            }

            if (errors.Any())
            {
                return ResponseDto.Fail(400, "Logo SQL Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Logo SQL Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "Logo SQL Ayarları güncellenirken beklenmeyen hata", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateLogoRestServiceSettingsAsync(LogoRestServiceSettingViewModel model)
    {
        try
        {
            var errors = new List<string>();

            // Tüm RestService ayarlarını bir kere çekip dictionary yapalım
            var entities = await repository.GetSettings(SettingsTypeEnum.LogoRestServiceSettings);
            var entityDict = entities.ToDictionary(e => e.Name, e => e);

            foreach (var prop in typeof(LogoRestServiceSettingViewModel).GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // TÜM ALANLAR ŞİFRELENECEK (özellikle Token, Secret vs. varsa)
                string encryptedValue;
                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (Exception cryptoEx)
                {
                    errors.Add($"'{propertyName}' şifrelenirken hata: {cryptoEx.Message}");
                    continue;
                }

                if (!entityDict.TryGetValue(propertyName, out var entity))
                {
                    errors.Add($"'{propertyName}' isimli ayar veritabanında bulunamadı.");
                    continue;
                }

                entity.Value = encryptedValue;
                repository.UpdateSettings(entity);
            }

            if (errors.Any())
            {
                return ResponseDto.Fail(400, "Logo Rest Service Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Logo Rest Service Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "Logo Rest Service ayarları güncellenirken beklenmeyen hata", ex.Message, true);
        }
    }




    //public async Task<ResponseDto> UpdateLogoSqlSettingsAsync(LogoSqlSettingViewModel model)
    //{
    //    try
    //    {
    //        var errors = new List<string>();

    //        foreach (var prop in model.GetType().GetProperties())
    //        {
    //            var propertyName = prop.Name;
    //            var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

    //            // TÜM ALANLARI ŞİFRELE (boş bile olsa)
    //            string encryptedValue = propertyValue;
    //            if (!string.IsNullOrEmpty(propertyValue) || propertyValue == string.Empty) // boş string bile şifrelenecek
    //            {
    //                try
    //                {
    //                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
    //                }
    //                catch (Exception ex)
    //                {
    //                    errors.Add($"'{propertyName}' şifrelenirken hata: {ex.Message}");
    //                    continue;
    //                }
    //            }

    //            var entity = await repository.GetSettingByName(propertyName, SettingsTypeEnum.LogoSqlSettings);
    //            if (entity == null)
    //            {
    //                errors.Add($"'{propertyName}' isimli ayar bulunamadı.");
    //                continue;
    //            }

    //            entity.Value = encryptedValue;
    //            repository.UpdateSettings(entity);
    //        }

    //        if (errors.Any())
    //        {
    //            return ResponseDto.Fail(400, "Logo Sql Ayarları Güncellenirken Hata Oluştu", errors, true);
    //        }

    //        await unitOfWork.CommitAsync();

    //        return ResponseDto.Success(200, "Logo Sql Ayarları Başarıyla Güncellendi");
    //    }
    //    catch (Exception ex)
    //    {
    //        return ResponseDto.Fail(500, "Logo Sql Ayarları Güncellenirken Beklenmeyen Hata", ex.Message, true);
    //    }
    //}

    //public async Task<ResponseDto> UpdateLogoRestServiceSettingsAsync(LogoRestServiceSettingViewModel model)
    //{
    //    try
    //    {
    //        var errors = new List<string>();
    //        foreach (var item in model.GetType().GetProperties())
    //        {
    //            var lineName = item.Name;
    //            var lineVal = model.GetType().GetProperty(lineName)?.GetValue(model, null);
    //            var entity = await repository.GetSettingByName(lineName, SettingsTypeEnum.LogoRestServiceSettings);
    //            if (entity == null)
    //            {
    //                errors.Add($"'{lineName}' isimli ayar bulunamadı.");
    //                continue;
    //            }
    //            entity.Value = lineVal?.ToString() ?? string.Empty;
    //            repository.UpdateSettings(entity);
    //        }

    //        if (errors.Any())
    //        {
    //            return ResponseDto.Fail(400, "Logo Rest Service Ayarları Güncellenirken Bir Sorunla Karşılaşıldı", errors, true);
    //        }
    //        await unitOfWork.CommitAsync();
    //        return ResponseDto.Success(200, "Logo Rest Service Ayarları Ayarları Başarıyla Güncellendi");
    //    }
    //    catch (Exception ex)
    //    {
    //        return ResponseDto.Fail(400, "Logo Rest Service Ayarları Ayarları Güncellenirken Bir Sorunla Karşılaşıldı", ex.Message, true);
    //    }
    //}

    public async Task<ResponseDto<EmailSettingViewModel>> GetEmailSettingsAsync()
    {
        var res = await repository.GetSettings(SettingsTypeEnum.EmailSettings);
        if (res == null || res.Count < 1)
        {
            return ResponseDto<EmailSettingViewModel>.FailData(404, "E-Posta Ayarları Henüz Yapılandırılmamış", "Email Ayarları Henüz Yapılandırılmamış", true);
        }

        var retVal = new EmailSettingViewModel();

        // foreach (var cryptoService = HttpContext.RequestServices.GetRequiredService<ICryptoService>(); // veya constructor'dan inject et

        foreach (var item in res)
        {
            string decryptedValue = item.Value;

            // TÜM ALANLARI ÇÖZ (boş değilse)
            if (!string.IsNullOrEmpty(item.Value))
            {
                try
                {
                    decryptedValue = await cryptoService.DecryptAsync(item.Value);
                }
                catch
                {
                    decryptedValue = "****** (Çözülemedi)"; // hata olursa gizle
                }
            }

            switch (item.Name)
            {
                case "SmtpServer":
                    retVal.SmtpServer = decryptedValue;
                    break;
                case "UserName":
                    retVal.UserName = decryptedValue;
                    break;
                case "Password":
                    retVal.Password = decryptedValue;
                    break;
                case "Port":
                    if (int.TryParse(decryptedValue, out var port))
                        retVal.Port = port;
                    break;
                case "SenderEmail":
                    retVal.SenderEmail = decryptedValue;
                    break;
                case "SenderName":
                    retVal.SenderName = decryptedValue;
                    break;
                case "EnableSsl":
                    if (bool.TryParse(decryptedValue, out var enableSsl))
                        retVal.EnableSsl = enableSsl;
                    break;
            }
        }

        return ResponseDto<EmailSettingViewModel>.SuccessData(200, "E-Posta Ayarları Başarıyla Getirildi", retVal);
    }

    public async Task<ResponseDto<LogoSqlSettingViewModel>> GetLogoSqlSettingsAsync()
    {
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.LogoSqlSettings);

            if (entities == null || !entities.Any())
            {
                return ResponseDto<LogoSqlSettingViewModel>.FailData(
                    404,
                    "Logo SQL Ayarları Henüz Yapılandırılmamış",
                    "Logo SQL Ayarları Henüz Yapılandırılmamış",
                    true);
            }

            var model = new LogoSqlSettingViewModel();
            var entityDict = entities.ToDictionary(e => e.Name, e => e.Value);

            foreach (var prop in typeof(LogoSqlSettingViewModel).GetProperties())
            {
                var propName = prop.Name;

                if (!entityDict.TryGetValue(propName, out var encryptedValue) || string.IsNullOrEmpty(encryptedValue))
                {
                    prop.SetValue(model, prop.PropertyType == typeof(string) ? "" : null);
                    continue;
                }

                string decryptedValue;
                try
                {
                    decryptedValue = await cryptoService.DecryptAsync(encryptedValue);
                }
                catch (Exception)
                {
                    // Log atabilirsin: _logger.LogError($"LogoSql {propName} decrypt edilemedi");
                    decryptedValue = ""; // asla şifreli veri dönmesin
                }

                // Port gibi özel tipler varsa burada dönüştür ama bu modelde yok gibi
                prop.SetValue(model, decryptedValue);
            }

            return ResponseDto<LogoSqlSettingViewModel>.SuccessData(200, "Logo SQL Ayarları Başarıyla Getirildi",
                model);
        }
        catch (Exception ex)
        {
            {
                return ResponseDto<LogoSqlSettingViewModel>.FailData(500, "Logo SQL ayarları getirilirken hata oluştu",
                    ex.Message, true);
            }
        }
    }

    public async Task<ResponseDto<LogoRestServiceSettingViewModel>> GetLogoRestServiceSettingsAsync()
    {
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.LogoRestServiceSettings);

            if (entities == null || !entities.Any())
            {
                return ResponseDto<LogoRestServiceSettingViewModel>.FailData(
                    404,
                    "Logo Rest Service Ayarları Henüz Yapılandırılmamış",
                    "Logo Rest Service Ayarları Henüz Yapılandırılmamış",
                    true);
            }

            var model = new LogoRestServiceSettingViewModel();
            var entityDict = entities.ToDictionary(e => e.Name, e => e.Value);

            foreach (var prop in typeof(LogoRestServiceSettingViewModel).GetProperties())
            {
                var propName = prop.Name;

                if (!entityDict.TryGetValue(propName, out var encryptedValue) || string.IsNullOrEmpty(encryptedValue))
                {
                    if (prop.PropertyType == typeof(string))
                        prop.SetValue(model, "");
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                        prop.SetValue(model, 0);
                    continue;
                }

                string decryptedText;
                try
                {
                    decryptedText = await cryptoService.DecryptAsync(encryptedValue);
                }
                catch (Exception)
                {
                    decryptedText = prop.PropertyType == typeof(string) ? "" : "0";
                }

                // Özel tip dönüşümleri burada yapılır
                if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                {
                    if (int.TryParse(decryptedText, out var intValue))
                        prop.SetValue(model, intValue);
                    else
                        prop.SetValue(model, 0);
                }
                else
                {
                    prop.SetValue(model, decryptedText);
                }
            }

            return ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Logo Rest Service Ayarları Başarıyla Getirildi", model);
        }
        catch (Exception ex)
        {
            return ResponseDto<LogoRestServiceSettingViewModel>.FailData(500, "Logo Rest Service ayarları getirilirken hata oluştu", ex.Message, true);
        }
    }

    public async Task<ResponseDto<LogoSettingViewModel>> GetLogoSettingsAsync()
    {
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.LogoUserSettings);

            if (entities == null || !entities.Any())
            {
                return ResponseDto<LogoSettingViewModel>.FailData(
                    404,
                    "Logo Ayarları Henüz Yapılandırılmamış",
                    "Logo Ayarları Henüz Yapılandırılmamış",
                    true);
            }

            var model = new LogoSettingViewModel();
            var entityDict = entities.ToDictionary(e => e.Name);

            foreach (var prop in typeof(LogoSettingViewModel).GetProperties())
            {
                var propName = prop.Name;

                if (!entityDict.TryGetValue(propName, out var entity) || string.IsNullOrEmpty(entity?.Value))
                {
                    // Ayar yoksa veya değer boşsa varsayılan boş string ata
                    prop.SetValue(model, string.Empty);
                    continue;
                }

                string decryptedValue;
                try
                {
                    decryptedValue = await cryptoService.DecryptAsync(entity.Value);
                }
                catch (Exception cryptoEx)
                {
                    // Şifre çözme hatasında logla ama kullanıcıya şifreli veri gösterme!
                    // Örnek: _logger.LogError(cryptoEx, $"Ayar çözülürken hata: {propName}");
                    decryptedValue = string.Empty; // veya "[Çözülemedi]" gibi bir placeholder
                }

                prop.SetValue(model, decryptedValue);
            }

            return ResponseDto<LogoSettingViewModel>.SuccessData(200, "Logo Ayarları Başarıyla Getirildi", model);
        }
        catch (Exception ex)
        {
            return ResponseDto<LogoSettingViewModel>.FailData(500, "Logo ayarları getirilirken hata oluştu.", ex.Message, true);
        }
    }

    //public async Task<ResponseDto<LogoSettingViewModel>> GetLogoSettingsAsync()
    //{
    //    var res = await repository.GetSettings(SettingsTypeEnum.LogoUserSettings);
    //    if (res == null)
    //    {
    //        return ResponseDto<LogoSettingViewModel>.FailData(404, "Logo Ayarları Henüz Yapılandırılmamış", "Logo Ayarları Henüz Yapılandırılmamış", true);
    //    }
    //    var retVal = new LogoSettingViewModel();
    //    foreach (var item in res)
    //    {
    //        switch (item.Name)
    //        {
    //            case "UserName":
    //                retVal.UserName = item.Value ?? "";
    //                break;
    //            case "Password":
    //                retVal.Password = item.Value ?? "";
    //                break;
    //            case "Firm":
    //                retVal.Firm = item.Value ?? "";
    //                break;
    //            case "Period":
    //                retVal.Period = item.Value ?? "";
    //                break;
    //        }
    //    }
    //    return ResponseDto<LogoSettingViewModel>.SuccessData(200, "Logo Ayarları Başarıyla Getirildi", retVal);
    //}

    //public async Task<ResponseDto<LogoSqlSettingViewModel>> GetLogoSqlSettingsAsync()
    //{
    //    var res = await repository.GetSettings(SettingsTypeEnum.LogoSqlSettings);
    //    if (res == null)
    //    {
    //        return ResponseDto<LogoSqlSettingViewModel>.FailData(404, "Logo SQL Ayarları Henüz Yapılandırılmamış", "Logo SQL Ayarları Henüz Yapılandırılmamış", true);
    //    }
    //    var retVal = new LogoSqlSettingViewModel();
    //    foreach (var item in res)
    //    {
    //        switch (item.Name)
    //        {
    //            case "Server":
    //                retVal.Server = item.Value ?? "";
    //                break;
    //            case "Database":
    //                retVal.Database = item.Value ?? "";
    //                break;
    //            case "UserName":
    //                retVal.UserName = item.Value ?? "";
    //                break;
    //            case "Password":
    //                retVal.Password = item.Value ?? "";
    //                break;
    //        }

    //    }
    //    return ResponseDto<LogoSqlSettingViewModel>.SuccessData(200, "Logo SQL Ayarları Başarıyla Getirildi", retVal);

    //}

    //public async Task<ResponseDto<LogoRestServiceSettingViewModel>> GetLogoRestServiceSettingsAsync()
    //{
    //    var res = await repository.GetSettings(SettingsTypeEnum.LogoRestServiceSettings);
    //    if (res == null)
    //    {
    //        return ResponseDto<LogoRestServiceSettingViewModel>.FailData(404, "Logo Rest Service Ayarları Henüz Yapılandırılmamış", "Logo Rest Service Ayarları Henüz Yapılandırılmamış", true);
    //    }
    //    var retVal = new LogoRestServiceSettingViewModel();
    //    foreach (var item in res)
    //    {
    //        switch (item.Name)
    //        {
    //            case "Server":
    //                retVal.Server = item.Value ?? "";
    //                break;
    //            case "Port":
    //                {
    //                    if (int.TryParse(item.Value, out var port))
    //                    {
    //                        retVal.Port = port;
    //                    }
    //                    break;
    //                }
    //            case "UserName":
    //                retVal.UserName = item.Value ?? "";
    //                break;
    //            case "Password":
    //                retVal.Password = item.Value ?? "";
    //                break;
    //            case "Firm":
    //                retVal.Firm = item.Value ?? "";
    //                break;
    //            case "Period":
    //                retVal.Period = item.Value ?? "";
    //                break;
    //        }
    //    }
    //    return ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Logo Rest Service Ayarları Başarıyla Getirildi", retVal);
    //}
}