# PendingInvoices Endpoint Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add PendingInvoices and PendingInvoicesSearch endpoints to LogoClCardApiController, returning unpaid invoices from Logo PAYTRANS table with pagination and filtering.

**Architecture:** Follows existing ApiLogoSqlDataService pattern. SQL query joins PAYTRANS, CLCARD, INVOICE tables with a subquery for paid amounts. Results paginated via ROW_NUMBER() CTE. Two new ViewModels added to the existing ApiLogoViewModels.cs file.

**Tech Stack:** ASP.NET Core WebAPI, SQL Server (Logo ERP), ADO.NET via ISqlProvider, Swagger annotations

**Spec:** `docs/superpowers/specs/2026-04-30-pending-invoices-design.md`

---

## Chunk 1: ViewModels and Interface

### Task 1: Add PendingInvoiceViewModel

**Files:**
- Modify: `Koala.Yedpa.Core/Models/ViewModels/ApiLogoViewModels.cs`

- [ ] **Step 1: Add PendingInvoiceViewModel class**

Add the following class at the end of the `Koala.Yedpa.Core.Models.ViewModels` namespace, before the closing `}` of the namespace block (after `ClCardStatementReceiptsViewModel`):

```csharp
public class PendingInvoiceViewModel
{
    [SwaggerSchema(Description = "Cari kart referans numarası")]
    public int CustomerReference { get; set; }

    [SwaggerSchema(Description = "Müşteri kodu")]
    public string CustomerCode { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Müşteri adı")]
    public string CustomerName { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Fatura LogicalRef")]
    public int InvoiceLogicalRef { get; set; }

    [SwaggerSchema(Description = "Fatura numarası")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Fatura tarihi")]
    public DateTime InvoiceDate { get; set; }

    [SwaggerSchema(Description = "Fatura türü")]
    public string InvoiceType { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Fatura açıklama 1")]
    public string InvoiceDescription1 { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Fatura açıklama 2")]
    public string InvoiceDescription2 { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Fatura vade tutarı")]
    public decimal InvoiceDueAmount { get; set; }

    [SwaggerSchema(Description = "Kapatılan tutar")]
    public decimal PaidAmount { get; set; }

    [SwaggerSchema(Description = "Kalan ödenecek tutar")]
    public decimal RemainingAmount { get; set; }

    [SwaggerSchema(Description = "Vade tarihi")]
    public DateTime DueDate { get; set; }

    [SwaggerSchema(Description = "Ay")]
    public int Month { get; set; }

    [SwaggerSchema(Description = "Hafta")]
    public int Week { get; set; }

    [SwaggerSchema(Description = "Vade gün sayısı")]
    public int DueDays { get; set; }

    [SwaggerSchema(Description = "Kalan gün sayısı")]
    public int RemainingDays { get; set; }

    [SwaggerSchema(Description = "Döviz türü")]
    public string CurrencyType { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Ödeme durumu")]
    public string Status { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Add PendingInvoiceSearchViewModel class**

Add after PendingInvoiceViewModel, still inside the namespace:

```csharp
public class PendingInvoiceSearchViewModel
{
    [SwaggerSchema(Description = "Müşteri kodu ile arama")]
    public string? CustomerCode { get; set; }

    [SwaggerSchema(Description = "Müşteri adı ile arama")]
    public string? CustomerName { get; set; }

    [SwaggerSchema(Description = "Vade tarihi başlangıç")]
    public DateTime? DueDateStart { get; set; }

    [SwaggerSchema(Description = "Vade tarihi bitiş")]
    public DateTime? DueDateEnd { get; set; }

    [SwaggerSchema(Description = "Fatura numarası ile arama")]
    public string? InvoiceNumber { get; set; }
}
```

- [ ] **Step 3: Commit**

```bash
git add Koala.Yedpa.Core/Models/ViewModels/ApiLogoViewModels.cs
git commit -m "feat: add PendingInvoiceViewModel and PendingInvoiceSearchViewModel"
```

---

### Task 2: Update IApiLogoSqlDataService interface

**Files:**
- Modify: `Koala.Yedpa.Core/Services/IApiLogoSqlDataService.cs`

- [ ] **Step 1: Add two new method signatures**

Add these two lines before the closing `}` of the interface, after the `GetWorkplaceCurrentAccountsAsync` method:

```csharp
Task<ResponseListDto<List<PendingInvoiceViewModel>>> GetPendingInvoicesAsync(int perPage, int pageNo);
Task<ResponseListDto<List<PendingInvoiceViewModel>>> SearchPendingInvoicesAsync(PendingInvoiceSearchViewModel searchModel, int perPage, int pageNo);
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.Core/Services/IApiLogoSqlDataService.cs
git commit -m "feat: add PendingInvoices method signatures to IApiLogoSqlDataService"
```

---

## Chunk 2: Service Implementation

### Task 3: Implement BuildBasePendingInvoiceQuery

**Files:**
- Modify: `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`

- [ ] **Step 1: Add BuildBasePendingInvoiceQuery private method**

Add this private method to `ApiLogoSqlDataService` class, after the existing `BuildBaseClCardQuery` method (around line 516):

```csharp
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
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs
git commit -m "feat: add BuildBasePendingInvoiceQuery SQL builder method"
```

---

### Task 4: Implement GetPendingInvoicesAsync

**Files:**
- Modify: `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`

- [ ] **Step 1: Add GetPendingInvoicesAsync method**

Add this method to `ApiLogoSqlDataService` class, after `GetWorkplaceCurrentAccountsAsync`:

```csharp
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
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs
git commit -m "feat: implement GetPendingInvoicesAsync with pagination"
```

---

### Task 5: Implement SearchPendingInvoicesAsync

**Files:**
- Modify: `Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs`

- [ ] **Step 1: Add SearchPendingInvoicesAsync method**

Add this method after `GetPendingInvoicesAsync`:

```csharp
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
```

- [ ] **Step 2: Commit**

```bash
git add Koala.Yedpa.Service/Services/ApiLogoSqlDataService.cs
git commit -m "feat: implement SearchPendingInvoicesAsync with dynamic filters"
```

---

## Chunk 3: Controller Endpoints

### Task 6: Add PendingInvoices and PendingInvoicesSearch endpoints

**Files:**
- Modify: `Koala.Yedpa.WebApi/Controllers/LogoClCardApiController.cs`

- [ ] **Step 1: Add using for new ViewModels**

The `Koala.Yedpa.Core.Models.ViewModels` namespace is already imported (line 2). No new using needed.

- [ ] **Step 2: Add PendingInvoices GET endpoint**

Add after the existing `Test()` method (after line 106), before the closing `}` of the class:

```csharp
/// <summary>
/// Bekleyen (ödenecek) faturaları sayfalı getirir
/// </summary>
[HttpGet("PendingInvoices")]
[SwaggerOperation(Summary = "Bekleyen faturaları sayfalı getirir")]
[SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<PendingInvoiceViewModel>>))]
[SwaggerResponse(401, "Yetkisiz erişim")]
[SwaggerResponse(500, "Sunucu hatası")]
public async Task<IActionResult> PendingInvoices(
    [FromQuery] int perPage = 50,
    [FromQuery] int pageNo = 1)
{
    _logger.LogInformation("PendingInvoices called with perPage={PerPage}, pageNo={PageNo}", perPage, pageNo);
    var result = await _service.GetPendingInvoicesAsync(perPage, pageNo);

    if (result.IsSuccess)
    {
        _logger.LogInformation("PendingInvoices: Successfully retrieved pending invoices");
    }
    else
    {
        _logger.LogWarning("PendingInvoices: Failed to retrieve, StatusCode: {StatusCode}", result.StatusCode);
    }

    return StatusCode(result.StatusCode, result);
}
```

- [ ] **Step 3: Add PendingInvoicesSearch POST endpoint**

Add after the `PendingInvoices` method:

```csharp
/// <summary>
/// Gelişmiş arama - bekleyen faturalarda filtreleme yapar
/// </summary>
[HttpPost("PendingInvoicesSearch")]
[SwaggerOperation(Summary = "Filtreleme ile bekleyen fatura araması")]
[SwaggerResponse(200, "Başarılı", typeof(ResponseListDto<List<PendingInvoiceViewModel>>))]
[SwaggerResponse(400, "Geçersiz istek")]
[SwaggerResponse(401, "Yetkisiz erişim")]
[SwaggerResponse(500, "Sunucu hatası")]
public async Task<IActionResult> PendingInvoicesSearch(
    [FromBody] PendingInvoiceSearchViewModel searchModel,
    [FromQuery] int perPage = 50,
    [FromQuery] int pageNo = 1)
{
    _logger.LogInformation("PendingInvoicesSearch called with perPage={PerPage}, pageNo={PageNo}", perPage, pageNo);
    if (searchModel == null)
    {
        _logger.LogWarning("PendingInvoicesSearch: Search model is null");
        return BadRequest(ResponseListDto<List<PendingInvoiceViewModel>>.FailData(
            400, "Arama modeli boş olamaz", "Model null", true));
    }

    var result = await _service.SearchPendingInvoicesAsync(searchModel, perPage, pageNo);

    if (result.IsSuccess)
    {
        _logger.LogInformation("PendingInvoicesSearch: Successfully completed search");
    }
    else
    {
        _logger.LogWarning("PendingInvoicesSearch: Search failed, StatusCode: {StatusCode}", result.StatusCode);
    }

    return StatusCode(result.StatusCode, result);
}
```

- [ ] **Step 4: Build and verify compilation**

Run: `dotnet build Koala.Yedpa.WebApi/Koala.Yedpa.WebApi.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 5: Commit**

```bash
git add Koala.Yedpa.WebApi/Controllers/LogoClCardApiController.cs
git commit -m "feat: add PendingInvoices and PendingInvoicesSearch endpoints"
```

---

### Task 7: Final verification and push

- [ ] **Step 1: Full solution build**

Run: `dotnet build`
Expected: Build succeeded, 0 errors

- [ ] **Step 2: Push to remote**

```bash
git push origin master
```
