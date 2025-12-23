using Koala.Yedpa.Core.Dtos;
using System.Data;

namespace Koala.Yedpa.Core.Providers;

public interface IDapperProvider
{
    // Okuma işlemleri
    Task<ResponseDto<IEnumerable<T>>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null);
    Task<ResponseDto<T>> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null);
    Task<ResponseDto<T>> QuerySingleAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null);

    // Yazma işlemleri
    Task<ResponseDto<int>> ExecuteAsync(string sql, object parameters = null, IDbTransaction transaction = null);

    // Transaction desteği
    Task<IDbTransaction> BeginTransactionAsync();

    // Dinamik insert/update (opsiyonel - eski yapıyı korumak için)
    Task<ResponseDto<int>> InsertAsync<T>(T entity, string tableName = null) where T : class;
    Task<ResponseDto<int>> UpdateAsync<T>(T entity, string condition, string tableName = null) where T : class;

    // Connection test
    Task<ResponseDto<bool>> CheckConnectionAsync();
}
