using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class TransactionItemService : ITransactionItemService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ILogger<TransactionItemService> _logger;

    public TransactionItemService(
        IUnitOfWork<AppDbContext> unitOfWork,
        ILogger<TransactionItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResponseDto<TransactionItemViewModel>> GetTransactionItemAsync(string itemId)
    {
        try
        {
            var item = await _unitOfWork.TransactionItemRepository.GetByIdAsync(itemId);
            if (item == null)
            {
                return ResponseDto<TransactionItemViewModel>.FailData(404, "Transaction item bulunamadı.", $"ID: {itemId}", false);
            }

            var viewModel = new TransactionItemViewModel
            {
                Id = item.Id,
                TransactionId = item.TransactionId ?? string.Empty,
                Description = item.Description ?? string.Empty,
                IsSuccess = item.IsSuccess,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime
            };

            return ResponseDto<TransactionItemViewModel>.SuccessData(200, "Transaction item başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction item getirme hatası: {ItemId}", itemId);
            return ResponseDto<TransactionItemViewModel>.FailData(500, "Transaction item getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionItemViewModel>>> GetTransactionItemsAsync(string transactionId)
    {
        try
        {
            var items = await _unitOfWork.TransactionItemRepository.GetByTransactionIdAsync(transactionId);
            var viewModels = items.Select(item => new TransactionItemViewModel
            {
                Id = item.Id,
                TransactionId = item.TransactionId ?? string.Empty,
                Description = item.Description ?? string.Empty,
                IsSuccess = item.IsSuccess,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime
            }).ToList();

            return ResponseDto<List<TransactionItemViewModel>>.SuccessData(200, "Transaction items başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction items getirme hatası: {TransactionId}", transactionId);
            return ResponseDto<List<TransactionItemViewModel>>.FailData(500, "Transaction items getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionItemViewModel>>> GetAllTransactionItemsAsync()
    {
        try
        {
            var items = await _unitOfWork.TransactionItemRepository.GetAllAsync();
            var viewModels = items.Select(item => new TransactionItemViewModel
            {
                Id = item.Id,
                TransactionId = item.TransactionId ?? string.Empty,
                Description = item.Description ?? string.Empty,
                IsSuccess = item.IsSuccess,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime
            }).ToList();

            return ResponseDto<List<TransactionItemViewModel>>.SuccessData(200, "Tüm transaction items başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tüm transaction items getirme hatası");
            return ResponseDto<List<TransactionItemViewModel>>.FailData(500, "Transaction items listesi getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto> AddTransactionItemAsync(CreateTransactionItemViewModel model)
    {
        try
        {
            var transactionItem = new TransactionItem
            {
                TransactionId = model.TransactionId,
                Description = model.Description,
                IsSuccess = model.IsSuccess,
                CreateTime = DateTime.UtcNow
            };

            await _unitOfWork.TransactionItemRepository.AddAsync(transactionItem);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction item eklendi: {TransactionId} - {Description}", model.TransactionId, model.Description);

            return ResponseDto.Success(200, "Transaction item başarıyla eklendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction item ekleme hatası: {TransactionId}", model.TransactionId);
            return ResponseDto.Fail(500, "Transaction item ekleme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto> DeleteTransactionItemAsync(string itemId)
    {
        try
        {
            var item = await _unitOfWork.TransactionItemRepository.GetByIdAsync(itemId);
            if (item == null)
            {
                return ResponseDto.Fail(404, "Transaction item bulunamadı.", new ErrorDto($"ID: {itemId}", false));
            }

            _unitOfWork.TransactionItemRepository.Delete(item);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction item silindi: {ItemId}", itemId);

            return ResponseDto.Success(200, "Transaction item başarıyla silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction item silme hatası: {ItemId}", itemId);
            return ResponseDto.Fail(500, "Transaction item silme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto<List<TransactionItemViewModel>>> GetItemsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var items = await _unitOfWork.TransactionItemRepository.GetItemsByDateRangeAsync(startDate, endDate);
            var viewModels = items.Select(item => new TransactionItemViewModel
            {
                Id = item.Id,
                TransactionId = item.TransactionId ?? string.Empty,
                Description = item.Description ?? string.Empty,
                IsSuccess = item.IsSuccess,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime
            }).ToList();

            return ResponseDto<List<TransactionItemViewModel>>.SuccessData(200, "Tarih aralığındaki transaction items başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tarih aralığına göre transaction items getirme hatası: {StartDate} - {EndDate}", startDate, endDate);
            return ResponseDto<List<TransactionItemViewModel>>.FailData(500, "Tarih aralığına göre transaction items getirme başarısız.", ex.Message, true);
        }
    }
}


