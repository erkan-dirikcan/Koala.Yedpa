using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Exceptions;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class SettingsService(ISettingsRepository repository, IUnitOfWork<AppDbContext> unitOfWork, ICryptoService cryptoService, IMapper mapper, ILogger<SettingsService> logger)
    : ISettingsService
{
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<SettingsService> _logger = logger;

    public async Task<ResponseDto> AddEmailSettings(List<AddSettingViewModel> model)
    {
        _logger.LogInformation("AddEmailSettings called with {Count} settings", model?.Count ?? 0);
        try
        {
            if (model == null || !model.Any())
            {
                _logger.LogWarning("AddEmailSettings: No settings provided");
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
            _logger.LogInformation("AddEmailSettings: Successfully added {Count} email settings", entities.Count);
            return ResponseDto.Success(200, "E-Posta Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddEmailSettings: Error while adding email settings");
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
        _logger.LogInformation("UpdateEmailSettingsAsync called");
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
                    _logger.LogError(cryptoEx, "UpdateEmailSettingsAsync: Error encrypting property {PropertyName}", propertyName);
                    errors.Add($"'{propertyName}' şifrelenirken hata: {cryptoEx.Message}");
                    continue;
                }

                var entity = await repository.GetSettingByName(propertyName, SettingsTypeEnum.EmailSettings);
                if (entity == null)
                {
                    _logger.LogWarning("UpdateEmailSettingsAsync: Setting not found for {PropertyName}", propertyName);
                    errors.Add($"'{propertyName}' isimli ayar bulunamadı.");
                    continue;
                }

                entity.Value = encryptedValue;
                repository.UpdateSettings(entity);
            }

            if (errors.Any())
            {
                _logger.LogWarning("UpdateEmailSettingsAsync: Completed with {ErrorCount} errors", errors.Count);
                return ResponseDto.Fail(400, "E-Posta Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            _logger.LogInformation("UpdateEmailSettingsAsync: Successfully updated email settings");
            return ResponseDto.Success(200, "E-Posta Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEmailSettingsAsync: Unexpected error");
            return ResponseDto.Fail(500, "E-Posta Ayarları Güncellenirken Beklenmeyen Hata", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateLogoSettingsAsync(LogoSettingViewModel model)
    {
        _logger.LogInformation("UpdateLogoSettingsAsync called");
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    _logger.LogError(licenseEx, "UpdateLogoSettingsAsync: License error");
                    return ResponseDto.Fail(403, licenseEx.Message, licenseEx.Message, true);
                }
                catch (Exception cryptoEx)
                {
                    _logger.LogError(cryptoEx, "UpdateLogoSettingsAsync: Error encrypting property {PropertyName}", propertyName);
                    errors.Add($"'{propertyName}' şifrelenirken hata: {cryptoEx.Message}");
                    continue;
                }

                var entity = await repository.GetSettingByName(propertyName, SettingsTypeEnum.LogoUserSettings);
                if (entity == null)
                {
                    _logger.LogWarning("UpdateLogoSettingsAsync: Setting not found for {PropertyName}", propertyName);
                    errors.Add($"'{propertyName}' isimli ayar bulunamadı.");
                    continue;
                }

                entity.Value = encryptedValue;
                repository.UpdateSettings(entity);
            }

            if (errors.Any())
            {
                _logger.LogWarning("UpdateLogoSettingsAsync: Completed with {ErrorCount} errors", errors.Count);
                return ResponseDto.Fail(400, "Logo Kullanıcı Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            _logger.LogInformation("UpdateLogoSettingsAsync: Successfully updated logo settings");
            return ResponseDto.Success(200, "Logo Kullanıcı Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateLogoSettingsAsync: Unexpected error");
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto.Fail(403, licenseEx.Message, licenseEx.Message, true);
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto.Fail(403, licenseEx.Message, licenseEx.Message, true);
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
        _logger.LogInformation("GetEmailSettingsAsync called");
        var res = await repository.GetSettings(SettingsTypeEnum.EmailSettings);
        if (res == null || res.Count < 1)
        {
            _logger.LogWarning("GetEmailSettingsAsync: No email settings found");
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    _logger.LogError(licenseEx, "GetEmailSettingsAsync: License error while decrypting");
                    return ResponseDto<EmailSettingViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
                }
                catch (Exception decryptEx)
                {
                    _logger.LogError(decryptEx, "GetEmailSettingsAsync: Error decrypting value for {SettingName}", item.Name);
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

        _logger.LogInformation("GetEmailSettingsAsync: Successfully retrieved email settings");
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto<LogoSqlSettingViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto<LogoRestServiceSettingViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
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
        _logger.LogInformation("GetLogoSettingsAsync called");
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.LogoUserSettings);

            if (entities == null || !entities.Any())
            {
                _logger.LogWarning("GetLogoSettingsAsync: No logo settings found");
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    _logger.LogError(licenseEx, "GetLogoSettingsAsync: License error while decrypting");
                    return ResponseDto<LogoSettingViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
                }
                catch (Exception cryptoEx)
                {
                    // Şifre çözme hatasında logla ama kullanıcıya şifreli veri gösterme!
                    _logger.LogError(cryptoEx, "GetLogoSettingsAsync: Error decrypting property {PropertyName}", propName);
                    decryptedValue = string.Empty; // veya "[Çözülemedi]" gibi bir placeholder
                }

                prop.SetValue(model, decryptedValue);
            }

            _logger.LogInformation("GetLogoSettingsAsync: Successfully retrieved logo settings");
            return ResponseDto<LogoSettingViewModel>.SuccessData(200, "Logo Ayarları Başarıyla Getirildi", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLogoSettingsAsync: Unexpected error");
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

    public async Task<ResponseDto> AddMessage34Settings(List<AddSettingViewModel> model)
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
                entity.SettingType = SettingsTypeEnum.Message34Settings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Message34 Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Message34 Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateMessage34SettingsAsync(Message34SettingsViewModel model)
    {
        try
        {
            var errors = new List<string>();

            // Tüm Message34 ayarlarını bir kereye mahsus çekip dictionary'ye alalım
            var entities = await repository.GetSettings(SettingsTypeEnum.Message34Settings);
            var entityDict = entities.ToDictionary(e => e.Name, e => e);

            foreach (var prop in typeof(Message34SettingsViewModel).GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // TÜM ALANLARI ŞİFRELE (boş string bile gelse)
                string encryptedValue;
                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto.Fail(403, licenseEx.Message, licenseEx.Message, true);
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
                return ResponseDto.Fail(400, "Message34 Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Message34 Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "Message34 Ayarları güncellenirken beklenmeyen hata", ex.Message, true);
        }
    }

    public async Task<ResponseDto<Message34SettingsViewModel>> GetMessage34SettingsAsync()
    {
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.Message34Settings);

            if (entities == null || !entities.Any())
            {
                return ResponseDto<Message34SettingsViewModel>.FailData(
                    404,
                    "Message34 Ayarları Henüz Yapılandırılmamış",
                    "Message34 Ayarları Henüz Yapılandırılmamış",
                    true);
            }

            var model = new Message34SettingsViewModel();
            var entityDict = entities.ToDictionary(e => e.Name, e => e.Value);

            foreach (var prop in typeof(Message34SettingsViewModel).GetProperties())
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
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto<Message34SettingsViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
                }
                catch (Exception)
                {
                    decryptedValue = ""; // asla şifreli veri dönmesin
                }

                prop.SetValue(model, decryptedValue);
            }

            return ResponseDto<Message34SettingsViewModel>.SuccessData(200, "Message34 Ayarları Başarıyla Getirildi",
                model);
        }
        catch (Exception ex)
        {
            return ResponseDto<Message34SettingsViewModel>.FailData(500, "Message34 ayarları getirilirken hata oluştu",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddKoalaApiSettings(List<AddSettingViewModel> model)
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
                entity.SettingType = SettingsTypeEnum.KoalaApiSettings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Koala API Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "Koala API Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateKoalaApiSettingsAsync(KoalaApiSettingsViewModel model)
    {
        try
        {
            var errors = new List<string>();

            // Tüm KoalaApiSettings ayarlarını bir kereye mahsus çekip dictionary'ye alalım
            var entities = await repository.GetSettings(SettingsTypeEnum.KoalaApiSettings);

            // Kayıt yoksa hata ver
            if (entities == null || !entities.Any())
            {
                return ResponseDto.Fail(404, "Koala API Ayarları Henüz Veritabanında Oluşturulmamış",
                    "Lütfen önce veritabanında KoalaApiSettings kayıtlarını oluşturun.", true);
            }

            var entityDict = entities.ToDictionary(e => e.Name, e => e);

            foreach (var prop in typeof(KoalaApiSettingsViewModel).GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // TÜM ALANLARI ŞİFRELE (boş string bile gelse)
                string encryptedValue;
                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto.Fail(403, licenseEx.Message, licenseEx.Message, true);
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
                return ResponseDto.Fail(400, "Koala API Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "Koala API Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "Koala API Ayarları güncellenirken beklenmeyen hata", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateQRCodeSettingsAsync(QRCodeSettingsViewModel model)
    {
        try
        {
            var errors = new List<string>();

            // Tüm QRCode ayarlarını bir kereye mahsus çekip dictionary'ye alalım
            var entities = await repository.GetSettings(SettingsTypeEnum.ApplicationSettings);

            // Kayıt yoksa hata ver
            if (entities == null || !entities.Any())
            {
                return ResponseDto.Fail(404, "QR Code Ayarları Henüz Veritabanında Oluşturulmamış",
                    "Lütfen önce veritabanında QRCodeSettings kayıtlarını oluşturun.", true);
            }

            var entityDict = entities.ToDictionary(e => e.Name, e => e);

            foreach (var prop in typeof(QRCodeSettingsViewModel).GetProperties())
            {
                var propertyName = prop.Name;
                var propertyValue = prop.GetValue(model)?.ToString() ?? string.Empty;

                // TÜM ALANLARI ŞİFRELE (boş string bile gelse)
                string encryptedValue;
                try
                {
                    encryptedValue = await cryptoService.EncryptAsync(propertyValue);
                }
                catch (CryptoLicenseException licenseEx)
                {
                    // Lisans hatasında hemen durdur ve hata mesajını döndür
                    return ResponseDto.Fail(403, licenseEx.Message, licenseEx.Message, true);
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
                return ResponseDto.Fail(400, "QR Code Ayarları Güncellenirken Hata Oluştu", errors, true);
            }

            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "QR Code Ayarları Başarıyla Güncellendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(500, "QR Code Ayarları güncellenirken beklenmeyen hata", ex.Message, true);
        }
    }

    public async Task<ResponseDto<QRCodeSettingsViewModel>> GetQRCodeSettingsAsync()
    {
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.ApplicationSettings);

            // Kayıt yoksa boş model döndür (kullanıcı formu doldurabilir)
            var model = new QRCodeSettingsViewModel();

            if (entities != null && entities.Any())
            {
                var entityDict = entities.ToDictionary(e => e.Name, e => e.Value);

                foreach (var prop in typeof(QRCodeSettingsViewModel).GetProperties())
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
                    catch (CryptoLicenseException licenseEx)
                    {
                        // Lisans hatasında hemen durdur ve hata mesajını döndür
                        return ResponseDto<QRCodeSettingsViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
                    }
                    catch (Exception)
                    {
                        decryptedValue = ""; // asla şifreli veri dönmesin
                    }

                    prop.SetValue(model, decryptedValue);
                }
            }

            return ResponseDto<QRCodeSettingsViewModel>.SuccessData(200, "QR Code Ayarları Başarıyla Getirildi", model);
        }
        catch (Exception ex)
        {
            return ResponseDto<QRCodeSettingsViewModel>.FailData(500, "QR Code ayarları getirilirken hata oluştu",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto<KoalaApiSettingsViewModel>> GetKoalaApiSettingsAsync()
    {
        try
        {
            var entities = await repository.GetSettings(SettingsTypeEnum.KoalaApiSettings);

            // Kayıt yoksa boş model döndür (kullanıcı formu doldurabilir)
            var model = new KoalaApiSettingsViewModel();

            if (entities != null && entities.Any())
            {
                var entityDict = entities.ToDictionary(e => e.Name, e => e.Value);

                foreach (var prop in typeof(KoalaApiSettingsViewModel).GetProperties())
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
                    catch (CryptoLicenseException licenseEx)
                    {
                        // Lisans hatasında hemen durdur ve hata mesajını döndür
                        return ResponseDto<KoalaApiSettingsViewModel>.FailData(403, licenseEx.Message, licenseEx.Message, true);
                    }
                    catch (Exception)
                    {
                        decryptedValue = ""; // asla şifreli veri dönmesin
                    }

                    prop.SetValue(model, decryptedValue);
                }
            }

            return ResponseDto<KoalaApiSettingsViewModel>.SuccessData(200, "Koala API Ayarları Başarıyla Getirildi", model);
        }
        catch (Exception ex)
        {
            return ResponseDto<KoalaApiSettingsViewModel>.FailData(500, "Koala API ayarları getirilirken hata oluştu",
                ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddQRCodeSettings(List<AddSettingViewModel> model)
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
                entity.SettingType = SettingsTypeEnum.ApplicationSettings;
                await repository.AddSettingsAsync(entity);
            }
            await unitOfWork.CommitAsync();
            return ResponseDto.Success(200, "QR Code Ayarları Başarıyla Eklendi");
        }
        catch (Exception ex)
        {
            return ResponseDto.Fail(400, "QR Code Ayarları Oluşturulurken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }
}