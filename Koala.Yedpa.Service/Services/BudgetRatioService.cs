using AutoMapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.UnitOfWorks;
using Koala.Yedpa.Repositories;
using Microsoft.Extensions.Logging;

namespace Koala.Yedpa.Service.Services;

public class BudgetRatioService : IBudgetRatioService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly IBudgetRatioRepository _budgetRatioRepository;
    private readonly ITransactionService _transactionService;
    private readonly ITransactionItemService _transactionItemService;
    private readonly IMapper _mapper;
    private readonly ILogger<BudgetRatioService> _logger;

    public BudgetRatioService(
        IUnitOfWork<AppDbContext> unitOfWork,
        IBudgetRatioRepository budgetRatioRepository,
        ITransactionService transactionService,
        ITransactionItemService transactionItemService,
        IMapper mapper,
        ILogger<BudgetRatioService> logger)
    {
        _unitOfWork = unitOfWork;
        _budgetRatioRepository = budgetRatioRepository;
        _transactionService = transactionService;
        _transactionItemService = transactionItemService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseDto<BudgetRatioDetailViewModel>> GetByIdAsync(string id)
    {
        try
        {
            _logger.LogDebug("Getting BudgetRatio by ID: {Id}", id);

            var budgetRatio = await _budgetRatioRepository.GetByIdAsync(id);
            if (budgetRatio == null)
            {
                _logger.LogWarning("BudgetRatio not found: {Id}", id);
                return ResponseDto<BudgetRatioDetailViewModel>.FailData(
                    404,
                    "BudgetRatio bulunamadı",
                    $"ID: {id}",
                    false
                );
            }

            var viewModel = _mapper.Map<BudgetRatioDetailViewModel>(budgetRatio);
            _logger.LogDebug("BudgetRatio retrieved successfully: {Id}", id);

            return ResponseDto<BudgetRatioDetailViewModel>.SuccessData(
                200,
                "BudgetRatio başarıyla getirildi",
                viewModel
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BudgetRatio by ID: {Id}", id);
            return ResponseDto<BudgetRatioDetailViewModel>.FailData(
                500,
                "BudgetRatio getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<BudgetRatioListViewModel>>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all BudgetRatios");

            var budgetRatios = await _budgetRatioRepository.GetAllAsync();
            var viewModels = _mapper.Map<List<BudgetRatioListViewModel>>(budgetRatios);

            _logger.LogDebug("Retrieved {Count} BudgetRatios", viewModels.Count);

            return ResponseDto<List<BudgetRatioListViewModel>>.SuccessData(
                200,
                "BudgetRatios başarıyla getirildi",
                viewModels
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all BudgetRatios");
            return ResponseDto<List<BudgetRatioListViewModel>>.FailData(
                500,
                "BudgetRatios getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<List<BudgetRatioListViewModel>>> GetByYearAsync(int year)
    {
        try
        {
            _logger.LogDebug("Getting BudgetRatios by year: {Year}", year);

            var budgetRatios = await _budgetRatioRepository.GetByYearAsync(year);
            var viewModels = _mapper.Map<List<BudgetRatioListViewModel>>(budgetRatios);

            _logger.LogDebug("Retrieved {Count} BudgetRatios for year {Year}", viewModels.Count, year);

            return ResponseDto<List<BudgetRatioListViewModel>>.SuccessData(
                200,
                "BudgetRatios başarıyla getirildi",
                viewModels
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BudgetRatios by year: {Year}", year);
            return ResponseDto<List<BudgetRatioListViewModel>>.FailData(
                500,
                "BudgetRatios getirme başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto<BudgetRatioDetailViewModel>> CreateAsync(CreateBudgetRatioViewModel model)
    {
        try
        {
            _logger.LogDebug("Creating BudgetRatio with Code: {Code}, Year: {Year}", model.Code, model.Year);

            // Check if record already exists
            var exists = await _budgetRatioRepository.ExistsAsync(model.Code, model.Year);
            if (exists)
            {
                _logger.LogWarning("BudgetRatio already exists: Code={Code}, Year={Year}", model.Code, model.Year);
                return ResponseDto<BudgetRatioDetailViewModel>.FailData(
                    400,
                    "Bu kod ve yıl için kayıt zaten mevcut",
                    $"Code: {model.Code}, Year: {model.Year}",
                    false
                );
            }

            // Create transaction for tracking
            var transactionModel = new CreateTransactionViewModel
            {
                TransactionTypeId = "BUDGET_RATIO_CREATE", // You may need to adjust this
                Title = $"BudgetRatio Oluşturma - {model.Code}",
                Description = $"BudgetRatio kaydı oluşturuldu: Code={model.Code}, Year={model.Year}"
            };

            var transactionResult = await _transactionService.CreateTransactionAsync(transactionModel);
            if (!transactionResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create transaction for BudgetRatio creation");
            }

            // Map ViewModel to Entity
            var budgetRatio = _mapper.Map<BudgetRatio>(model);
            budgetRatio.Id = Guid.NewGuid().ToString();
            budgetRatio.Status = StatusEnum.Active;
            budgetRatio.CreateTime = DateTime.UtcNow;

            var id = await _budgetRatioRepository.AddAsync(budgetRatio);
            await _unitOfWork.CommitAsync();

            // Add transaction item
            if (transactionResult.IsSuccess)
            {
                var transactionItemModel = new CreateTransactionItemViewModel
                {
                    TransactionId = transactionResult.Data.Id,
                    Description = $"BudgetRatio kaydı oluşturuldu: {budgetRatio.Id}",
                    IsSuccess = true
                };
                await _transactionItemService.AddTransactionItemAsync(transactionItemModel);
            }

            var createdBudgetRatio = await _budgetRatioRepository.GetByIdAsync(id);
            var viewModel = _mapper.Map<BudgetRatioDetailViewModel>(createdBudgetRatio);

            _logger.LogInformation("BudgetRatio created successfully: {Id}, Code: {Code}, Year: {Year}", 
                id, model.Code, model.Year);

            return ResponseDto<BudgetRatioDetailViewModel>.SuccessData(
                201,
                "BudgetRatio başarıyla oluşturuldu",
                viewModel
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating BudgetRatio: Code={Code}, Year={Year}", 
                model.Code, model.Year);
            return ResponseDto<BudgetRatioDetailViewModel>.FailData(
                500,
                "BudgetRatio oluşturma başarısız",
                ex.Message,
                true
            );
        }
    }

    public async Task<ResponseDto> UpdateAsync(UpdateBudgetRatioViewModel model)
    {
        try
        {
            _logger.LogDebug("Updating BudgetRatio: {Id}", model.Id);

            var budgetRatio = await _budgetRatioRepository.GetByIdAsync(model.Id);
            if (budgetRatio == null)
            {
                _logger.LogWarning("BudgetRatio not found for update: {Id}", model.Id);
                return ResponseDto.Fail(
                    404,
                    "BudgetRatio bulunamadı",
                    new ErrorDto($"ID: {model.Id}", false)
                );
            }

            // Create transaction for tracking
            var transactionModel = new CreateTransactionViewModel
            {
                TransactionTypeId = "BUDGET_RATIO_UPDATE",
                Title = $"BudgetRatio Güncelleme - {model.Code}",
                Description = $"BudgetRatio kaydı güncellendi: ID={model.Id}"
            };

            var transactionResult = await _transactionService.CreateTransactionAsync(transactionModel);

            // Update entity
            budgetRatio.Code = model.Code;
            budgetRatio.Description = model.Description;
            budgetRatio.Year = model.Year;
            budgetRatio.Ratio = model.Ratio;
            budgetRatio.TotalBugget = model.TotalBugget;
            budgetRatio.BuggetRatioMounths = model.BuggetRatioMounths;
            budgetRatio.BuggetType = model.BuggetType;
            budgetRatio.LastUpdateTime = DateTime.UtcNow;

            await _budgetRatioRepository.UpdateAsync(budgetRatio);
            await _unitOfWork.CommitAsync();

            // Add transaction item
            if (transactionResult.IsSuccess)
            {
                var transactionItemModel = new CreateTransactionItemViewModel
                {
                    TransactionId = transactionResult.Data.Id,
                    Description = $"BudgetRatio kaydı güncellendi: {model.Id}",
                    IsSuccess = true
                };
                await _transactionItemService.AddTransactionItemAsync(transactionItemModel);
            }

            _logger.LogInformation("BudgetRatio updated successfully: {Id}", model.Id);

            return ResponseDto.Success(200, "BudgetRatio başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating BudgetRatio: {Id}", model.Id);
            return ResponseDto.Fail(
                500,
                "BudgetRatio güncelleme başarısız",
                new ErrorDto(ex.Message, true)
            );
        }
    }

    public async Task<ResponseDto> DeleteAsync(string id)
    {
        try
        {
            _logger.LogDebug("Deleting BudgetRatio: {Id}", id);

            var budgetRatio = await _budgetRatioRepository.GetByIdAsync(id);
            if (budgetRatio == null)
            {
                _logger.LogWarning("BudgetRatio not found for delete: {Id}", id);
                return ResponseDto.Fail(
                    404,
                    "BudgetRatio bulunamadı",
                    new ErrorDto($"ID: {id}", false)
                );
            }

            // Create transaction for tracking
            var transactionModel = new CreateTransactionViewModel
            {
                TransactionTypeId = "BUDGET_RATIO_DELETE",
                Title = $"BudgetRatio Silme - {budgetRatio.Code}",
                Description = $"BudgetRatio kaydı silindi: ID={id}"
            };

            var transactionResult = await _transactionService.CreateTransactionAsync(transactionModel);

            await _budgetRatioRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            // Add transaction item
            if (transactionResult.IsSuccess)
            {
                var transactionItemModel = new CreateTransactionItemViewModel
                {
                    TransactionId = transactionResult.Data.Id,
                    Description = $"BudgetRatio kaydı silindi: {id}",
                    IsSuccess = true
                };
                await _transactionItemService.AddTransactionItemAsync(transactionItemModel);
            }

            _logger.LogInformation("BudgetRatio deleted successfully: {Id}", id);

            return ResponseDto.Success(200, "BudgetRatio başarıyla silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting BudgetRatio: {Id}", id);
            return ResponseDto.Fail(
                500,
                "BudgetRatio silme başarısız",
                new ErrorDto(ex.Message, true)
            );
        }
    }

    public async Task<ResponseDto<bool>> CheckExistsAsync(string code, int year)
    {
        try
        {
            _logger.LogDebug("Checking if BudgetRatio exists: Code={Code}, Year={Year}", code, year);

            var exists = await _budgetRatioRepository.ExistsAsync(code, year);

            return ResponseDto<bool>.SuccessData(
                200,
                "Kontrol başarıyla tamamlandı",
                exists
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking BudgetRatio existence: Code={Code}, Year={Year}", code, year);
            return ResponseDto<bool>.FailData(
                500,
                "Kontrol başarısız",
                ex.Message,
                true
            );
        }
    }
}





