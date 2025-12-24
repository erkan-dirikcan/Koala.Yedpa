using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class TransactionTypeService : ITransactionTypeService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ILogger<TransactionTypeService> _logger;

    public TransactionTypeService(
        IUnitOfWork<AppDbContext> unitOfWork,
        ILogger<TransactionTypeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ResponseDto<TransactionTypeViewModel>> CreateTransactionTypeAsync(CreateTransactionTypeViewModel model)
    {
        try
        {
            // Check if name already exists
            var existingType = await _unitOfWork.TransactionTypeRepository.GetByNameAsync(model.Name);
            if (existingType != null)
            {
                return ResponseDto<TransactionTypeViewModel>.FailData(400, "Bu isimde bir transaction type zaten mevcut.", model.Name, false);
            }

            var transactionType = new TransactionType
            {
                Name = model.Name,
                Description = model.Description,
                ColorClass = model.ColorClass,
                Icon = model.Icon,
                Status = model.Status
            };

            await _unitOfWork.TransactionTypeRepository.AddAsync(transactionType);
            await _unitOfWork.CommitAsync();

            var viewModel = new TransactionTypeViewModel
            {
                Id = transactionType.Id,
                Name = transactionType.Name,
                Description = transactionType.Description,
                ColorClass = transactionType.ColorClass,
                Icon = transactionType.Icon,
                Status = transactionType.Status,
                CreateTime = transactionType.CreateTime,
                UpdateTime = transactionType.UpdateTime,
                TransactionCount = 0
            };

            _logger.LogInformation("Transaction type oluşturuldu: {TypeId} - {Name}", transactionType.Id, model.Name);

            return ResponseDto<TransactionTypeViewModel>.SuccessData(201, "Transaction type başarıyla oluşturuldu.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction type oluşturma hatası: {Name}", model.Name);
            return ResponseDto<TransactionTypeViewModel>.FailData(500, "Transaction type oluşturma başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<TransactionTypeViewModel>> GetTransactionTypeAsync(string typeId)
    {
        try
        {
            var transactionType = await _unitOfWork.TransactionTypeRepository.GetByIdAsync(typeId);
            if (transactionType == null)
            {
                return ResponseDto<TransactionTypeViewModel>.FailData(404, "Transaction type bulunamadı.", $"ID: {typeId}", false);
            }

            var viewModel = new TransactionTypeViewModel
            {
                Id = transactionType.Id,
                Name = transactionType.Name,
                Description = transactionType.Description,
                ColorClass = transactionType.ColorClass,
                Icon = transactionType.Icon,
                Status = transactionType.Status,
                CreateTime = transactionType.CreateTime,
                UpdateTime = transactionType.UpdateTime,
                TransactionCount = transactionType.Transactions?.Count ?? 0
            };

            return ResponseDto<TransactionTypeViewModel>.SuccessData(200, "Transaction type başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction type getirme hatası: {TypeId}", typeId);
            return ResponseDto<TransactionTypeViewModel>.FailData(500, "Transaction type getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionTypeViewModel>>> GetAllTransactionTypesAsync()
    {
        try
        {
            var transactionTypes = await _unitOfWork.TransactionTypeRepository.GetAllAsync();
            var viewModels = transactionTypes.Select(tt => new TransactionTypeViewModel
            {
                Id = tt.Id,
                Name = tt.Name,
                Description = tt.Description,
                ColorClass = tt.ColorClass,
                Icon = tt.Icon,
                Status = tt.Status,
                CreateTime = tt.CreateTime,
                UpdateTime = tt.UpdateTime,
                TransactionCount = tt.Transactions?.Count ?? 0
            }).ToList();

            return ResponseDto<List<TransactionTypeViewModel>>.SuccessData(200, "Tüm transaction types başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tüm transaction types getirme hatası");
            return ResponseDto<List<TransactionTypeViewModel>>.FailData(500, "Transaction types listesi getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionTypeViewModel>>> GetActiveTransactionTypesAsync()
    {
        try
        {
            var transactionTypes = await _unitOfWork.TransactionTypeRepository.GetActiveTransactionTypesAsync();
            var viewModels = transactionTypes.Select(tt => new TransactionTypeViewModel
            {
                Id = tt.Id,
                Name = tt.Name,
                Description = tt.Description,
                ColorClass = tt.ColorClass,
                Icon = tt.Icon,
                Status = tt.Status,
                CreateTime = tt.CreateTime,
                UpdateTime = tt.UpdateTime,
                TransactionCount = tt.Transactions?.Count ?? 0
            }).ToList();

            return ResponseDto<List<TransactionTypeViewModel>>.SuccessData(200, "Aktif transaction types başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif transaction types getirme hatası");
            return ResponseDto<List<TransactionTypeViewModel>>.FailData(500, "Aktif transaction types getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto> UpdateTransactionTypeAsync(UpdateTransactionTypeViewModel model)
    {
        try
        {
            var transactionType = await _unitOfWork.TransactionTypeRepository.GetByIdAsync(model.Id);
            if (transactionType == null)
            {
                return ResponseDto.Fail(404, "Transaction type bulunamadı.", new ErrorDto($"ID: {model.Id}", false));
            }

            // Check if name conflicts with other records
            var existingType = await _unitOfWork.TransactionTypeRepository.GetByNameAsync(model.Name);
            if (existingType != null && existingType.Id != model.Id)
            {
                return ResponseDto.Fail(400, "Bu isimde başka bir transaction type zaten mevcut.", new ErrorDto(model.Name, false));
            }

            transactionType.Name = model.Name;
            transactionType.Description = model.Description;
            transactionType.ColorClass = model.ColorClass;
            transactionType.Icon = model.Icon;
            transactionType.Status = model.Status;
            transactionType.UpdateTime = DateTime.UtcNow;

            await _unitOfWork.TransactionTypeRepository.UpdateAsync(transactionType);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction type güncellendi: {TypeId} - {Name}", model.Id, model.Name);

            return ResponseDto.Success(200, "Transaction type başarıyla güncellendi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction type güncelleme hatası: {TypeId}", model.Id);
            return ResponseDto.Fail(500, "Transaction type güncelleme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto> DeleteTransactionTypeAsync(string typeId)
    {
        try
        {
            var transactionType = await _unitOfWork.TransactionTypeRepository.GetByIdAsync(typeId);
            if (transactionType == null)
            {
                return ResponseDto.Fail(404, "Transaction type bulunamadı.", new ErrorDto($"ID: {typeId}", false));
            }

            // Check if there are related transactions
            if (transactionType.Transactions != null && transactionType.Transactions.Any())
            {
                return ResponseDto.Fail(400, "Bu transaction type ile ilişkili transaction kayıtları var. Önce onları silmelisiniz.", new ErrorDto($"İlişkili transaction sayısı: {transactionType.Transactions.Count}", false));
            }

            _unitOfWork.TransactionTypeRepository.Delete(transactionType);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Transaction type silindi: {TypeId} - {Name}", typeId, transactionType.Name);

            return ResponseDto.Success(200, "Transaction type başarıyla silindi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction type silme hatası: {TypeId}", typeId);
            return ResponseDto.Fail(500, "Transaction type silme başarısız.", new ErrorDto(ex.Message, true));
        }
    }

    public async Task<ResponseDto<TransactionTypeViewModel>> GetByNameAsync(string name)
    {
        try
        {
            var transactionType = await _unitOfWork.TransactionTypeRepository.GetByNameAsync(name);
            if (transactionType == null)
            {
                return ResponseDto<TransactionTypeViewModel>.FailData(404, "Transaction type bulunamadı.", $"Name: {name}", false);
            }

            var viewModel = new TransactionTypeViewModel
            {
                Id = transactionType.Id,
                Name = transactionType.Name,
                Description = transactionType.Description,
                ColorClass = transactionType.ColorClass,
                Icon = transactionType.Icon,
                Status = transactionType.Status,
                CreateTime = transactionType.CreateTime,
                UpdateTime = transactionType.UpdateTime,
                TransactionCount = transactionType.Transactions?.Count ?? 0
            };

            return ResponseDto<TransactionTypeViewModel>.SuccessData(200, "Transaction type başarıyla getirildi.", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction type isme göre getirme hatası: {Name}", name);
            return ResponseDto<TransactionTypeViewModel>.FailData(500, "Transaction type getirme başarısız.", ex.Message, true);
        }
    }

    public async Task<ResponseDto<List<TransactionTypeViewModel>>> GetByStatusAsync(StatusEnum status)
    {
        try
        {
            var transactionTypes = await _unitOfWork.TransactionTypeRepository.GetByStatusAsync(status);
            var viewModels = transactionTypes.Select(tt => new TransactionTypeViewModel
            {
                Id = tt.Id,
                Name = tt.Name,
                Description = tt.Description,
                ColorClass = tt.ColorClass,
                Icon = tt.Icon,
                Status = tt.Status,
                CreateTime = tt.CreateTime,
                UpdateTime = tt.UpdateTime,
                TransactionCount = tt.Transactions?.Count ?? 0
            }).ToList();

            return ResponseDto<List<TransactionTypeViewModel>>.SuccessData(200, "Statüye göre transaction types başarıyla getirildi.", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Statüye göre transaction types getirme hatası: {Status}", status);
            return ResponseDto<List<TransactionTypeViewModel>>.FailData(500, "Transaction types getirme başarısız.", ex.Message, true);
        }
    }
}



