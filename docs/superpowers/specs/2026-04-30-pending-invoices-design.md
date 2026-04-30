# PendingInvoices Endpoint Design

**Goal:** Add two endpoints to LogoClCardApiController that return pending (unpaid) invoices from Logo PAYTRANS table, with filtering support.

**Architecture:** Follows existing ApiLogoSqlDataService pattern. SQL query joins PAYTRANS, CLCARD, INVOICE tables with a subquery for paid amounts. Results are paginated via ROW_NUMBER() CTE. Two ViewModels map query output to typed responses.

**Tech Stack:** ASP.NET Core WebAPI, SQL Server (Logo ERP), ADO.NET via ISqlProvider

---

## Endpoints

| Endpoint | HTTP | Description |
|----------|------|-------------|
| `PendingInvoices` | GET | All pending invoices, paginated |
| `PendingInvoicesSearch` | POST | Filtered search (CustomerCode, CustomerName, DueDate range, InvoiceNumber) |

Both endpoints accept `perPage` (default 50) and `pageNo` (default 1) query parameters.
Both return `ResponseListDto<List<PendingInvoiceViewModel>>` with RecordsTotal, RecordsFiltered, RecordsShow.

## Data Models

### PendingInvoiceViewModel

Maps each row from the PAYTRANS query. **SQL column aliases use PascalCase English names to match C# property names** (required by `AsList<T>` reflection-based mapping, same pattern as `ClCardInfoViewModel` using `CARI_KODU` aliases).

| Property | Type | SQL Alias |
|----------|------|-----------|
| CustomerReference | int | `CLNTC.LOGICALREF AS CustomerReference` |
| CustomerCode | string | `CLNTC.CODE AS CustomerCode` |
| CustomerName | string | `CLNTC.DEFINITION_ AS CustomerName` |
| InvoiceLogicalRef | int | `PTRNS.FICHEREF AS InvoiceLogicalRef` |
| InvoiceNumber | string | `INVFC.FICHENO AS InvoiceNumber` |
| InvoiceDate | DateTime | `PTRNS.PROCDATE AS InvoiceDate` |
| InvoiceType | string | `CASE INVFC.TRCODE ... END AS InvoiceType` |
| InvoiceDescription1 | string | `INVFC.GENEXP1 AS InvoiceDescription1` |
| InvoiceDescription2 | string | `INVFC.GENEXP2 AS InvoiceDescription2` |
| InvoiceDueAmount | decimal | `PTRNS.TOTAL AS InvoiceDueAmount` |
| PaidAmount | decimal | `ISNULL(...) AS PaidAmount` |
| RemainingAmount | decimal | `(... - ...) AS RemainingAmount` |
| DueDate | DateTime | `PTRNS.DATE_ AS DueDate` |
| Month | int | `DATEPART(mm, PTRNS.DATE_) AS Month` |
| Week | int | `DATEPART(wk, PTRNS.DATE_) AS Week` |
| DueDays | int | `DATEDIFF(...) AS DueDays` |
| RemainingDays | int | `DATEDIFF(...) AS RemainingDays` |
| CurrencyType | string | `CASE PTRNS.TRCURR ... END AS CurrencyType` |
| Status | string | `CASE ... END AS Status` |

### PendingInvoiceSearchViewModel

| Property | Type | Filter Logic |
|----------|------|--------------|
| CustomerCode | string? | `CLNTC.CODE LIKE '%value%'` |
| CustomerName | string? | `CLNTC.DEFINITION_ LIKE '%value%'` |
| DueDateStart | DateTime? | `PTRNS.DATE_ >= value` |
| DueDateEnd | DateTime? | `PTRNS.DATE_ <= value` |
| InvoiceNumber | string? | `INVFC.FICHENO LIKE '%value%'` |

Status filter intentionally omitted: base WHERE clause `remaining > 0` already excludes fully paid invoices, leaving only ACIK and KISMI ODEME rows.

## Service Layer

Two new methods added to `IApiLogoSqlDataService`:

```csharp
Task<ResponseListDto<List<PendingInvoiceViewModel>>> GetPendingInvoicesAsync(int perPage, int pageNo);
Task<ResponseListDto<List<PendingInvoiceViewModel>>> SearchPendingInvoicesAsync(PendingInvoiceSearchViewModel searchModel, int perPage, int pageNo);
```

Implementation in `ApiLogoSqlDataService`:
- Table names use `LogoSetting.Firm` and `LogoSetting.Period` for `PAYTRANS` and `INVOICE` tables
- SQL uses `LG_{Firm}_{Period}_PAYTRANS`, `LG_{Firm}_CLCARD`, `LG_{Firm}_{Period}_INVOICE`
- KAPATILAN subquery groups CROSSREF to sum paid amounts
- WHERE filters: CROSSREF=0, CANCELLED=0, SIGN=1, FROMKASA=0, remaining > 0
- Pagination via ROW_NUMBER() CTE (same as ClCardInfoAll)
- Filtered version builds dynamic WHERE clauses from non-null search fields, appended to base query before CTE wrapping (same pattern as `WhereClCardInfoAsync`)
- Filter values use `.Replace("'", "''")` for SQL escaping (consistent with existing pattern)

## File Changes

| Action | File | Responsibility |
|--------|------|----------------|
| Create | `Core/Models/ViewModels/PendingInvoiceViewModel.cs` | Result model |
| Create | `Core/Models/ViewModels/PendingInvoiceSearchViewModel.cs` | Filter model |
| Modify | `Core/Services/IApiLogoSqlDataService.cs` | Add 2 method signatures |
| Modify | `Service/Services/ApiLogoSqlDataService.cs` | SQL query + 2 method implementations |
| Modify | `WebApi/Controllers/LogoClCardApiController.cs` | Add 2 endpoints |

ViewModels placed in `Core/Models/ViewModels/` following existing convention (where `ClCardInfoViewModel` lives).

## Flow

Controller -> IApiLogoSqlDataService -> ApiLogoSqlDataService -> ISqlProvider -> SQL Server
