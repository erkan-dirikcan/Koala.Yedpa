using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Extensions;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Models.ViewModels.YourNamespace.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services
{
    public class ApiLogoSqlDataService : IApiLogoSqlDataService
    {
        private readonly AppDbContext _context;
        private readonly ISqlProvider _sqlProvider;
        private readonly ILogger<ApiLogoSqlDataService> _logger;

        public LogoSettingViewModel LogoSetting { get; set; }
        public LogoSqlSettingViewModel LogoSqlSetting { get; set; }


        public ApiLogoSqlDataService( ILogger<ApiLogoSqlDataService> logger, ISqlProvider sqlProvider, AppDbContext context, ISettingsService settingsService)
        {
            _logger = logger;
            _sqlProvider = sqlProvider;
            _context = context;
            var logoSettingResp = settingsService.GetLogoSettingsAsync().Result;
            if (logoSettingResp.IsSuccess)
            {
                LogoSetting = logoSettingResp.Data;
            }
            var logoSqlSettingResp = settingsService.GetLogoSqlSettingsAsync().Result;
            if (logoSqlSettingResp.IsSuccess)
            {
                LogoSqlSetting = logoSqlSettingResp.Data;
            }
        }

        public async Task<ResponseListDto<List<ClCardInfoViewModel>>> GetAllClCardInfoAsync(int perPage = 50, int pageNo = 1)
        {
            if (perPage <= 0) perPage = 50;
            if (pageNo <= 0) pageNo = 1;

            var offset = (pageNo - 1) * perPage;

            var query = BuildBaseClCardQuery();

            var totalQuery = $@"
                                SELECT COUNT(*) 
                                FROM LG_{LogoSetting.Firm}_CLCARD AS CLC
                                INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS CLP ON CLP.LOGICALREF = CLC.PARENTCLREF
                                WHERE CLP.CODE LIKE '%.IS'";

            var totalResult = _sqlProvider.SqlReader(totalQuery);
            var recordsTotal = totalResult.IsSuccess ? Convert.ToInt32(totalResult.Data.Rows[0][0]) : 0;

            query = $@"
                    {query}
                    ORDER BY CLC.CODE, CLP.CODE
                    OFFSET {offset} ROWS
                    FETCH NEXT {perPage} ROWS ONLY";



            var result = _sqlProvider.SqlReader(query);
            if (!result.IsSuccess)
            {
                _logger.LogError("GetAllClCardInfoAsync - Hata: {Message}", result.Message);
                return ResponseListDto<List<ClCardInfoViewModel>>.FailData(500, "Veri çekilemedi", result.Message, true);
            }


            var list = result.Data.AsList<ClCardInfoViewModel>();

            return ResponseListDto<List<ClCardInfoViewModel>>.SuccessData(
                200, "Tüm kayıtlar getirildi", list,
                RecordsTotal: recordsTotal,
                RecordsFiltered: recordsTotal,
                RecordsShow: list.Count
            );
        }

        public async Task<ResponseListDto<List<ClCardInfoViewModel>>> WhereClCardInfoAsync(ClCardInfoSearchViewModel searchModel, int perPage = 50, int pageNo = 1)
        {

            if (searchModel == null) searchModel = new ClCardInfoSearchViewModel();
            if (perPage <= 0) perPage = 20;
            if (pageNo <= 0) pageNo = 1;
            var offset = (pageNo - 1) * perPage;
            var query = BuildBaseClCardQuery();

            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchModel.CariKodu))
                conditions.Add("CLC.CODE LIKE '%' + @CariKodu + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.CariUnvan))
                conditions.Add("CLC.DEFINITION_ LIKE '%' + @CariUnvan + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.VergiDairesi))
                conditions.Add("CLC.TAXOFFICE LIKE '%' + @VergiDairesi + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.VergiNumarasi))
                conditions.Add("CLC.TAXNR LIKE '%' + @VergiNumarasi + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Tckn))
                conditions.Add("CLC.TCKNO LIKE '%' + @Tckn + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Il))
                conditions.Add("CLC.CITY LIKE '%' + @Il + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Ilce))
                conditions.Add("CLC.TOWN LIKE '%' + @Ilce + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Mahalle))
                conditions.Add("CLC.DISTRICT LIKE '%' + @Mahalle + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Adres1))
                conditions.Add("CLC.ADDR1 LIKE '%' + @Adres1 + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Adres2))
                conditions.Add("CLC.ADDR2 LIKE '%' + @Adres2 + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod))
                conditions.Add("CLC.SPECODE LIKE '%' + @OzelKod + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod2))
                conditions.Add("CLC.SPECODE2 LIKE '%' + @OzelKod2 + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod3))
                conditions.Add("CLC.SPECODE3 LIKE '%' + @OzelKod3 + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod4))
                conditions.Add("CLC.SPECODE4 LIKE '%' + @OzelKod4 + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod5))
                conditions.Add("CLC.SPECODE5 LIKE '%' + @OzelKod5 + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.DukkanCariKodu))
                conditions.Add("CLP.CODE LIKE '%' + @DukkanCariKodu + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.DukkanAdresOrjinal))
                conditions.Add("CLP.DEFINITION_ LIKE '%' + @DukkanAdresOrjinal + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Cadde))
                conditions.Add("CADDE LIKE '%' + @Cadde + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.No))
                conditions.Add("NO LIKE '%' + @No + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.PasajNo))
                conditions.Add("PASAJ_NO LIKE '%' + @PasajNo + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.YeniNo))
                conditions.Add("YENI_NO LIKE '%' + @YeniNo + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Kat))
                conditions.Add("KAT LIKE '%' + @Kat + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Yetkili1AdSoyad))
                conditions.Add("CLC.INCHARGE LIKE '%' + @Yetkili1AdSoyad + '%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Yetkili2AdSoyad))
                conditions.Add("CLC.INCHARGE2 LIKE '%' + @Yetkili2AdSoyad + '%'");

            // 1) Filtresiz toplam kayıt (RecordsTotal)
            var whereClause = conditions.Any() ? " AND " + string.Join(" AND ", conditions) : "";
            var totalQuery = $@"
                                SELECT COUNT(*) 
                                FROM LG_{LogoSetting.Firm}_CLCARD AS CLC
                                INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS CLP ON CLP.LOGICALREF = CLC.PARENTCLREF
                                WHERE CLP.CODE LIKE '%.IS'";
            var totalResult = _sqlProvider.SqlReader(totalQuery);
            var recordsTotal = totalResult.IsSuccess ? Convert.ToInt32(totalResult.Data.Rows[0][0]) : 0;

            // 2) Filtreli toplam kayıt (RecordsFiltered)
            var filteredCountQuery = query + whereClause;
            var countQuery = "SELECT COUNT(*) FROM (" + filteredCountQuery + ") AS T";
            var filteredResult = _sqlProvider.SqlReader(countQuery);
            var recordsFiltered = filteredResult.IsSuccess ? Convert.ToInt32(filteredResult.Data.Rows[0][0]) : 0;


            var pagedQuery = $@"
                                {query}
                                {whereClause}
                                ORDER BY CLC.CODE, CLP.CODE
                                OFFSET {offset} ROWS
                                FETCH NEXT {perPage} ROWS ONLY";


            var result = _sqlProvider.SqlReader(query);
            if (!result.IsSuccess)
            {
                _logger.LogError("WhereClCardInfoAsync - Sorgu hatası: {Message}", result.Message);
                return ResponseListDto<List<ClCardInfoViewModel>>.FailData(500, "Arama sırasında hata oluştu", result.Message, true);
            }




            var list = result.Data.AsList<ClCardInfoViewModel>();

            return ResponseListDto<List<ClCardInfoViewModel>>.SuccessData(
                statusCode: 200,
                message: "Arama başarılı",
                data: list,
                RecordsTotal: recordsTotal,
                RecordsFiltered: recordsFiltered,
                RecordsShow: list.Count
            );
        }

        private string BuildBaseClCardQuery()
        {
            return $@"
                SELECT
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.CODE)), ''), '') AS CARI_KODU,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.DEFINITION_)), ''), '') AS CARI_UNVAN,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TAXOFFICE)), ''), '') AS VERGI_DAIRESI,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TAXNR)), ''), '') AS VERGI_NUMARASI,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TCKNO)), ''), '') AS TCKN,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.CITY)), ''), '') AS IL,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TOWN)), ''), '') AS ILCE,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.DISTRICT)), ''), '') AS MAHALLE,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.ADDR1)), ''), '') AS ADRES_1,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.ADDR2)), ''), '') AS ADRES_2,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.EMAILADDR3)), ''), '') AS EMAIL_3,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TELEXTNUMS1)), ''), '') AS TELEFON_1,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TELEXTNUMS2)), ''), '') AS TELEFON_2,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.FAXNR)), ''), '') AS FAX,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.SPECODE)), ''), '') AS OZEL_KOD,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.SPECODE2)), ''), '') AS OZEL_KOD2,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.SPECODE3)), ''), '') AS OZEL_KOD3,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.SPECODE4)), ''), '') AS OZEL_KOD4,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.SPECODE5)), ''), '') AS OZEL_KOD5,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.INCHARGE)), ''), '') AS YETKILI1_AD_SOYAD,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.EMAILADDR)), ''), '') AS YETKILI1_EMAIL,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.INCHTELNRS1)), ''), '') AS YETKILI1_TELEFON,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.INCHARGE2)), ''), '') AS YETKILI2_AD_SOYAD,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.EMAILADDR2)), ''), '') AS YETKILI2_EMAIL,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.INCHTELNRS2)), ''), '') AS YETKILI2_TELEFON,
                    CLP.CODE AS DUKKAN_CARI_KODU,
                    LTRIM(RTRIM(CLP.DEFINITION_)) AS DUKKAN_ADRES_ORJINAL,
                    ISNULL(NULLIF(LTRIM(RTRIM(LEFT(CLP.DEFINITION_, CHARINDEX(' CADDESİ', UPPER(CLP.DEFINITION_ + ' CADDESİ')) - 1))), ''), '') AS CADDE,
                    ISNULL(NULLIF(LTRIM(RTRIM(SUBSTRING(CLP.DEFINITION_, CHARINDEX('NO:', UPPER(CLP.DEFINITION_ + ' NO:')) + 3,
                        CASE 
                            WHEN CHARINDEX(' (YENI', UPPER(CLP.DEFINITION_), CHARINDEX('NO:', UPPER(CLP.DEFINITION_))) > 0 
                                THEN CHARINDEX(' (YENI', UPPER(CLP.DEFINITION_), CHARINDEX('NO:', UPPER(CLP.DEFINITION_))) - CHARINDEX('NO:', UPPER(CLP.DEFINITION_)) - 3
                            WHEN CHARINDEX(' AS', UPPER(CLP.DEFINITION_), CHARINDEX('NO:', UPPER(CLP.DEFINITION_))) > 0 
                                THEN CHARINDEX(' AS', UPPER(CLP.DEFINITION_), CHARINDEX('NO:', UPPER(CLP.DEFINITION_))) - CHARINDEX('NO:', UPPER(CLP.DEFINITION_)) - 3
                            WHEN CHARINDEX(' ZE', UPPER(CLP.DEFINITION_), CHARINDEX('NO:', UPPER(CLP.DEFINITION_))) > 0 
                                THEN CHARINDEX(' ZE', UPPER(CLP.DEFINITION_), CHARINDEX('NO:', UPPER(CLP.DEFINITION_))) - CHARINDEX('NO:', UPPER(CLP.DEFINITION_)) - 3
                            ELSE 30 
                        END ))), ''), '') AS NO,
                    ISNULL(NULLIF(CASE 
                        WHEN PATINDEX('%[0-9][.]PASAJ%', UPPER(CLP.DEFINITION_)) > 0 THEN SUBSTRING(CLP.DEFINITION_, PATINDEX('%[0-9][.]PASAJ%', UPPER(CLP.DEFINITION_)), 1)
                        WHEN PATINDEX('%[0-9] .PASAJ%', UPPER(CLP.DEFINITION_)) > 0 THEN SUBSTRING(CLP.DEFINITION_, PATINDEX('%[0-9] .PASAJ%', UPPER(CLP.DEFINITION_)), 1) 
                        END, ''), '') AS PASAJ_NO,
                    ISNULL(NULLIF(LTRIM(RTRIM(REPLACE(REPLACE(SUBSTRING(CLP.DEFINITION_, CHARINDEX('(YENI NO:', UPPER(CLP.DEFINITION_ + '(YENI NO:')) + 9, 30), ')', ''), ' ', ''))), ''), '') AS YENI_NO,
                    ISNULL(NULLIF(CASE
                        WHEN UPPER(CLP.DEFINITION_) LIKE '% AS%' OR UPPER(CLP.DEFINITION_) LIKE '%AS)%' THEN 'ASMAKAT'
                        WHEN UPPER(CLP.DEFINITION_) LIKE '% ZE%' OR UPPER(CLP.DEFINITION_) LIKE '%ZE)%' THEN 'ZEMİN KAT'
                        WHEN UPPER(CLP.DEFINITION_) LIKE '%DEPOLAR%' THEN 'DEPOLAR'
                        WHEN UPPER(CLP.DEFINITION_) LIKE '%KİRALIK DEPO%' THEN 'KİRALIK DEPO'
                        WHEN UPPER(CLP.DEFINITION_) LIKE '%BODRUM%' THEN 'BODRUM KAT'
                        ELSE NULL 
                    END, ''), '') AS KAT
                FROM LG_{LogoSetting.Firm}_CLCARD AS CLC
                INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS CLP ON CLP.LOGICALREF = CLC.PARENTCLREF
                WHERE CLP.CODE LIKE '%.IS'
                  AND CLP.DEFINITION_ IS NOT NULL
                  AND LTRIM(RTRIM(CLP.DEFINITION_)) <> ''";
        }


    }
}
