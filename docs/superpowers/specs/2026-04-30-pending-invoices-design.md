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

Maps each row from the PAYTRANS query:

| Property | Type | SQL Source |
|----------|------|------------|
| CustomerReference | int | CLNTC.LOGICALREF |
| CustomerCode | string | CLNTC.CODE |
| CustomerName | string | CLNTC.DEFINITION_ |
| InvoiceLogicalRef | int | PTRNS.FICHEREF |
| InvoiceNumber | string | INVFC.FICHENO |
| InvoiceDate | DateTime | PTRNS.PROCDATE |
| InvoiceType | string | CASE INVFC.TRCODE ... END |
| InvoiceDescription1 | string | INVFC.GENEXP1 |
| InvoiceDescription2 | string | INVFC.GENEXP2 |
| InvoiceDueAmount | decimal | PTRNS.TOTAL |
| PaidAmount | decimal | ISNULL(KAPATILAN.KAPANAN_TUTAR, 0) |
| RemainingAmount | decimal | PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0) |
| DueDate | DateTime | PTRNS.DATE_ |
| Month | int | DATEPART(mm, PTRNS.DATE_) |
| Week | int | DATEPART(wk, PTRNS.DATE_) |
| DueDays | int | DATEDIFF(DAY, PTRNS.PROCDATE, PTRNS.DATE_) |
| RemainingDays | int | DATEDIFF(DAY, GETDATE(), PTRNS.DATE_) |
| CurrencyType | string | CASE PTRNS.TRCURR ... END |
| Status | string | PAID/PaidAmount/Remaining logic |

### PendingInvoiceSearchViewModel

| Property | Type | Filter Logic |
|----------|------|--------------|
| CustomerCode | string? | CLNTC.CODE LIKE '%value%' |
| CustomerName | string? | CLNTC.DEFINITION_ LIKE '%value%' |
| DueDateStart | DateTime? | PTRNS.DATE_ >= value |
| DueDateEnd | DateTime? | PTRNS.DATE_ <= value |
| InvoiceNumber | string? | INVFC.FICHENO LIKE '%value%' |

## Service Layer

Two new methods added to `IApiLogoSqlDataService`:

```csharp
Task<ResponseListDto<List<PendingInvoiceViewModel>>> GetPendingInvoicesAsync(int perPage, int pageNo);
Task<ResponseListDto<List<PendingInvoiceViewModel>>> SearchPendingInvoicesAsync(PendingInvoiceSearchViewModel searchModel, int perPage, int pageNo);
```

Implementation in `ApiLogoSqlDataService`:
- SQL uses `LG_{Firm}_{Period}_PAYTRANS`, `LG_{Firm}_CLCARD`, `LG_{Firm}_{Period}_INVOICE`
- KAPATILAN subquery groups CROSSREF to sum paid amounts
- WHERE filters: CROSSREF=0, CANCELLED=0, SIGN=1, FROMKASA=0, remaining > 0
- Pagination via ROW_NUMBER() CTE (same as ClCardInfoAll)
- Filtered version builds dynamic WHERE clauses from non-null search fields

## File Changes

| Action | File | Responsibility |
|--------|------|----------------|
| Create | Core/Dtos/PendingInvoiceViewModel.cs | Result model |
| Create | Core/Dtos/PendingInvoiceSearchViewModel.cs | Filter model |
| Modify | Core/Services/IApiLogoSqlDataService.cs | Add 2 method signatures |
| Modify | Service/Services/ApiLogoSqlDataService.cs | SQL query + 2 method implementations |
| Modify | WebApi/Controllers/LogoClCardApiController.cs | Add 2 endpoints |

## Flow

Controller -> IApiLogoSqlDataService -> ApiLogoSqlDataService -> ISqlProvider -> SQL Server
