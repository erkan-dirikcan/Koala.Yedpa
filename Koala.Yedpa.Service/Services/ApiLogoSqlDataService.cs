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
                    WITH NumberedClCard AS (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY CARI_KODU, DUKKAN_CARI_KODU) AS RowNum
                        FROM (
                            {query}
                        ) AS Base
                    )
                    SELECT * FROM NumberedClCard
                    WHERE RowNum BETWEEN {offset + 1} AND {offset + perPage}
                    ORDER BY RowNum";



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
                conditions.Add($"CLC.CODE LIKE '%{searchModel.CariKodu.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.CariUnvan))
                conditions.Add($"CLC.DEFINITION_ LIKE '%{searchModel.CariUnvan.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.VergiDairesi))
                conditions.Add($"CLC.TAXOFFICE LIKE '%{searchModel.VergiDairesi.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.VergiNumarasi))
                conditions.Add($"CLC.TAXNR LIKE '%{searchModel.VergiNumarasi.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Tckn))
                conditions.Add($"CLC.TCKNO LIKE '%{searchModel.Tckn.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Il))
                conditions.Add($"CLC.CITY LIKE '%{searchModel.Il.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Ilce))
                conditions.Add($"CLC.TOWN LIKE '%{searchModel.Ilce.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Mahalle))
                conditions.Add($"CLC.DISTRICT LIKE '%{searchModel.Mahalle.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Adres1))
                conditions.Add($"CLC.ADDR1 LIKE '%{searchModel.Adres1.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Adres2))
                conditions.Add($"CLC.ADDR2 LIKE '%{searchModel.Adres2.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod))
                conditions.Add($"CLC.SPECODE LIKE '%{searchModel.OzelKod.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod2))
                conditions.Add($"CLC.SPECODE2 LIKE '%{searchModel.OzelKod2.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod3))
                conditions.Add($"CLC.SPECODE3 LIKE '%{searchModel.OzelKod3.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod4))
                conditions.Add($"CLC.SPECODE4 LIKE '%{searchModel.OzelKod4.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.OzelKod5))
                conditions.Add($"CLC.SPECODE5 LIKE '%{searchModel.OzelKod5.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.DukkanCariKodu))
                conditions.Add($"CLP.CODE LIKE '%{searchModel.DukkanCariKodu.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.DukkanAdresOrjinal))
                conditions.Add($"CLP.DEFINITION_ LIKE '%{searchModel.DukkanAdresOrjinal.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Cadde))
                conditions.Add($"CADDE LIKE '%{searchModel.Cadde.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.No))
                conditions.Add($"NO LIKE '%{searchModel.No.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.PasajNo))
                conditions.Add($"PASAJ_NO LIKE '%{searchModel.PasajNo.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.YeniNo))
                conditions.Add($"YENI_NO LIKE '%{searchModel.YeniNo.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Kat))
                conditions.Add($"KAT LIKE '%{searchModel.Kat.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Yetkili1AdSoyad))
                conditions.Add($"CLC.INCHARGE LIKE '%{searchModel.Yetkili1AdSoyad.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.Yetkili2AdSoyad))
                conditions.Add($"CLC.INCHARGE2 LIKE '%{searchModel.Yetkili2AdSoyad.Replace("'", "''")}%'");

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
                                WITH NumberedClCard AS (
                                    SELECT *, ROW_NUMBER() OVER (ORDER BY CARI_KODU, DUKKAN_CARI_KODU) AS RowNum
                                    FROM (
                                        {query}
                                        {whereClause}
                                    ) AS Base
                                )
                                SELECT * FROM NumberedClCard
                                WHERE RowNum BETWEEN {offset + 1} AND {offset + perPage}
                                ORDER BY RowNum";


            var result = _sqlProvider.SqlReader(pagedQuery);
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
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TELNRS1)), ''), '') AS TELEFON_1,
                    ISNULL(NULLIF(LTRIM(RTRIM(CLC.TELNRS2)), ''), '') AS TELEFON_2,
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

        public async Task<ResponseDto<Dictionary<string, (string ClientCode, long ClientRef)>>> GetClientInfoByWorkplaceCodesAsync(List<string> workplaceCodes)
        {
            try
            {
                if (workplaceCodes == null || !workplaceCodes.Any())
                {
                    return ResponseDto<Dictionary<string, (string, long)>>.FailData(400, "İşyeri kodu listesi boş olamaz", "workplaceCodes parametresi gereklidir", true);
                }

                var codes = workplaceCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
                if (!codes.Any())
                {
                    return ResponseDto<Dictionary<string, (string, long)>>.SuccessData(200, "Boş liste", new Dictionary<string, (string, long)>());
                }

                var codeList = string.Join(",", codes.Select(c => $"'{c.Replace("'", "''")}'"));
                var query = $@"
                    WITH RankedRecords AS (
                        SELECT
                            CL.CODE,
                            CL.LOGICALREF,
                            IC.CODE AS IC_CODE,
                            ROW_NUMBER() OVER (
                                PARTITION BY IC.CODE
                                ORDER BY
                                    CASE
                                        WHEN RIGHT(CL.CODE, 2) LIKE '[1-9][0-9]'
                                             AND CAST(RIGHT(CL.CODE, 2) AS INT) BETWEEN 10 AND 99
                                        THEN 5000 + CAST(RIGHT(CL.CODE, 2) AS INT)

                                        WHEN LEFT(RIGHT(CL.CODE, 2), 1) = 'K'
                                             AND RIGHT(CL.CODE, 1) LIKE '[0-9]'
                                        THEN 4000 + CAST(RIGHT(CL.CODE, 1) AS INT)

                                        WHEN RIGHT(CL.CODE, 2) = 'KR' THEN 3000

                                        WHEN RIGHT(CL.CODE, 2) LIKE 'M[0-9]'
                                        THEN 2000 + CAST(RIGHT(CL.CODE, 1) AS INT)

                                        WHEN RIGHT(CL.CODE, 2) = 'MS' THEN 1000

                                        ELSE 0
                                    END DESC
                            ) AS RowNum
                        FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS IC ON IC.LOGICALREF = CL.PARENTCLREF
                        WHERE
                            IC.CODE IN ({codeList})
                            AND CL.CODE NOT LIKE '%DK0%'
                            AND CL.CODE NOT LIKE '%DK1%'
                            AND CL.CODE NOT LIKE '%KD%'
                            AND (CL.SPECODE NOT IN('KIRMIZI','YEŞİL','MAVİ') OR CL.SPECODE IS NULL)
                            AND LEFT(CL.CODE, 1) = '1'
                            AND CL.ACTIVE = 0
                    )
                    SELECT CODE, LOGICALREF, IC_CODE
                    FROM RankedRecords
                    WHERE RowNum = 1";

                var result = _sqlProvider.SqlReader(query);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetClientInfoByWorkplaceCodesAsync - Sorgu hatası: {Message}", result.Message);
                    return ResponseDto<Dictionary<string, (string, long)>>.FailData(500, "Cari bilgisi bulunamadı", result.Message, true);
                }

                var clientInfoDict = new Dictionary<string, (string ClientCode, long ClientRef)>(StringComparer.OrdinalIgnoreCase);

                if (result.Data != null)
                {
                    foreach (DataRow row in result.Data.Rows)
                    {
                        var icCode = row["IC_CODE"]?.ToString() ?? string.Empty;
                        var clientCode = row["CODE"]?.ToString() ?? string.Empty;
                        var clientRef = row["LOGICALREF"] != DBNull.Value ? Convert.ToInt64(row["LOGICALREF"]) : 0L;

                        if (!string.IsNullOrEmpty(icCode))
                        {
                            clientInfoDict[icCode] = (clientCode, clientRef);
                        }
                    }
                }

                return ResponseDto<Dictionary<string, (string, long)>>.SuccessData(200, $"Cari bilgisi başarıyla getirildi ({clientInfoDict.Count} kayıt)", clientInfoDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClientInfoByWorkplaceCodesAsync - Beklenmeyen hata");
                return ResponseDto<Dictionary<string, (string, long)>>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseDto<Dictionary<string, List<WorkplaceCurrentAccounts>>>> GetWorkplaceCurrentAccountsAsync()
        {
            try
            {
                var query=$@"SELECT WP.CODE AS WPCODE, 
                                    WP.DEFINITION_ AS WPADDRESS, 
                                    CL.LOGICALREF AS CLREFFERANCE,
                                    CL.CODE AS CLCODE, 
                                    CL.DEFINITION_ AS CLCTITLE, 
                                    CL.EMAILADDR AS CLMAIL
                             FROM LG_211_CLCARD AS CL
                             INNER JOIN LG_211_CLCARD AS WP ON WP.LOGICALREF = CL.PARENTCLREF
                             WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                               AND CL.CODE LIKE '1%'
                               AND CL.ACTIVE = 0
                             ORDER BY WP.CODE
                             ";
                //var query = $@"
                //    SELECT WP.CODE AS WPCODE, WP.DEFINITION_ AS WPADDRESS, CL.LOGICALREF AS CLREFFERANCE,
                //           CL.CODE AS CLCODE, CL.DEFINITION_ AS CLCTITLE, CL.EMAILADDR AS CLMAIL
                //    FROM LG_{LogoSetting.Firm}_CLCARD AS CL
                //    INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS WP ON WP.LOGICALREF = CL.PARENTCLREF
                //    WHERE CL.SPECODE NOT IN ('KIRMIZI','MAVİ','YEŞİL')
                //      AND LEFT(TRIM(CL.CODE), 1) = '1'
                //      AND CL.ACTIVE = 0
                //    ORDER BY WP.CODE";

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

        private string BuildBasePendingInvoiceQuery()
        {
            return $@"
        SELECT
            CLNTC.LOGICALREF        AS CustomerReference,
            CLNTC.CODE              AS CustomerCode,
            CLNTC.DEFINITION_       AS CustomerName,
            PTRNS.FICHEREF          AS InvoiceLogicalRef,
            INVFC.FICHENO           AS InvoiceNumber,
            PTRNS.PROCDATE          AS InvoiceDate,
            CASE INVFC.TRCODE
                WHEN 1  THEN 'Mal Alım Faturası'
                WHEN 2  THEN 'Perakende Satış İade Faturası'
                WHEN 3  THEN 'Toptan Satış İade Faturası'
                WHEN 4  THEN 'Alınan Hizmet Faturası'
                WHEN 5  THEN 'Alınan Proforma Fatura'
                WHEN 6  THEN 'Alım İade Faturası'
                WHEN 7  THEN 'Alım Fiyat Farkı Faturası'
                WHEN 8  THEN 'Perakende Satış Faturası'
                WHEN 9  THEN 'Toptan Satış Faturası'
                WHEN 10 THEN 'Verilen Hizmet Faturası'
                WHEN 11 THEN 'Verilen Proforma Fatura'
                WHEN 12 THEN 'Verilen Vade Farkı Faturası'
                WHEN 13 THEN 'Satış Fiyat Farkı Faturası'
                WHEN 14 THEN 'Satınalma Fiyat Farkı Faturası'
                WHEN 26 THEN 'Müstahsil Makbuzu'
                WHEN 32 THEN 'Alınan Fiyat Farkı Faturası'
                WHEN 33 THEN 'Verilen Fiyat Farkı Faturası'
                ELSE 'Diğer'
            END                     AS InvoiceType,
            INVFC.GENEXP1           AS InvoiceDescription1,
            INVFC.GENEXP2           AS InvoiceDescription2,
            PTRNS.TOTAL             AS InvoiceDueAmount,
            ISNULL(KAPATILAN.KAPANAN_TUTAR, 0) AS PaidAmount,
            (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) AS RemainingAmount,
            PTRNS.DATE_             AS DueDate,
            DATEPART(mm, PTRNS.DATE_) AS Month,
            DATEPART(wk, PTRNS.DATE_) AS Week,
            DATEDIFF(DAY, PTRNS.PROCDATE, PTRNS.DATE_) AS DueDays,
            DATEDIFF(DAY, GETDATE(), PTRNS.DATE_)      AS RemainingDays,
            CASE PTRNS.TRCURR
                WHEN 0  THEN 'TL'
                WHEN 1  THEN 'USD'
                WHEN 20 THEN 'EUR'
                ELSE ''
            END                     AS CurrencyType,
            CASE
                WHEN PTRNS.PAID = 0 THEN 'AÇIK'
                WHEN PTRNS.PAID = 1 AND (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) > 0 THEN 'KISMİ ÖDEME'
                ELSE 'KAPALI'
            END                     AS Status
        FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_PAYTRANS AS PTRNS
        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS CLNTC
            ON CLNTC.LOGICALREF = PTRNS.CARDREF
        LEFT OUTER JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_INVOICE AS INVFC
            ON INVFC.LOGICALREF = PTRNS.FICHEREF
        LEFT OUTER JOIN (
            SELECT
                CROSSREF,
                SUM(PAID) AS KAPANAN_TUTAR
            FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_PAYTRANS
            WHERE CROSSREF <> 0
              AND CANCELLED = 0
            GROUP BY CROSSREF
        ) AS KAPATILAN
            ON KAPATILAN.CROSSREF = PTRNS.LOGICALREF
        WHERE
            PTRNS.CROSSREF = 0
            AND PTRNS.CANCELLED = 0
            AND PTRNS.SIGN = 1
            AND ISNULL(INVFC.FROMKASA, 0) = 0
            AND (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) > 0";
        }

        public async Task<ResponseListDto<List<PendingInvoiceViewModel>>> GetPendingInvoicesAsync(int perPage = 50, int pageNo = 1)
        {
            if (perPage <= 0) perPage = 50;
            if (pageNo <= 0) pageNo = 1;
            var offset = (pageNo - 1) * perPage;

            var query = BuildBasePendingInvoiceQuery();

            var totalQuery = $@"
        SELECT COUNT(*)
        FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_PAYTRANS AS PTRNS
        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS CLNTC ON CLNTC.LOGICALREF = PTRNS.CARDREF
        LEFT OUTER JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_INVOICE AS INVFC ON INVFC.LOGICALREF = PTRNS.FICHEREF
        LEFT OUTER JOIN (
            SELECT CROSSREF, SUM(PAID) AS KAPANAN_TUTAR
            FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_PAYTRANS
            WHERE CROSSREF <> 0 AND CANCELLED = 0
            GROUP BY CROSSREF
        ) AS KAPATILAN ON KAPATILAN.CROSSREF = PTRNS.LOGICALREF
        WHERE PTRNS.CROSSREF = 0 AND PTRNS.CANCELLED = 0 AND PTRNS.SIGN = 1
          AND ISNULL(INVFC.FROMKASA, 0) = 0
          AND (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) > 0";

            var totalResult = _sqlProvider.SqlReader(totalQuery);
            var recordsTotal = totalResult.IsSuccess ? Convert.ToInt32(totalResult.Data.Rows[0][0]) : 0;

            var pagedQuery = $@"
        WITH NumberedInvoices AS (
            SELECT *, ROW_NUMBER() OVER (ORDER BY DueDate DESC) AS RowNum
            FROM ({query}) AS Base
        )
        SELECT * FROM NumberedInvoices
        WHERE RowNum BETWEEN {offset + 1} AND {offset + perPage}
        ORDER BY RowNum";

            var result = _sqlProvider.SqlReader(pagedQuery);
            if (!result.IsSuccess)
            {
                _logger.LogError("GetPendingInvoicesAsync - Hata: {Message}", result.Message);
                return ResponseListDto<List<PendingInvoiceViewModel>>.FailData(500, "Veri çekilemedi", result.Message, true);
            }

            var list = result.Data.AsList<PendingInvoiceViewModel>();

            return ResponseListDto<List<PendingInvoiceViewModel>>.SuccessData(
                200, "Bekleyen faturalar getirildi", list,
                RecordsTotal: recordsTotal,
                RecordsFiltered: recordsTotal,
                RecordsShow: list.Count
            );
        }

        public async Task<ResponseListDto<List<PendingInvoiceViewModel>>> SearchPendingInvoicesAsync(PendingInvoiceSearchViewModel searchModel, int perPage = 50, int pageNo = 1)
        {
            if (searchModel == null) searchModel = new PendingInvoiceSearchViewModel();
            if (perPage <= 0) perPage = 50;
            if (pageNo <= 0) pageNo = 1;
            var offset = (pageNo - 1) * perPage;

            var query = BuildBasePendingInvoiceQuery();

            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchModel.CustomerCode))
                conditions.Add($"CLNTC.CODE LIKE '%{searchModel.CustomerCode.Replace("'", "''")}%'");
            if (!string.IsNullOrWhiteSpace(searchModel.CustomerName))
                conditions.Add($"CLNTC.DEFINITION_ LIKE '%{searchModel.CustomerName.Replace("'", "''")}%'");
            if (searchModel.DueDateStart.HasValue)
                conditions.Add($"PTRNS.DATE_ >= '{searchModel.DueDateStart.Value:yyyy-MM-dd}'");
            if (searchModel.DueDateEnd.HasValue)
                conditions.Add($"PTRNS.DATE_ <= '{searchModel.DueDateEnd.Value:yyyy-MM-dd}'");
            if (!string.IsNullOrWhiteSpace(searchModel.InvoiceNumber))
                conditions.Add($"INVFC.FICHENO LIKE '%{searchModel.InvoiceNumber.Replace("'", "''")}%'");

            var whereClause = conditions.Any() ? " AND " + string.Join(" AND ", conditions) : "";

            // Filtresiz toplam
            var totalQuery = $@"
        SELECT COUNT(*)
        FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_PAYTRANS AS PTRNS
        INNER JOIN LG_{LogoSetting.Firm}_CLCARD AS CLNTC ON CLNTC.LOGICALREF = PTRNS.CARDREF
        LEFT OUTER JOIN LG_{LogoSetting.Firm}_{LogoSetting.Period}_INVOICE AS INVFC ON INVFC.LOGICALREF = PTRNS.FICHEREF
        LEFT OUTER JOIN (
            SELECT CROSSREF, SUM(PAID) AS KAPANAN_TUTAR
            FROM LG_{LogoSetting.Firm}_{LogoSetting.Period}_PAYTRANS
            WHERE CROSSREF <> 0 AND CANCELLED = 0
            GROUP BY CROSSREF
        ) AS KAPATILAN ON KAPATILAN.CROSSREF = PTRNS.LOGICALREF
        WHERE PTRNS.CROSSREF = 0 AND PTRNS.CANCELLED = 0 AND PTRNS.SIGN = 1
          AND ISNULL(INVFC.FROMKASA, 0) = 0
          AND (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) > 0";
            var totalResult = _sqlProvider.SqlReader(totalQuery);
            var recordsTotal = totalResult.IsSuccess ? Convert.ToInt32(totalResult.Data.Rows[0][0]) : 0;

            // Filtreli toplam
            var filteredCountQuery = query + whereClause;
            var countQuery = "SELECT COUNT(*) FROM (" + filteredCountQuery + ") AS T";
            var filteredResult = _sqlProvider.SqlReader(countQuery);
            var recordsFiltered = filteredResult.IsSuccess ? Convert.ToInt32(filteredResult.Data.Rows[0][0]) : 0;

            var pagedQuery = $@"
        WITH NumberedInvoices AS (
            SELECT *, ROW_NUMBER() OVER (ORDER BY DueDate DESC) AS RowNum
            FROM ({query} {whereClause}) AS Base
        )
        SELECT * FROM NumberedInvoices
        WHERE RowNum BETWEEN {offset + 1} AND {offset + perPage}
        ORDER BY RowNum";

            var result = _sqlProvider.SqlReader(pagedQuery);
            if (!result.IsSuccess)
            {
                _logger.LogError("SearchPendingInvoicesAsync - Sorgu hatası: {Message}", result.Message);
                return ResponseListDto<List<PendingInvoiceViewModel>>.FailData(500, "Arama sırasında hata oluştu", result.Message, true);
            }

            var list = result.Data.AsList<PendingInvoiceViewModel>();

            return ResponseListDto<List<PendingInvoiceViewModel>>.SuccessData(
                statusCode: 200,
                message: "Arama başarılı",
                data: list,
                RecordsTotal: recordsTotal,
                RecordsFiltered: recordsFiltered,
                RecordsShow: list.Count
            );
        }

        public async Task<ResponseDto<ClCardStatementDetailedViewModel>> GetClCardStatementDetailedAsync(string clCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clCode))
                {
                    return ResponseDto<ClCardStatementDetailedViewModel>.FailData(400, "Cari kodu boş olamaz", "clCode parametresi gereklidir", true);
                }

                var safeCode = clCode.Replace("'", "''");
                var f = LogoSetting.Firm;
                var p = LogoSetting.Period;

                var query = $@"
                    SELECT * FROM (
                        -- 1) Fatura satırları (KDV hariç)
                        SELECT
                            CLC.LOGICALREF AS CL_REF,
                            CLC.CODE AS CL_CODE,
                            CLC.DEFINITION_ AS CL_TITLE,
                            INV.DATE_ AS INVOICE_DATE,
                            INV.CAPIBLOCK_CREADEDDATE AS CDATE,
                            INV.FICHENO AS FICHE_NO,
                            CASE INV.TRCODE
                                WHEN 1 THEN 'SATINALMA FATURASI'
                                WHEN 2 THEN 'PERAKENDE SATIŞ İADE FATURASI'
                                WHEN 3 THEN 'TOPTAN SATIŞ İADE FATURASI'
                                WHEN 4 THEN 'ALINAN HİZMET FATURASI'
                                WHEN 6 THEN 'SATINALMA İADE FATURASI'
                                WHEN 7 THEN 'PERAKENDE SATIŞ FATURASI'
                                WHEN 8 THEN 'TOPTAN SATIŞ FATURASI'
                                WHEN 9 THEN 'VERİLEN HİZMET FATURASI'
                                WHEN 13 THEN 'SATINALMA FİYAT FARKI FATURASI'
                                WHEN 14 THEN 'SATIŞ FİYAT FARKI FATURASI'
                                WHEN 26 THEN 'MÜSTAHSİL MAKBUZU'
                                ELSE 'DİĞER'
                            END AS FICHE_TYPE,
                            '' AS CURR_STATUS,
                            INV.DOCODE AS DONUMBER,
                            INV.GENEXP1 AS DESCRIPTION,
                            CASE WHEN STL.LINETYPE NOT IN (4) THEN ITE.CODE
                                 WHEN STL.LINETYPE IN (4) THEN SRV.CODE
                            END AS ITEM_CODE,
                            CASE WHEN STL.LINETYPE NOT IN (4) THEN ITE.NAME
                                 WHEN STL.LINETYPE IN (4) THEN SRV.DEFINITION_
                            END AS ITEM_NAME,
                            STL.AMOUNT AS AMOUNT,
                            UNI.NAME AS UNIT,
                            STL.PRICE AS UNIT_PRICE,
                            INV.TOTALDISCOUNTS AS DISCOUNT_TOTAL,
                            INV.TOTALVAT AS TAX_TOTAL,
                            CASE WHEN STL.LINETYPE=1 THEN ((STL.TOTAL - STL.DISTCOST) + (STL.VATAMNT)) - (STL.VATAMNT)
                                 ELSE CASE WHEN INV.TRCODE IN (6,7,8,9,10) THEN (STL.TOTAL - STL.DISTCOST) + (STL.VATAMNT) ELSE 0 END
                            END AS DEPT,
                            CASE WHEN INV.TRCODE IN (1,4,5,2,3) THEN (STL.TOTAL - STL.DISTCOST) + (STL.VATAMNT) ELSE 0 END AS CREDIT,
                            CASE WHEN INV.TRCODE IN (6,7,8,9,10) THEN (STL.TOTAL - STL.DISTCOST) + (STL.VATAMNT) ELSE 0 END
                            - CASE WHEN INV.TRCODE IN (1,4,5,2,3) THEN (STL.TOTAL - STL.DISTCOST) + (STL.VATAMNT) ELSE 0 END AS BALANCE_TL,
                            STL.LINENET AS LINENET,
                            STL.TOTAL AS LINETOTAL,
                            STL.LINETYPE AS LINETYPE,
                            STL.VAT AS VAT,
                            CLF.TRRATE AS TR_EXP_RATE,
                            CLF.TRNET AS TR_EX_AMOUNT,
                            CLF.REPORTRATE AS REP_EX_RATE,
                            CASE WHEN CLF.SIGN = 0 THEN INV.REPORTNET ELSE 0 END AS REP_EX_DEPT,
                            CASE WHEN CLF.SIGN = 1 THEN INV.REPORTNET ELSE 0 END AS REP_EX_CREDIT,
                            (CASE WHEN CLF.SIGN = 0 THEN INV.REPORTNET ELSE 0 END
                             - CASE WHEN CLF.SIGN = 1 THEN INV.REPORTNET ELSE 0 END) AS REP_BALANCE,
                            CASE CLF.TRCURR
                                WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER'
                            END AS EXP_TYPE
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            LEFT OUTER JOIN LG_{f}_{p}_INVOICE AS INV ON CLF.SOURCEFREF = INV.LOGICALREF
                            LEFT OUTER JOIN LG_{f}_CLCARD AS CLC ON CLC.LOGICALREF = CLF.CLIENTREF
                            LEFT OUTER JOIN LG_{f}_{p}_STLINE AS STL ON STL.INVOICEREF = INV.LOGICALREF
                            LEFT OUTER JOIN LG_{f}_ITEMS AS ITE ON ITE.LOGICALREF = STL.STOCKREF
                            LEFT OUTER JOIN LG_{f}_SRVCARD AS SRV ON SRV.LOGICALREF = STL.STOCKREF
                            LEFT OUTER JOIN LG_{f}_UNITSETL AS UNI ON UNI.LOGICALREF = STL.UOMREF
                        WHERE CLF.MODULENR = 4 AND INV.CANCELLED = 0 AND CLF.CANCELLED = 0
                            AND STL.VATINC = 0 AND (STL.CANCELLED = 0 OR STL.CANCELLED IS NULL)

                        UNION ALL
                        -- 2) Fatura satırları (KDV dahil)
                        SELECT
                            CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                            INV.DATE_, INV.CAPIBLOCK_CREADEDDATE, INV.FICHENO,
                            CASE INV.TRCODE
                                WHEN 1 THEN 'SATINALMA FATURASI' WHEN 2 THEN 'PERAKENDE SATIŞ İADE FATURASI'
                                WHEN 3 THEN 'TOPTAN SATIŞ İADE FATURASI' WHEN 4 THEN 'ALINAN HİZMET FATURASI'
                                WHEN 6 THEN 'SATINALMA İADE FATURASI' WHEN 7 THEN 'PERAKENDE SATIŞ FATURASI'
                                WHEN 8 THEN 'TOPTAN SATIŞ FATURASI' WHEN 9 THEN 'VERİLEN HİZMET FATURASI'
                                WHEN 13 THEN 'SATINALMA FİYAT FARKI FATURASI' WHEN 14 THEN 'SATIŞ FİYAT FARKI FATURASI'
                                WHEN 26 THEN 'MÜSTAHSİL MAKBUZU' ELSE 'DİĞER'
                            END, '',
                            INV.DOCODE, INV.GENEXP1,
                            CASE WHEN STL.LINETYPE NOT IN (4) THEN ITE.CODE WHEN STL.LINETYPE IN (4) THEN SRV.CODE END,
                            CASE WHEN STL.LINETYPE NOT IN (4) THEN ITE.NAME WHEN STL.LINETYPE IN (4) THEN SRV.DEFINITION_ END,
                            STL.AMOUNT, UNI.NAME, STL.PRICE,
                            INV.TOTALDISCOUNTS, INV.TOTALVAT,
                            CASE WHEN INV.TRCODE IN (6,7,8,9,10) THEN (STL.TOTAL - STL.DISTCOST) ELSE 0 END,
                            CASE WHEN INV.TRCODE IN (1,4,5,2,3) THEN (STL.TOTAL - STL.DISTCOST) ELSE 0 END,
                            CASE WHEN INV.TRCODE IN (6,7,8,9,10) THEN (STL.TOTAL - STL.DISTCOST) ELSE 0 END
                            - CASE WHEN INV.TRCODE IN (1,4,5,2,3) THEN (STL.TOTAL - STL.DISTCOST) ELSE 0 END,
                            STL.LINENET, STL.TOTAL, STL.VAT,
                            CLF.TRRATE, CLF.TRNET, CLF.REPORTRATE,
                            CASE WHEN CLF.SIGN = 0 THEN INV.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN INV.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN INV.REPORTNET ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN INV.REPORTNET ELSE 0 END,
                            CASE CLF.TRCURR WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER' END
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            LEFT OUTER JOIN LG_{f}_{p}_INVOICE AS INV ON CLF.SOURCEFREF = INV.LOGICALREF
                            LEFT OUTER JOIN LG_{f}_CLCARD AS CLC ON CLC.LOGICALREF = CLF.CLIENTREF
                            LEFT OUTER JOIN LG_{f}_{p}_STLINE AS STL ON STL.INVOICEREF = INV.LOGICALREF
                            LEFT OUTER JOIN LG_{f}_ITEMS AS ITE ON ITE.LOGICALREF = STL.STOCKREF
                            LEFT OUTER JOIN LG_{f}_SRVCARD AS SRV ON SRV.LOGICALREF = STL.STOCKREF
                            LEFT OUTER JOIN LG_{f}_UNITSETL AS UNI ON UNI.LOGICALREF = STL.UOMREF
                        WHERE CLF.MODULENR = 4 AND INV.CANCELLED = 0 AND CLF.CANCELLED = 0 AND STL.VATINC = 1

                        UNION ALL
                        -- 3) Cari fişler (tahsilat/ödeme)
                        SELECT
                            CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                            CHE.DATE_, CHE.CAPIBLOCK_CREADEDDATE, CHE.FICHENO,
                            CASE CHE.TRCODE
                                WHEN 1 THEN 'NAKİT TAHSİLAT' WHEN 2 THEN 'NAKİT ÖDEME'
                                WHEN 3 THEN 'BORÇ DEKONTU' WHEN 4 THEN 'ALACAK DEKONTU'
                                WHEN 5 THEN 'VİRMAN FİŞİ' WHEN 6 THEN 'KUR FARKI FİŞİ'
                                WHEN 12 THEN 'ÖZEL FİŞ' WHEN 14 THEN 'AÇILIŞ FİŞİ'
                                WHEN 41 THEN 'VERİLEN VADE FARKI FATURASI' WHEN 42 THEN 'ALINAN VADE FARKI FATURASI'
                                WHEN 45 THEN 'VERİLEN SERBEST MESLEK MAKBUZU' WHEN 46 THEN 'ALINAN SERBEST MESLEK MAKBUZU'
                                WHEN 70 THEN 'KREDİ KARTI FİŞİ' WHEN 71 THEN 'KREDİ KARTI İADE FİŞİ'
                                WHEN 72 THEN 'FİRMA KREDİ KARTI FİŞİ' ELSE 'DİĞER'
                            END, '', CHE.DOCODE, CHE.GENEXP1,
                            '', '', '', '', '',
                            0, 0,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END AS DEPT,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END AS CREDIT,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            0, 0, 0,
                            CLF.TRRATE, CLF.TRNET, CLF.REPORTRATE,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE CLF.TRCURR WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER' END
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            LEFT OUTER JOIN LG_{f}_{p}_CLFICHE AS CHE ON CLF.SOURCEFREF = CHE.LOGICALREF
                            LEFT OUTER JOIN LG_{f}_CLCARD AS CLC ON CLC.LOGICALREF = CLF.CLIENTREF
                        WHERE CLF.MODULENR = 5 AND CHE.CANCELLED = 0 AND CLF.CANCELLED = 0

                        UNION ALL
                        -- 4) Banka işlemleri
                        SELECT
                            CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                            BNF.DATE_, BNF.CAPIBLOCK_CREADEDDATE, BNF.FICHENO,
                            CASE BNF.TRCODE
                                WHEN 1 THEN 'BANKA İŞLEM FİŞİ' WHEN 2 THEN 'BANKA VİRMAN FİŞİ'
                                WHEN 3 THEN 'GELEN HAVALE/EFT' WHEN 4 THEN 'GÖNDERİLEN HAVALE/EFT'
                                WHEN 5 THEN 'BANKA AÇILIŞ FİŞİ' WHEN 6 THEN 'BANKA KUR FARKI FİŞİ'
                                WHEN 16 THEN 'BANKA ALINAN HİZMET FATURASI' WHEN 17 THEN 'BANKA VERİLEN HİZMET FATURASI'
                                WHEN 18 THEN 'BANKADAN ÇEK ÖDEMESİ' WHEN 19 THEN 'BANKADAN SENET ÖDEMESİ'
                                ELSE 'DİĞER'
                            END, '', '', BNF.GENEXP1,
                            '', '', '', '', '',
                            0, 0,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            0, 0, 0,
                            CLF.TRRATE, CLF.TRNET, CLF.REPORTRATE,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE CLF.TRCURR WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER' END
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            INNER JOIN LG_{f}_{p}_BNFLINE AS BNL ON CLF.SOURCEFREF = BNL.LOGICALREF
                            INNER JOIN LG_{f}_CLCARD AS CLC ON CLF.CLIENTREF = CLC.LOGICALREF
                            INNER JOIN LG_{f}_{p}_BNFICHE AS BNF ON BNL.SOURCEFREF = BNF.LOGICALREF
                        WHERE CLF.MODULENR = 7 AND BNL.MODULENR = 7 AND BNF.CANCELLED = 0 AND CLF.CANCELLED = 0

                        UNION ALL
                        -- 5) Çek/Senet işlemleri
                        SELECT
                            CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                            CLF.DATE_, CLF.CAPIBLOCK_CREADEDDATE, CLF.TRANNO,
                            CASE CSR.TRCODE
                                WHEN 1 THEN 'ÇEK GİRİŞİ' WHEN 2 THEN 'SENET GİRİŞİ'
                                WHEN 3 THEN 'ÇEK ÇIKIŞ (CARİ HESABA)' WHEN 4 THEN 'SENET ÇIKIŞ (CARİ HESABA)'
                                WHEN 5 THEN 'ÇEK ÇIKIŞ (BANKA TAHSİL)' WHEN 6 THEN 'SENET ÇIKIŞ (BANKA TAHSİL)'
                                WHEN 7 THEN 'ÇEK ÇIKIŞ (BANKA TEMİNAT)' WHEN 8 THEN 'SENET ÇIKIŞ (BANKA TEMİNAT)'
                                WHEN 9 THEN 'İŞLEM BORDROSU (MÜŞTERİ ÇEKİ)' WHEN 10 THEN 'İŞLEM BORDROSU (MÜŞTERİ SENEDİ)'
                                WHEN 11 THEN 'İŞLEM BORDROSU (KENDİ ÇEKİMİZ)' WHEN 12 THEN 'İŞLEM BORDROSU (BORÇ SENEDİMİZ)'
                                ELSE 'DİĞER'
                            END,
                            CASE CSC.CURRSTAT
                                WHEN 1 THEN 'PORTFÖYDE' WHEN 2 THEN 'CİRO EDİLDİ' WHEN 3 THEN 'TEMİNATA VERİLDİ'
                                WHEN 4 THEN 'TAHSİLE VERİLDİ' WHEN 5 THEN 'TAHSİLE VERİLDİ (PROTESTOLU)'
                                WHEN 6 THEN 'İADE EDİLDİ' WHEN 7 THEN 'PROTESTO EDİLDİ' WHEN 8 THEN 'TAHSİL EDİLDİ'
                                WHEN 9 THEN 'KENDİ ÇEKİMİZ' WHEN 10 THEN 'BORÇ SENEDİMİZ' WHEN 11 THEN 'KARŞILIĞI YOK'
                                WHEN 12 THEN 'TAHSİL EDİLEMİYOR' WHEN 14 THEN 'PORTFÖYDE PROTESTOLU' ELSE 'DİĞER'
                            END,
                            CSR.DOCODE, CSR.GENEXP1,
                            '', '', '', '', '',
                            0, 0,
                            CASE WHEN CSC.DOC IN (3,4) THEN CSC.TRNET ELSE 0 END,
                            CASE WHEN CSC.DOC IN (1,2) THEN CSC.TRNET ELSE 0 END,
                            CASE WHEN CSC.DOC IN (3,4) THEN CSC.TRNET ELSE 0 END - CASE WHEN CSC.DOC IN (1,2) THEN CSC.TRNET ELSE 0 END,
                            0, 0, 0,
                            CLF.TRRATE, CLF.TRNET, CLF.REPORTRATE,
                            CASE WHEN CLF.SIGN = 0 THEN CSC.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CSC.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CSC.REPORTNET ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CSC.REPORTNET ELSE 0 END,
                            CASE CLF.TRCURR WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER' END
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            INNER JOIN LG_{f}_CLCARD AS CLC ON CLF.CLIENTREF = CLC.LOGICALREF
                            INNER JOIN LG_{f}_{p}_CSROLL AS CSR ON CLF.SOURCEFREF = CSR.LOGICALREF
                            INNER JOIN LG_{f}_{p}_CSTRANS AS CST ON CSR.LOGICALREF = CST.ROLLREF
                            INNER JOIN LG_{f}_{p}_CSCARD AS CSC ON CST.CSREF = CSC.LOGICALREF
                        WHERE CLF.MODULENR = 6 AND CLF.CANCELLED = 0 AND CSC.CANCELLED = 0

                        UNION ALL
                        -- 6) Kasa işlemleri
                        SELECT
                            CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                            CLF.DATE_, CLF.CAPIBLOCK_CREADEDDATE, CLF.TRANNO,
                            CASE
                                WHEN KSL.TRCODE IN (11) THEN 'CARİ HESAP TAHSİLAT'
                                WHEN KSL.TRCODE IN (12) THEN 'CARİ HESAP ÖDEME'
                                WHEN KSL.TRCODE IN (21,22) THEN 'BANKA'
                                WHEN KSL.TRCODE IN (31,32,33,34,35,36,37,38,39) THEN 'FATURA'
                                WHEN KSL.TRCODE IN (61,62,63,64) THEN 'ÇEK/SENET'
                                WHEN KSL.TRCODE IN (71,72,73,74) THEN 'KASA'
                            END, '', KSL.DOCODE, KSL.LINEEXP,
                            '', '', '', '', '',
                            0, 0,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            0, 0, 0,
                            CLF.TRRATE, CLF.TRNET, CLF.REPORTRATE,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE CLF.TRCURR WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER' END
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            INNER JOIN LG_{f}_{p}_KSLINES AS KSL ON CLF.LOGICALREF = KSL.TRANSREF
                            INNER JOIN LG_{f}_CLCARD AS CLC ON CLF.CLIENTREF = CLC.LOGICALREF
                        WHERE CLF.MODULENR = 10 AND KSL.CANCELLED = 0 AND CLF.CANCELLED = 0

                        UNION ALL
                        -- 7) Çek/Senet borç dekontu
                        SELECT
                            CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                            CLF.DATE_, CLF.CAPIBLOCK_CREADEDDATE, CLF.TRANNO,
                            'ÇEK/SENET BORÇ DEKONTU', '', '', CLF.LINEEXP,
                            '', '', '', '', '',
                            0, 0,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.AMOUNT ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.AMOUNT ELSE 0 END,
                            0, 0, 0,
                            CLF.TRRATE, CLF.TRNET, CLF.REPORTRATE,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE WHEN CLF.SIGN = 0 THEN CLF.REPORTNET ELSE 0 END - CASE WHEN CLF.SIGN = 1 THEN CLF.REPORTNET ELSE 0 END,
                            CASE CLF.TRCURR WHEN 0 THEN 'TL' WHEN 1 THEN 'USD' WHEN 20 THEN 'EURO' WHEN 17 THEN 'GBP' WHEN 160 THEN 'TL' ELSE 'DİĞER' END
                        FROM LG_{f}_{p}_CLFLINE AS CLF
                            INNER JOIN LG_{f}_CLCARD AS CLC ON CLF.CLIENTREF = CLC.LOGICALREF
                        WHERE CLF.MODULENR IN (61, 62) AND CLF.CANCELLED = 0
                    ) AS T
                    WHERE CL_CODE = '{safeCode}'
                    ORDER BY INVOICE_DATE, CDATE";

                var result = _sqlProvider.SqlReader(query);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetClCardStatementDetailedAsync - Sorgu hatası: {Message}", result.Message);
                    return ResponseDto<ClCardStatementDetailedViewModel>.FailData(500, "Veri çekilemedi", result.Message, true);
                }

                if (result.Data == null || result.Data.Rows.Count == 0)
                {
                    return ResponseDto<ClCardStatementDetailedViewModel>.SuccessData(
                        200, "Detaylı ekstre bulunamadı", new ClCardStatementDetailedViewModel());
                }

                // Parse flat rows and group by FICHE_NO
                var invoiceTypes = new HashSet<string>
                {
                    "SATINALMA FATURASI", "PERAKENDE SATIŞ İADE FATURASI", "TOPTAN SATIŞ İADE FATURASI",
                    "ALINAN HİZMET FATURASI", "SATINALMA İADE FATURASI", "PERAKENDE SATIŞ FATURASI",
                    "TOPTAN SATIŞ FATURASI", "VERİLEN HİZMET FATURASI", "SATINALMA FİYAT FARKI FATURASI",
                    "SATIŞ FİYAT FARKI FATURASI", "MÜSTAHSİL MAKBUZU", "AÇILIŞ FİŞİ",
                    "GELEN HAVALE/EFT"
                };

                var firstRow = result.Data.Rows[0];
                var retVal = new ClCardStatementDetailedViewModel
                {
                    LogicalRef = Convert.ToInt32(firstRow["CL_REF"]),
                    ClCode = firstRow["CL_CODE"]?.ToString() ?? string.Empty,
                    ClTitle = firstRow["CL_TITLE"]?.ToString() ?? string.Empty,
                };

                decimal cumulativeBalance = 0;
                var grouped = result.Data.Rows.Cast<DataRow>()
                    .GroupBy(row => row["FICHE_NO"]?.ToString() ?? string.Empty)
                    .ToList();

                foreach (var group in grouped)
                {
                    var header = group.First();
                    var ficheType = header["FICHE_TYPE"]?.ToString() ?? string.Empty;

                    var groupModel = new StatementDetailedGroupViewModel
                    {
                        Date = header["INVOICE_DATE"] != DBNull.Value ? Convert.ToDateTime(header["INVOICE_DATE"]) : DateTime.MinValue,
                        FicheNo = group.Key,
                        FicheType = ficheType,
                        Description = header["DESCRIPTION"]?.ToString() ?? string.Empty,
                        Donumber = header["DONUMBER"]?.ToString() ?? string.Empty,
                        ExpType = header["EXP_TYPE"]?.ToString() ?? string.Empty,
                        Discount = header["DISCOUNT_TOTAL"] != DBNull.Value ? Convert.ToDecimal(header["DISCOUNT_TOTAL"]) : 0,
                        TaxTotal = header["TAX_TOTAL"] != DBNull.Value ? Convert.ToDecimal(header["TAX_TOTAL"]) : 0,
                    };

                    if (invoiceTypes.Contains(ficheType))
                    {
                        foreach (var row in group)
                        {
                            var itemCode = row["ITEM_CODE"]?.ToString() ?? string.Empty;
                            if (string.IsNullOrEmpty(itemCode)) continue;

                            groupModel.Debit += row["DEPT"] != DBNull.Value ? Convert.ToDecimal(row["DEPT"]) : 0;
                            groupModel.Credit += row["CREDIT"] != DBNull.Value ? Convert.ToDecimal(row["CREDIT"]) : 0;

                            groupModel.Lines.Add(new StatementDetailedLineViewModel
                            {
                                ItemCode = itemCode,
                                ItemName = row["ITEM_NAME"]?.ToString() ?? string.Empty,
                                Amount = row["AMOUNT"] != DBNull.Value ? Convert.ToDecimal(row["AMOUNT"]) : 0,
                                Unit = row["UNIT"]?.ToString() ?? string.Empty,
                                UnitPrice = row["UNIT_PRICE"] != DBNull.Value ? Convert.ToDecimal(row["UNIT_PRICE"]) : 0,
                                LineNet = row["LINENET"] != DBNull.Value ? Convert.ToDecimal(row["LINENET"]) : 0,
                                LineTotal = row["LINETOTAL"] != DBNull.Value ? Convert.ToDecimal(row["LINETOTAL"]) : 0,
                                Vat = row["VAT"] != DBNull.Value ? Convert.ToDecimal(row["VAT"]) : 0,
                                LineType = row["LINETYPE"]?.ToString() ?? string.Empty,
                            });
                        }
                    }
                    else
                    {
                        groupModel.Debit = header["DEPT"] != DBNull.Value ? Convert.ToDecimal(header["DEPT"]) : 0;
                        groupModel.Credit = header["CREDIT"] != DBNull.Value ? Convert.ToDecimal(header["CREDIT"]) : 0;
                    }

                    cumulativeBalance += groupModel.Debit - groupModel.Credit;
                    groupModel.Balance = cumulativeBalance;
                    retVal.StatementGroups.Add(groupModel);
                }

                retVal.Balance = cumulativeBalance;

                return ResponseDto<ClCardStatementDetailedViewModel>.SuccessData(
                    200, "Detaylı cari ekstre başarıyla getirildi", retVal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetClCardStatementDetailedAsync - Beklenmeyen hata");
                return ResponseDto<ClCardStatementDetailedViewModel>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
        }

        public async Task<ResponseListDto<List<CustomerListWithBalanceViewModel>>> GetCustomerListWithBalanceAsync(string? specode2, int perPage = 50, int pageNo = 1)
        {
            try
            {
                if (perPage <= 0) perPage = 50;
                if (pageNo <= 0) pageNo = 1;
                var offset = (pageNo - 1) * perPage;
                var f = LogoSetting.Firm;
                var p = LogoSetting.Period;

                var specode2Filter = !string.IsNullOrWhiteSpace(specode2)
                    ? $"AND CLC.SPECODE2 = '{specode2.Replace("'", "''")}'"
                    : "";

                var query = $@"
                    SELECT
                        CLC.LOGICALREF AS LogicalRef,
                        CLC.CODE AS Code,
                        CLC.DEFINITION_ AS Definition,
                        ISNULL(CLC.CITY, '') AS City,
                        ISNULL(CLC.TOWN, '') AS Town,
                        ISNULL(CLC.DISTRICT, '') AS District,
                        ISNULL(CLC.ADDR1, '') AS Addr1,
                        ISNULL(CLC.ADDR2, '') AS Addr2,
                        CONVERT(DECIMAL(38, 2), SUM(GNCLTOT.DEBIT) - SUM(GNCLTOT.CREDIT)) AS Balance,
                        LS.LASTSALEDATE AS LastSaleDate,
                        LP.LASTPAYMENTDATE AS LastPaymentDate
                    FROM LV_{f}_{p}_GNTOTCL GNCLTOT WITH (NOLOCK)
                    INNER JOIN LG_{f}_CLCARD CLC WITH (NOLOCK) ON CLC.LOGICALREF = GNCLTOT.CARDREF
                    LEFT JOIN (
                        SELECT CLIENTREF, MAX(DATE_) AS LASTSALEDATE
                        FROM LG_{f}_{p}_STLINE
                        WHERE TRCODE IN (7, 8)
                        GROUP BY CLIENTREF
                    ) LS ON LS.CLIENTREF = CLC.LOGICALREF
                    LEFT JOIN (
                        SELECT CLIENTREF, MAX(DATE_) AS LASTPAYMENTDATE
                        FROM LG_{f}_{p}_CLFLINE
                        WHERE TRCODE IN (1)
                        GROUP BY CLIENTREF
                    ) LP ON LP.CLIENTREF = CLC.LOGICALREF
                    WHERE GNCLTOT.TOTTYP = 1
                      AND CLC.CODE IS NOT NULL
                      AND CLC.ACTIVE = 0
                      {specode2Filter}
                    GROUP BY
                        CLC.LOGICALREF, CLC.CODE, CLC.DEFINITION_,
                        CLC.CITY, CLC.TOWN, CLC.DISTRICT, CLC.ADDR1, CLC.ADDR2,
                        LS.LASTSALEDATE, LP.LASTPAYMENTDATE
                    ORDER BY CLC.DEFINITION_";

                var totalQuery = $@"
                    SELECT COUNT(*)
                    FROM LV_{f}_{p}_GNTOTCL GNCLTOT WITH (NOLOCK)
                    INNER JOIN LG_{f}_CLCARD CLC WITH (NOLOCK) ON CLC.LOGICALREF = GNCLTOT.CARDREF
                    WHERE GNCLTOT.TOTTYP = 1
                      AND CLC.CODE IS NOT NULL
                      AND CLC.ACTIVE = 0
                      {specode2Filter}";

                var totalResult = _sqlProvider.SqlReader(totalQuery);
                var recordsTotal = totalResult.IsSuccess ? Convert.ToInt32(totalResult.Data.Rows[0][0]) : 0;

                var pagedQuery = $@"
                    WITH NumberedCustomers AS (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY Definition) AS RowNum
                        FROM ({query}) AS Base
                    )
                    SELECT * FROM NumberedCustomers
                    WHERE RowNum BETWEEN {offset + 1} AND {offset + perPage}
                    ORDER BY RowNum";

                var result = _sqlProvider.SqlReader(pagedQuery);
                if (!result.IsSuccess)
                {
                    _logger.LogError("GetCustomerListWithBalanceAsync - Sorgu hatası: {Message}", result.Message);
                    return ResponseListDto<List<CustomerListWithBalanceViewModel>>.FailData(500, "Veri çekilemedi", result.Message, true);
                }

                var list = result.Data.AsList<CustomerListWithBalanceViewModel>();

                return ResponseListDto<List<CustomerListWithBalanceViewModel>>.SuccessData(
                    200, "Cari hesap listesi başarıyla getirildi", list,
                    RecordsTotal: recordsTotal,
                    RecordsFiltered: recordsTotal,
                    RecordsShow: list.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomerListWithBalanceAsync - Beklenmeyen hata");
                return ResponseListDto<List<CustomerListWithBalanceViewModel>>.FailData(500, "Beklenmeyen bir hata oluştu", ex.Message, true);
            }
        }
    }
}
