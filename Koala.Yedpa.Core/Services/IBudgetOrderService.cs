using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
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

        /// <summary>
        /// Yeni BudgetRatio oluştur (SaveNewBudget için)
        /// </summary>
        /// <param name="budgetRatio">BudgetRatio nesnesi</param>
        /// <returns>Oluşturma sonucu</returns>
        Task<ResponseDto<bool>> CreateBudgetRatioAsync(BudgetRatio budgetRatio);

        /// <summary>
        /// Yeni DuesStatistic oluştur (SaveNewBudget için)
        /// </summary>
        /// <param name="duesStatistic">DuesStatistic nesnesi</param>
        /// <returns>Oluşturma sonucu</returns>
        Task<ResponseDto<bool>> CreateDuesStatisticAsync(DuesStatistic duesStatistic);

        /// <summary>
        /// BudgetRatio güncelle (TotalBugget güncellemesi için)
        /// </summary>
        /// <param name="budgetRatio">BudgetRatio nesnesi</param>
        /// <returns>Güncelleme sonucu</returns>
        Task<ResponseDto<bool>> UpdateBudgetRatioAsync(BudgetRatio budgetRatio);

        /// <summary>
        /// BudgetRatio getir (ID ile)
        /// </summary>
        /// <param name="id">BudgetRatio ID</param>
        /// <returns>BudgetRatio detayları</returns>
        Task<ResponseDto<BudgetRatio>> GetBudgetRatioByIdAsync(string id);

        /// <summary>
        /// BudgetRatio ve bağlı DuesStatistic kayıtlarını getir
        /// </summary>
        /// <param name="id">BudgetRatio ID</param>
        /// <returns>BudgetRatio ve DuesStatistic kayıtları</returns>
        Task<ResponseDto<(BudgetRatio budgetRatio, List<DuesStatistic> duesStatistics)>> GetBudgetRatioWithDuesAsync(string id);

        /// <summary>
        /// BudgetRatio ve bağlı tüm DuesStatistic kayıtlarını güncelle
        /// </summary>
        /// <param name="budgetRatio">BudgetRatio nesnesi</param>
        /// <param name="selectedMonthsFlag">Seçilen aylar flag değeri</param>
        /// <returns>Güncelleme sonucu</returns>
        Task<ResponseDto<bool>> UpdateBudgetRatioWithDuesAsync(BudgetRatio budgetRatio, BuggetRatioMounthEnum selectedMonthsFlag);

        /// <summary>
        /// Bütçe güncelleme önizleme (yeni değerleri hesapla ama kaydetme)
        /// </summary>
        /// <param name="budgetRatioId">BudgetRatio ID</param>
        /// <param name="newRatio">Yeni oran (opsiyonel)</param>
        /// <param name="targetAmount">Hedef toplam bütçe (opsiyonel)</param>
        /// <param name="newMonthsFlag">Yeni aylar flag değeri</param>
        /// <returns>Önizleme sonucu</returns>
        Task<ResponseDto<PreviewUpdateResultViewModel>> PreviewUpdateAsync(string budgetRatioId, decimal? newRatio, decimal? targetAmount, BuggetRatioMounthEnum newMonthsFlag);

        /// <summary>
        /// DuesStatistic kayıtlarını Logo'ya aktar
        /// </summary>
        /// <param name="duesStatisticIds">Aktarılacak DuesStatistic ID'leri</param>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="isDebugMode">Debug modu: Sadece 3 kayıt aktarır ve mail gönderir</param>
        /// <returns>Aktarım sonuçları</returns>
        Task<ResponseDto<List<OrderResultViewModel>>> TransferDuesStatisticsToLogoAsync(List<string> duesStatisticIds, string? userId = null, bool isDebugMode = false);
    }
}
