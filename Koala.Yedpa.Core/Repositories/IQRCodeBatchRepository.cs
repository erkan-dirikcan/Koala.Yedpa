using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IQRCodeBatchRepository
{
    Task<IEnumerable<QRCodeBatch>> GetAllAsync();
    Task<QRCodeBatch?> GetByIdAsync(int id);
    Task<IEnumerable<QRCodeBatch>> GetByYearAsync(string year);
    Task<QRCodeBatch> AddAsync(QRCodeBatch entity);
    QRCodeBatch Update(QRCodeBatch entity);
    Task DeleteAsync(int id);
    Task<int> CountAsync();
}
