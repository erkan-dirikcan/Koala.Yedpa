using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IUnitOfWork<AppDbContext> unitOfWork,
        ILogger<TransactionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResponseDto<Transaction>> CreateTransactionAsync(CreateTransactionViewModel model)
    {
        try
        {
            var transaction = new Transaction
            {
                TransactionNumber = $"TXN-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                TransactionTypeId = model.TransactionTypeId,
                UserId = model.UserId,
                Title = model.Title,
                Description = model.Description,
                IsComplated = false
            };

            await _unitOfWork.TransactionRepository.AddAsync(transaction);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction created: {TransactionId} - {Title}", transaction.Id, model.Title);

            return ResponseDto<Transaction>.SuccessData(201, "Transaction başarıyla oluşturuldu.", transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction oluşturma hatası: {Title}", model.Title);
            return ResponseDto<Transaction>.FailData(500, "Transaction oluşturma başarısız.", ex.Message, false);
        }
    }

    public async Task<ResponseDto<TransactionDetailViewModel>> GetTransactionAsync(string transactionId)
    {
        try
        {
            var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return ResponseDto<TransactionDetailViewModel>.FailData(404, "Transaction bulunamadı.", $"ID: {transactionId}", false);
            }

            var viewModel = new TransactionDetailViewModel
            {
                Id = transaction.Id,
                TransactionNumber = transaction.TransactionNumber,
                TransactionTypeId = transaction.TransactionTypeId,
                TransactionTypeName = transaction.TransactionType?.Name,
                UserId = transaction.UserId,
                UserName = transaction.AppUser?.UserName,
                Title = transaction.Title,
                Description = transaction.Description,
                IsCompleted = transaction.IsComplated,
                CreateTime = transaction.CreateTime,
                UpdateTime = transaction.UpdateTime,
                TransactionItems = transaction.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            };

            return ResponseDto<TransactionDetailViewModel>.SuccessData(200, "Transaction başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction getirme hatası: {TransactionId}", transactionId);
            return ResponseDto<TransactionDetailViewModel>.FailData(500, "Transaction getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetAllTransactionsAsync()
    {
        try
        {
            var transactions = await _unitOfWork.TransactionRepository.GetAllAsync();
            var viewModels = transactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Tüm transaction'lar başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tüm transaction'ları getirme hatası");
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Transaction listesi getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetUserTransactionsAsync(string userId)
    {
        try
        {
            var transactions = await _unitOfWork.TransactionRepository.GetByUserIdAsync(userId);
            var viewModels = transactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Kullanıcı transaction'ları başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı transaction'ları getirme hatası: {UserId}", userId);
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Kullanıcı transaction'ları getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetTransactionsByTypeAsync(string transactionTypeId)
    {
        try
        {
            var transactions = await _unitOfWork.TransactionRepository.GetByTransactionTypeIdAsync(transactionTypeId);
            var viewModels = transactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Transaction type'a göre kayıtlar başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction type getirme hatası: {TransactionTypeId}", transactionTypeId);
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Transaction type kayıtları getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateTransactionAsync(UpdateTransactionViewModel model)
    {
        try
        {
            var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(model.TransactionId);
            if (transaction == null)
            {
                return ResponseDto.Fail(404, "Transaction bulunamadı.", new ErrorDto($"ID: {model.TransactionId}", false));
            }

            transaction.Title = model.Title;
            transaction.Description = model.Description;
            transaction.UpdateTime = DateTime.UtcNow;

            await _unitOfWork.TransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction güncellendi: {TransactionId}", model.TransactionId);

            return ResponseDto.Success(200, "Transaction başarıyla güncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction güncelleme hatası: {TransactionId}", model.TransactionId);
            return ResponseDto.Fail(500, "Transaction güncelleme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto> CompleteTransactionAsync(CompleteTransactionViewModel model)
    {
        try
        {
            var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(model.TransactionId);
            if (transaction == null)
            {
                return ResponseDto.Fail(404, "Transaction bulunamadı.", new ErrorDto($"ID: {model.TransactionId}", false));
            }

            transaction.IsComplated = true;
            transaction.UpdateTime = DateTime.UtcNow;
            await _unitOfWork.TransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction tamamlandı: {TransactionId}", model.TransactionId);

            return ResponseDto.Success(200, "Transaction başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction tamamlama hatası: {TransactionId}", model.TransactionId);
            return ResponseDto.Fail(500, "Transaction tamamlama başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto> DeleteTransactionAsync(DeleteTransactionViewModel model)
    {
        try
        {
            var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(model.TransactionId);
            if (transaction == null)
            {
                return ResponseDto.Fail(404, "Transaction bulunamadı.", new ErrorDto($"ID: {model.TransactionId}", false));
            }

            _unitOfWork.TransactionRepository.Delete(transaction);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction silindi: {TransactionId}", model.TransactionId);

            return ResponseDto.Success(200, "Transaction başarıyla silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction silme hatası: {TransactionId}", model.TransactionId);
            return ResponseDto.Fail(500, "Transaction silme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetPendingTransactionsAsync()
    {
        try
        {
            var transactions = await _unitOfWork.TransactionRepository.GetPendingTransactionsAsync();
            var viewModels = transactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Bekleyen transaction'lar başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bekleyen transaction'ları getirme hatası");
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Bekleyen transaction'lar getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetCompletedTransactionsAsync()
    {
        try
        {
            var transactions = await _unitOfWork.TransactionRepository.GetCompletedTransactionsAsync();
            var viewModels = transactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Tamamlanan transaction'lar başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tamamlanan transaction'ları getirme hatası");
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Tamamlanan transaction'lar getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<int>> GetTransactionCountAsync()
    {
        try
        {
            var count = await _unitOfWork.TransactionRepository.CountAsync();
            return ResponseDto<int>.SuccessData(200, "Transaction sayısı başarıyla getirildi.", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction sayısı getirme hatası");
            return ResponseDto<int>.FailData(500, "Transaction sayısı getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var transactions = await _unitOfWork.TransactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
            var viewModels = transactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Tarih aralığındaki transaction'lar başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tarih aralığına göre transaction getirme hatası: {StartDate} - {EndDate}", startDate, endDate);
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Tarih aralığına göre transaction getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> GetRecentTransactionsAsync(int count = 10)
    {
        try
        {
            var allTransactions = await _unitOfWork.TransactionRepository.GetAllAsync();
            var recentTransactions = allTransactions.Take(count).ToList();
            var viewModels = recentTransactions.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, $"Son {count} transaction başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Son transaction'ları getirme hatası");
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Son transaction'lar getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionListViewModel>>> SearchTransactionsAsync(TransactionSearchViewModel model)
    {
        try
        {
            var query = await _unitOfWork.TransactionRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(model.UserId))
            {
                query = query.Where(t => t.UserId == model.UserId).ToList();
            }

            if (!string.IsNullOrEmpty(model.TransactionTypeId))
            {
                query = query.Where(t => t.TransactionTypeId == model.TransactionTypeId).ToList();
            }

            if (model.IsCompleted.HasValue)
            {
                query = query.Where(t => t.IsComplated == model.IsCompleted.Value).ToList();
            }

            if (model.StartDate.HasValue)
            {
                query = query.Where(t => t.CreateTime >= model.StartDate.Value).ToList();
            }

            if (model.EndDate.HasValue)
            {
                query = query.Where(t => t.CreateTime <= model.EndDate.Value).ToList();
            }

            // Apply pagination
            var paginatedQuery = query
                .Skip((model.PageIndex - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            var viewModels = paginatedQuery.Select(t => new TransactionListViewModel
            {
                Id = t.Id,
                TransactionNumber = t.TransactionNumber,
                TransactionTypeId = t.TransactionTypeId,
                TransactionTypeName = t.TransactionType?.Name,
                UserId = t.UserId,
                UserName = t.AppUser?.UserName,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsComplated,
                CreateTime = t.CreateTime,
                UpdateTime = t.UpdateTime,
                TransactionItems = t.TransactionItems.Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                }).ToList()
            }).ToList();

            return ResponseDto<List<TransactionListViewModel>>.SuccessData(200, "Transaction arama sonuçları başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction arama hatası");
            return ResponseDto<List<TransactionListViewModel>>.FailData(500, "Transaction arama başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddTransactionItemAsync(CreateTransactionItemViewModel model)
    {
        try
        {
            var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(model.TransactionId);
            if (transaction == null)
            {
                return ResponseDto.Fail(404, "Transaction bulunamadı.", new ErrorDto($"ID: {model.TransactionId}", false));
            }

            var transactionItem = new TransactionItem
            {
                TransactionId = model.TransactionId,
                Description = model.Description,
                IsSuccess = model.IsSuccess,
                CreateTime = DateTime.UtcNow
            };

            transaction.TransactionItems.Add(transactionItem);

            _logger.LogInformation("Transaction item eklendi: {TransactionId} - {Description}", model.TransactionId, model.Description);

            return ResponseDto.Success(200, "Transaction item başarıyla eklendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction item ekleme hatası: {TransactionId}", model.TransactionId);
            return ResponseDto.Fail(500, "Transaction item ekleme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto<List<TransactionItemViewModel>>> GetTransactionItemsAsync(string transactionId)
    {
        try
        {
            var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return ResponseDto<List<TransactionItemViewModel>>.FailData(404, "Transaction bulunamadı.", $"ID: {transactionId}", false);
            }

            var items = transaction.TransactionItems
                .OrderByDescending(t => t.CreateTime)
                .Select(ti => new TransactionItemViewModel
                {
                    Id = ti.Id,
                    TransactionId = ti.TransactionId,
                    Description = ti.Description,
                    IsSuccess = ti.IsSuccess,
                    CreateTime = ti.CreateTime,
                    UpdateTime = ti.UpdateTime
                })
                .ToList();

            return ResponseDto<List<TransactionItemViewModel>>.SuccessData(200, "Transaction items başarıyla getirildi.", items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction items getirme hatası: {TransactionId}", transactionId);
            return ResponseDto<List<TransactionItemViewModel>>.FailData(500, "Transaction items getirme başarısız.", ex.Message, true);
        }
    }
}



