using Dapper;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Koala.Yedpa.Service.Providers
{
    public class DapperProvider : IDapperProvider
    {
        private readonly string _connectionString;
        private readonly ILogger<DapperProvider> _logger;
        private SqlConnection _connection;

        public DapperProvider(ISettingsService settingsService, ILogger<DapperProvider> logger)
        {
            _logger = logger;

            var sqlSettingsRes = settingsService.GetLogoSqlSettingsAsync().Result;
            if (sqlSettingsRes.IsSuccess)
            {
                _connectionString = Tools.CreateConnectionString(
                    sqlSettingsRes.Data.Server,
                    sqlSettingsRes.Data.Database,
                    sqlSettingsRes.Data.UserName,
                    sqlSettingsRes.Data.Password
                );
            }
            else
            {
                throw new Exception("Veri Tabanı Ayarları Doğru Yapılandırılmamış.");
            }
        }

        private async Task<SqlConnection> GetOpenConnectionAsync()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            return _connection;
        }

        private SqlConnection GetOpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        // 1. GENERIC QUERY - Liste getir
        public async Task<ResponseDto<IEnumerable<T>>> QueryAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            try
            {
                using var connection = await GetOpenConnectionAsync();

                _logger.LogDebug("Executing Query: {Sql}", sql);
                if (parameters != null)
                    _logger.LogDebug("Parameters: {@Parameters}", parameters);

                var result = await connection.QueryAsync<T>(sql, parameters, transaction);

                return ResponseDto<IEnumerable<T>>.SuccessData(
                    200,
                    "Sorgu başarıyla çalıştırıldı",
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryAsync error: {Message}", ex.Message);
                return ResponseDto<IEnumerable<T>>.FailData(
                    400,
                    "Sorgu çalıştırılırken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 2. TEK KAYIT GETİR
        public async Task<ResponseDto<T>> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            try
            {
                using var connection = await GetOpenConnectionAsync();

                var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, transaction);

                return ResponseDto<T>.SuccessData(
                    200,
                    "Kayıt başarıyla getirildi",
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryFirstOrDefaultAsync error: {Message}", ex.Message);
                return ResponseDto<T>.FailData(
                    404,
                    "Kayıt getirilirken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 3. TEK DEĞER GETİR (Single)
        public async Task<ResponseDto<T>> QuerySingleAsync<T>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            try
            {
                using var connection = await GetOpenConnectionAsync();

                var result = await connection.QuerySingleAsync<T>(sql, parameters, transaction);

                return ResponseDto<T>.SuccessData(
                    200,
                    "Tek kayıt başarıyla getirildi",
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QuerySingleAsync error: {Message}", ex.Message);
                return ResponseDto<T>.FailData(
                    404,
                    "Tek kayıt getirilirken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 4. EXECUTE (INSERT/UPDATE/DELETE)
        public async Task<ResponseDto<int>> ExecuteAsync(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            try
            {
                using var connection = await GetOpenConnectionAsync();

                _logger.LogDebug("Executing Command: {Sql}", sql);
                if (parameters != null)
                    _logger.LogDebug("Parameters: {@Parameters}", parameters);

                var affectedRows = await connection.ExecuteAsync(sql, parameters, transaction);

                return ResponseDto<int>.SuccessData(
                    200,
                    "Komut başarıyla çalıştırıldı",
                    affectedRows
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteAsync error: {Message}", ex.Message);
                return ResponseDto<int>.FailData(
                    400,
                    "Komut çalıştırılırken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 5. TRANSACTION BAŞLAT
        public async Task<IDbTransaction> BeginTransactionAsync()
        {
            var connection = await GetOpenConnectionAsync();
            return await connection.BeginTransactionAsync();
        }

        // 6. GENERIC INSERT (Reflection ile otomatik mapping)
        public async Task<ResponseDto<int>> InsertAsync<T>(T entity, string tableName = null) where T : class
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                tableName ??= typeof(T).Name;

                var properties = typeof(T).GetProperties()
                    .Where(p => p.Name != "Id" && p.CanWrite)
                    .ToList();

                var columns = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
                var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

                var sql = $"INSERT INTO [{tableName}] ({columns}) VALUES ({values}); SELECT SCOPE_IDENTITY();";

                using var connection = await GetOpenConnectionAsync();

                var newId = await connection.ExecuteScalarAsync<int>(sql, entity);

                return ResponseDto<int>.SuccessData(
                    200,
                    "Kayıt başarıyla eklendi",
                    newId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InsertAsync error for {Type}: {Message}", typeof(T).Name, ex.Message);
                return ResponseDto<int>.FailData(
                    400,
                    "Kayıt eklenirken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 7. GENERIC UPDATE
        public async Task<ResponseDto<int>> UpdateAsync<T>(T entity, string condition, string tableName = null) where T : class
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (string.IsNullOrEmpty(condition))
                    throw new ArgumentException("Condition cannot be empty", nameof(condition));

                tableName ??= typeof(T).Name;

                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanWrite)
                    .ToList();

                var setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));

                var sql = $"UPDATE [{tableName}] SET {setClause} WHERE {condition}";

                using var connection = await GetOpenConnectionAsync();

                var affectedRows = await connection.ExecuteAsync(sql, entity);

                return ResponseDto<int>.SuccessData(
                    200,
                    "Kayıt başarıyla güncellendi",
                    affectedRows
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateAsync error for {Type}: {Message}", typeof(T).Name, ex.Message);
                return ResponseDto<int>.FailData(
                    400,
                    "Kayıt güncellenirken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 8. CONNECTION TEST
        public async Task<ResponseDto<bool>> CheckConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.ExecuteScalarAsync<int>("SELECT 1");

                return ResponseDto<bool>.SuccessData(
                    200,
                    "Veritabanı bağlantısı başarılı",
                    result == 1
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CheckConnectionAsync error: {Message}", ex.Message);
                return ResponseDto<bool>.FailData(
                    500,
                    "Veritabanı bağlantısı başarısız",
                    ex.Message,
                    true
                );
            }
        }

        // 9. STORED PROCEDURE ÇALIŞTIR
        public async Task<ResponseDto<IEnumerable<T>>> ExecuteStoredProcedureAsync<T>(string procedureName, object parameters = null)
        {
            try
            {
                using var connection = await GetOpenConnectionAsync();

                var result = await connection.QueryAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return ResponseDto<IEnumerable<T>>.SuccessData(
                    200,
                    "Stored procedure başarıyla çalıştırıldı",
                    result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExecuteStoredProcedureAsync error: {Message}", ex.Message);
                return ResponseDto<IEnumerable<T>>.FailData(
                    400,
                    "Stored procedure çalıştırılırken hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // 10. BULK INSERT (Performance için)
        public async Task<ResponseDto<int>> BulkInsertAsync<T>(IEnumerable<T> entities, string tableName = null) where T : class
        {
            try
            {
                if (entities == null || !entities.Any())
                    return ResponseDto<int>.SuccessData(200, "Eklenecek kayıt yok", 0);

                using var transaction = await BeginTransactionAsync();

                try
                {
                    var affectedRows = 0;
                    foreach (var entity in entities)
                    {
                        var result = await InsertAsync(entity, tableName);
                        if (result.IsSuccess)
                            affectedRows++;
                    }

                    transaction.Commit();

                    return ResponseDto<int>.SuccessData(
                        200,
                        "Toplu ekleme başarılı",
                        affectedRows
                    );
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BulkInsertAsync error: {Message}", ex.Message);
                return ResponseDto<int>.FailData(
                    400,
                    "Toplu ekleme sırasında hata oluştu",
                    ex.Message,
                    true
                );
            }
        }

        // Dispose pattern
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
    }
}
