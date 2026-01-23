using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Extensions;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Koala.Yedpa.Service.Services
{
    public class ApiLogoSqlDataService : IApiLogoSqlDataService
    {
        private readonly AppDbContext _context;
        private readonly ISqlProvider _sqlProvider;
        private readonly ILogger<ApiLogoSqlDataService> _logger;

        public LogoSettingViewModel LogoSetting { get; set; }
        public LogoSqlSettingViewModel LogoSqlSetting { get; set; }


        public ApiLogoSqlDataService(ILogger<ApiLogoSqlDataService> logger, ISqlProvider sqlProvider, AppDbContext context, ISettingsService settingsService)
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

        public async Task<ResponseDto<List<StatementSummeryViewModel>>> GetClsStatementsSummertAsync()
        {
            try
            {
                var query = $@"
                                WITH LastSales AS (
                                    SELECT 
                                        CLIENTREF,
                                        MAX(DATE_) AS LASTSALEDATE
                                    FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_STLINE 
                                    WHERE TRCODE IN (7, 8)
                                    GROUP BY CLIENTREF
                                ),
                                LastPayments AS (
                                    SELECT 
                                        CLIENTREF,
                                        MAX(DATE_) AS LASTPAYMENTDATE
                                    FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_CLFLINE 
                                    WHERE TRCODE IN (1)
                                    GROUP BY CLIENTREF
                                )
                                SELECT 
                                    CLC.LOGICALREF AS LogicalRef,
                                    CLC.CODE AS ClCode,
                                    CLC.DEFINITION_ AS ClDefinition,   
                                    ls.LASTSALEDATE AS LastInvoiceDate,
                                    lp.LASTPAYMENTDATE AS LastPayDate,
                                    CONVERT(DECIMAL(38, 2), SUM(GNCLTOT.DEBIT) - SUM(GNCLTOT.CREDIT)) AS Balance
                                FROM LV_{LogoSetting.Firm}_{LogoSetting.Period}_GNTOTCL GNCLTOT WITH(NOLOCK)
                                INNER JOIN dbo.LG_{LogoSetting.Firm}_CLCARD CLC WITH(NOLOCK)
                                    ON CLC.LOGICALREF = GNCLTOT.CARDREF
                                LEFT JOIN LastSales ls
                                    ON ls.CLIENTREF = CLC.LOGICALREF
                                LEFT JOIN LastPayments lp
                                    ON lp.CLIENTREF = CLC.LOGICALREF
                                WHERE GNCLTOT.TOTTYP = 1
                                  AND CLC.CODE IS NOT NULL
                                  AND CLC.ACTIVE=0
                                  AND CLC.CODE LIKE '1.%'
                                  AND CLC.CODE NOT LIKE '%KD%'
                                  AND ISNULL(CLC.SPECODE, '') NOT LIKE 'KIRMIZI%'
                                  AND ISNULL(CLC.SPECODE, '') NOT LIKE 'YEŞİL%'
                                  AND PARENTCLREF IN (
                                    SELECT LOGICALREF 
                                    FROM dbo.LG_{LogoSetting.Firm}_CLCARD 
                                    WHERE CARDTYPE = 4 
                                    AND ACTIVE = 0
                                  )
                                GROUP BY 
                                    CLC.LOGICALREF,
                                    CLC.CODE,
                                    CLC.DEFINITION_,
                                    ls.LASTSALEDATE,
                                    lp.LASTPAYMENTDATE
                                ORDER BY Balance DESC";

                var result = _sqlProvider.SqlReader(query);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetClsStatementsSummertAsync - Sorgu hatası: {Message}", result.Message);
                    return ResponseDto<List<StatementSummeryViewModel>>.FailData(500, "Veri çekilemedi", result.Message, true);
                }

                var list = result.Data.AsList<StatementSummeryViewModel>();

                return ResponseDto<List<StatementSummeryViewModel>>.SuccessData(
                    200, "Cari hesap özet bilgileri başarıyla getirildi", list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClsStatementsSummertAsync - Beklenmeyen hata");
                return ResponseDto<List<StatementSummeryViewModel>>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<List<ClCardStatementViewModel>>> GetClCardStatementAsync(string clCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clCode))
                {
                    return ResponseDto<List<ClCardStatementViewModel>>.FailData(400, "Cari kodu boş olamaz", "clCode parametresi gereklidir", true);
                }

                var query = $@"
                                WITH FilteredTransactions AS (
                                    SELECT 
                                        CTRNS.*,
                                        CLNTC.LOGICALREF AS CL_LOGICALREF,
                                        CLNTC.CODE AS CODE,
                                        CLNTC.DEFINITION_ AS DEFINITION_,
                                        CLFIC.CANCELLED AS CLFIC_CANCELLED,
                                        INVFC.CANCELLED AS INVFC_CANCELLED,
                                        RLFIC.CANCELLED AS RLFIC_CANCELLED,
                                        ORFIC.CANCELLED AS ORFIC_CANCELLED,
                                        CASE 
                                            WHEN CTRNS.TRCODE = 14 AND CTRNS.MODULENR = 5 THEN 0 
                                            ELSE 1 
                                        END AS SORT_PRIORITY,
                                        CASE 
                                            WHEN CTRNS.MODULENR = 5 THEN CLFIC.TIME 
                                            WHEN CTRNS.MODULENR = 4 THEN INVFC.TIME_ 
                                            ELSE CTRNS.FTIME 
                                        END AS SORT_TIME,
                                        -- Satır bazında bakiye (cari hareket tutarı)
                                        CASE SIGN 
                                            WHEN 0 THEN ROUND(AMOUNT, 2) 
                                            ELSE ROUND(-1 * AMOUNT, 2) 
                                        END AS ROW_BALANCE
                                    FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_CLFLINE CTRNS WITH (NOLOCK)
                                    LEFT JOIN LG_{LogoSetting.Firm}_CLCARD CLNTC WITH (NOLOCK)
                                        ON CTRNS.CLIENTREF = CLNTC.LOGICALREF
                                    LEFT JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_CLFICHE CLFIC WITH (NOLOCK)
                                        ON CTRNS.SOURCEFREF = CLFIC.LOGICALREF
                                    LEFT JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_INVOICE INVFC WITH (NOLOCK)
                                        ON CTRNS.MODULENR = 4 AND CTRNS.SOURCEFREF = INVFC.LOGICALREF
                                    LEFT JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_CSROLL RLFIC WITH (NOLOCK)
                                        ON CTRNS.SOURCEFREF = RLFIC.LOGICALREF
                                    LEFT JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_ORFICHE ORFIC WITH (NOLOCK)
                                        ON CTRNS.SOURCEFREF = ORFIC.LOGICALREF
                                    WHERE CTRNS.BRANCH IN (0)
                                        AND CTRNS.DEPARTMENT IN (0)
                                        AND CTRNS.TRCODE IN (
                                            31, 32, 33, 34, 36, 37, 38, 39, 43, 44, 56, 
                                            1, 2, 3, 4, 5, 6, 12, 14, 41, 42, 45, 46, 70, 
                                            71, 72, 73, 20, 21, 24, 25, 28, 29, 30, 61, 62, 63, 64, 75
                                        )
                                        AND (CLFIC.CANCELLED = 0 OR CLFIC.CANCELLED IS NULL)
                                        AND (INVFC.CANCELLED = 0 OR INVFC.CANCELLED IS NULL)
                                        AND (RLFIC.CANCELLED = 0 OR RLFIC.CANCELLED IS NULL)
                                        AND (ORFIC.CANCELLED = 0 OR ORFIC.CANCELLED IS NULL)
                                        AND CLNTC.CODE = '{clCode.Replace("'", "''")}'
                                ),
                                NumberedTransactions AS (
                                    SELECT 
                                        *,
                                        ROW_NUMBER() OVER (
                                            ORDER BY DATE_, SORT_PRIORITY, SORT_TIME, LOGICALREF, FTIME
                                        ) AS NR
                                    FROM FilteredTransactions
                                )
                                SELECT 
                                    NT.NR,
                                    NT.CL_LOGICALREF AS LogicalRef,
                                    NT.CODE AS ClCode,
                                    NT.DEFINITION_ AS ClDefinition,
                                    NT.DATE_ AS ReceiptDate,
                                    NT.TRANNO AS ReceiptNo,
                                    ((NT.MODULENR * 100) + NT.TRCODE) AS ReceiptTypeNo,
                                    CASE ((NT.MODULENR * 100) + NT.TRCODE)
                                        WHEN 381 THEN 'Satış Siparişi'
                                        WHEN 382 THEN 'Satınalma Siparişi'
                                        WHEN 431 THEN 'Satın Alma Faturası'
                                        WHEN 432 THEN 'Perakende Satış İade Faturası'
                                        WHEN 433 THEN 'Toptan Satış İade Faturası'
                                        WHEN 434 THEN 'Alınan Hizmet Faturası'
                                        WHEN 435 THEN 'Alınan Proforma Faturası'
                                        WHEN 436 THEN 'Alım İade Faturası'
                                        WHEN 437 THEN 'Perakende Satış Faturası'
                                        WHEN 438 THEN 'Toptan Satış Faturası'
                                        WHEN 439 THEN 'Verilen Hizmet Faturası'
                                        WHEN 440 THEN 'Verilen Proforma Faturası'
                                        WHEN 441 THEN 'Verilen Vade Farkı Faturası'
                                        WHEN 442 THEN 'Alınan Vade Farkı Faturası'
                                        WHEN 443 THEN 'Alınan Fiyat Farkı Faturası'
                                        WHEN 444 THEN 'Verilen Fiyat Farkı Faturası'
                                        WHEN 456 THEN 'Müstahsil Makbuzu'
                                        WHEN 501 THEN 'Nakit Tahsilat'
                                        WHEN 502 THEN 'Nakit Ödeme'
                                        WHEN 503 THEN 'Borç Dekontu'
                                        WHEN 504 THEN 'Alacak Dekontu'
                                        WHEN 505 THEN 'Virman İşlemi'
                                        WHEN 506 THEN 'Kur Farkı İşlemi'
                                        WHEN 512 THEN 'Özel İşlem'
                                        WHEN 514 THEN 'Açılış Fişi'
                                        WHEN 570 THEN 'Kredi Kartı Fişi'
                                        WHEN 661 THEN 'Çek Girişi'
                                        WHEN 662 THEN 'Senet Girişi'
                                        WHEN 663 THEN 'Çek Çıkış Cari Hesaba'
                                        WHEN 664 THEN 'Senet Çıkış Cari Hesaba'
                                        WHEN 720 THEN 'Gelen Havaleler'
                                        WHEN 721 THEN 'Gönderilen Havaleler'
                                        WHEN 728 THEN 'Banka Alınan Hizmet'
                                        WHEN 729 THEN 'Banka Verilen Hizmet'
                                        WHEN 1001 THEN 'Nakit Tahsilat'
                                        WHEN 1002 THEN 'Nakit Ödeme'
                                        WHEN 6103 THEN 'Borç Dekontu (Çek)'
                                        WHEN 6104 THEN 'Alacak Dekontu (Çek)'
                                    END AS ReceiptType,
                                    NT.DOCODE AS DOCODE,
                                    NT.LINEEXP AS LineExp,
                                    CASE NT.SIGN WHEN 0 THEN ROUND(NT.AMOUNT, 2) ELSE 0 END AS Debit,
                                    CASE NT.SIGN WHEN 1 THEN ROUND(-1 * NT.AMOUNT, 2) ELSE 0 END AS Credit,
                                    -- Satır bazında bakiye
                                    NT.ROW_BALANCE AS RowBalance,
                                    -- Kümülatif (toplam) bakiye
                                    SUM(NT.ROW_BALANCE) OVER (
                                        ORDER BY NT.DATE_, NT.SORT_PRIORITY, NT.SORT_TIME, NT.LOGICALREF, NT.FTIME
                                        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                                    ) AS CumulativeBalance
                                FROM NumberedTransactions NT
                                ORDER BY NT.DATE_, NT.SORT_PRIORITY, NT.SORT_TIME, NT.LOGICALREF, NT.FTIME";

                var result = _sqlProvider.SqlReader(query);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetClCardStatementAsync - Sorgu hatası: {Message}", result.Message);
                    return ResponseDto<List<ClCardStatementViewModel>>.FailData(500, "Veri çekilemedi", result.Message, true);
                }

                // Transform DataTable to grouped structure
                if (result.Data == null || result.Data.Rows.Count == 0)
                {
                    return ResponseDto<List<ClCardStatementViewModel>>.SuccessData(
                        200, "Cari hesap ekstresi bulunamadı", new List<ClCardStatementViewModel>());
                }

                // Group by client (should be single client since we filter by clCode)
                var groupedData = result.Data.Rows.Cast<DataRow>()
                    .GroupBy(row => new
                    {
                        LogicalRef = Convert.ToInt32(row["LogicalRef"]),
                        ClCode = row["ClCode"]?.ToString() ?? string.Empty,
                        ClDefinition = row["ClDefinition"]?.ToString() ?? string.Empty
                    })
                    .Select(g => new ClCardStatementViewModel
                    {
                        Logicalref = g.Key.LogicalRef,
                        ClCode = g.Key.ClCode,
                        ClDefinition = g.Key.ClDefinition,
                        Receipts = g.Select(row => new ClCardStatementReceiptsViewModel
                        {
                            ReceiptDate = row["ReceiptDate"] != DBNull.Value ? Convert.ToDateTime(row["ReceiptDate"]) : DateTime.MinValue,
                            ReceiptNo = row["ReceiptNo"]?.ToString() ?? string.Empty,
                            ReceiptTypeNo = row["ReceiptTypeNo"] != DBNull.Value ? Convert.ToInt32(row["ReceiptTypeNo"]) : 0,
                            ReceiptType = row["ReceiptType"]?.ToString() ?? string.Empty,
                            DOCODE = row["DOCODE"]?.ToString() ?? string.Empty,
                            LineExp = row["LineExp"]?.ToString() ?? string.Empty,
                            Debit = row["Debit"] != DBNull.Value ? Convert.ToDecimal(row["Debit"]) : 0,
                            Credit = row["Credit"] != DBNull.Value ? Convert.ToDecimal(row["Credit"]) : 0,
                            RowBalance = row["RowBalance"] != DBNull.Value ? Convert.ToDecimal(row["RowBalance"]) : 0,
                            CumulativeBalance = row["CumulativeBalance"] != DBNull.Value ? Convert.ToDecimal(row["CumulativeBalance"]) : 0
                        }).ToList()
                    })
                    .ToList();

                return ResponseDto<List<ClCardStatementViewModel>>.SuccessData(
                    200, "Cari hesap ekstresi başarıyla getirildi", groupedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClCardStatementAsync - Beklenmeyen hata");
                return ResponseDto<List<ClCardStatementViewModel>>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
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

        public async Task<ResponseDto<(string ClientCode, long ClientRef)>> GetClientInfoByWorkplaceCodeAsync(string workplaceCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workplaceCode))
                {
                    return ResponseDto<(string, long)>.FailData(400, "İşyeri kodu boş olamaz", "workplaceCode parametresi gereklidir", true);
                }

                var query = $@"
                    WITH RankedRecords AS (
                        SELECT
                            CL.CODE,
                            CL.LOGICALREF,
                            ROW_NUMBER() OVER (
                                PARTITION BY IC.CODE
                                ORDER BY
                                    CASE
                                        -- Rakam kontrolü (10-99) - En yüksek öncelik
                                        WHEN RIGHT(CL.CODE, 2) LIKE '[1-9][0-9]'
                                             AND CAST(RIGHT(CL.CODE, 2) AS INT) BETWEEN 10 AND 99
                                        THEN 5000 + CAST(RIGHT(CL.CODE, 2) AS INT)

                                        -- K kontrolü (K ile rakam) - İkinci öncelik
                                        WHEN LEFT(RIGHT(CL.CODE, 2), 1) = 'K'
                                             AND RIGHT(CL.CODE, 1) LIKE '[0-9]'
                                        THEN 4000 + CAST(RIGHT(CL.CODE, 1) AS INT)

                                        -- KR kontrolü - Üçüncü öncelik
                                        WHEN RIGHT(CL.CODE, 2) = 'KR' THEN 3000

                                        -- M[0-9] kontrolü - Dördüncü öncelik
                                        WHEN RIGHT(CL.CODE, 2) LIKE 'M[0-9]'
                                        THEN 2000 + CAST(RIGHT(CL.CODE, 1) AS INT)

                                        -- MS kontrolü - En düşük öncelik
                                        WHEN RIGHT(CL.CODE, 2) = 'MS' THEN 1000

                                        ELSE 0
                                    END DESC
                            ) AS RowNum
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS IC ON IC.LOGICALREF = CL.PARENTCLREF
                        WHERE
                            IC.CODE = '{workplaceCode.Replace("'", "''")}'
                            AND CL.CODE NOT LIKE '%DK0%'
                            AND CL.CODE NOT LIKE '%DK1%'
                            AND CL.CODE NOT LIKE '%KD%'
                            AND (CL.SPECODE NOT IN('KIRMIZI','YEŞİL','MAVİ') OR CL.SPECODE IS NULL)
                            AND LEFT(CL.CODE, 1) = '1'
                            AND CL.ACTIVE = 0
                    )
                    SELECT CODE, LOGICALREF
                    FROM RankedRecords
                    WHERE RowNum = 1";

                var result = _sqlProvider.SqlReader(query);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetClientInfoByWorkplaceCodeAsync - Sorgu hatası: {Message}, WorkplaceCode: {WorkplaceCode}",
                        result.Message, workplaceCode);
                    return ResponseDto<(string, long)>.FailData(500, "Cari bilgisi bulunamadı", result.Message, true);
                }

                if (result.Data == null || result.Data.Rows.Count == 0)
                {
                    _logger.LogWarning("GetClientInfoByWorkplaceCodeAsync - Cari bilgisi bulunamadı: {WorkplaceCode}", workplaceCode);
                    return ResponseDto<(string, long)>.FailData(404, "Cari bilgisi bulunamadı",
                        $"İşyeri kodu '{workplaceCode}' için cari hesap bilgisi bulunamadı", false);
                }

                var clientCode = result.Data.Rows[0]["CODE"]?.ToString() ?? string.Empty;
                var clientRef = result.Data.Rows[0]["LOGICALREF"] != DBNull.Value
                    ? Convert.ToInt64(result.Data.Rows[0]["LOGICALREF"])
                    : 0L;

                return ResponseDto<(string, long)>.SuccessData(200, "Cari bilgisi başarıyla getirildi", (clientCode, clientRef));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClientInfoByWorkplaceCodeAsync - Beklenmeyen hata: {WorkplaceCode}", workplaceCode);
                return ResponseDto<(string, long)>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<Dictionary<string, List<WorkplaceCurrentAccounts>>>> GetWorkplaceCurrentAccountsAsync()
        {
            try
            {
                var query = $@"
                    SELECT WP.CODE AS WPCODE, WP.DEFINITION_ AS WPADDRESS, CL.LOGICALREF AS CLREFFERANCE,
                           CL.CODE AS CLCODE, CL.DEFINITION_ AS CLCTITLE, CL.EMAILADDR AS CLMAIL
                    FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                    INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF = CL.PARENTCLREF
                    WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                      AND LEFT(TRIM(CL.CODE), 1) = '1'
                      AND CL.ACTIVE = 0
                    ORDER BY WP.CODE";

                var result = _sqlProvider.SqlReader(query);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetWorkplaceCurrentAccountsAsync - Sorgu hatası: {Message}", result.Message);
                    return ResponseDto<Dictionary<string, List<WorkplaceCurrentAccounts>>>.FailData(500, "Veri çekilemedi", result.Message, true);
                }

                var workplaceAccounts = new Dictionary<string, List<WorkplaceCurrentAccounts>>();

                if (result.Data != null && result.Data.Rows.Count > 0)
                {
                    foreach (DataRow row in result.Data.Rows)
                    {
                        var wpCode = row["WPCODE"]?.ToString() ?? string.Empty;

                        if (string.IsNullOrEmpty(wpCode))
                            continue;

                        var account = new WorkplaceCurrentAccounts
                        {
                            LogicalRef = row["CLREFFERANCE"] != DBNull.Value ? Convert.ToInt32(row["CLREFFERANCE"]) : 0,
                            Code = row["CLCODE"]?.ToString() ?? string.Empty,
                            Definition = row["CLCTITLE"]?.ToString() ?? string.Empty,
                            EmailAddress = row["CLMAIL"]?.ToString() ?? string.Empty
                        };

                        if (!workplaceAccounts.ContainsKey(wpCode))
                        {
                            workplaceAccounts[wpCode] = new List<WorkplaceCurrentAccounts>();
                        }

                        workplaceAccounts[wpCode].Add(account);
                    }
                }

                return ResponseDto<Dictionary<string, List<WorkplaceCurrentAccounts>>>.SuccessData(
                    200, "İşyeri cari hesapları başarıyla getirildi", workplaceAccounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWorkplaceCurrentAccountsAsync - Beklenmeyen hata");
                return ResponseDto<Dictionary<string, List<WorkplaceCurrentAccounts>>>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
        }
    }
}
