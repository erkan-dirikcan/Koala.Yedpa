using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;

namespace Koala.Yedpa.Core.Services
{
    /// <summary>
    /// Bütçe ve sipariş oluşturma servisi interface'i
    /// </summary>
    public interface IBudgetOrderService
    {
        /// <summary>
        /// Bütçe oluştur VE sipariş gönder
        /// </summary>
        /// <param name="model">Bütçe ve sipariş oluşturma modeli</param>
        /// <returns>Bütçe ve sipariş oluşturma sonucu</returns>
        Task<ResponseDto<BudgetOrderResultViewModel>> CreateBudgetAndOrdersAsync(CreateBudgetOrderViewModel model);

        /// <summary>
        /// Sadece bütçe oluştur (sipariş gönderme)
        /// </summary>
        /// <param name="model">Bütçe oluşturma modeli</param>
        /// <returns>Oluşturulan bütçe kayıtları</returns>
        Task<ResponseDto<List<DuesStatisticListViewModel>>> CreateBudgetAsync(CreateBudgetOrderViewModel model);

        /// <summary>
        /// Mevcut bütçe kayıtları için sipariş oluştur
        /// </summary>
        /// <param name="budgetRatioId">Bütçe oranı ID</param>
        /// <param name="selectedMonths">Seçilen aylar</param>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>Sipariş sonuçları</returns>
        Task<ResponseDto<List<OrderResultViewModel>>> CreateOrdersForExistingBudgetAsync(string budgetRatioId, List<int> selectedMonths, string? userId = null);

        /// <summary>
        /// Bütçe hesapla (preview)
        /// </summary>
        /// <param name="request">Hesaplama isteği</param>
        /// <returns>Hesaplanan bütçe verileri</returns>
        Task<ResponseDto<BudgetCalculationResultViewModel>> CalculateBudgetAsync(BudgetCalculationRequestViewModel request);
    }
}
