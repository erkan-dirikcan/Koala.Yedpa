using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IQRCodeRepository
{
    /// <summary>
    /// Tüm QR kodlarını getirir
    /// </summary>
    Task<IEnumerable<QRCode>> GetAllAsync();

    /// <summary>
    /// BatchId'ye göre QR kodları getirir
    /// </summary>
    Task<IEnumerable<QRCode>> GetByBatchIdAsync(int batchId);

    /// <summary>
    /// PartnerNo'ya göre QR kod getirir
    /// </summary>
    Task<QRCode?> GetByPartnerNoAsync(string partnerNo);

    /// <summary>
    /// QR kod yılına göre QR kodları getirir
    /// </summary>
    Task<IEnumerable<QRCode>> GetByYearAsync(string year);

    /// <summary>
    /// Durumuna göre QR kodları getirir
    /// </summary>
    Task<IEnumerable<QRCode>> GetByStatusAsync(StatusEnum status);

    /// <summary>
    /// Yeni QR kod ekler
    /// </summary>
    Task<QRCode> AddAsync(QRCode entity);

    /// <summary>
    /// QR kod günceller
    /// </summary>
    QRCode Update(QRCode entity);

    /// <summary>
    /// QR kod siler (soft delete)
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Tüm QR kodlarını siler (hard delete)
    /// </summary>
    Task DeleteAllAsync();

    /// <summary>
    /// Belirli bir yıldaki tüm QR kodlarını siler
    /// </summary>
    Task DeleteByYearAsync(string year);

    /// <summary>
    /// QR kod sayısını getirir
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// Belirli bir durumdaki QR kod sayısını getirir
    /// </summary>
    Task<int> CountAsync(StatusEnum status);
}
