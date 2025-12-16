using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Providers;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using Koala.Yedpa.Core.Helpers;
using Koala.Yedpa.Core.Services;

namespace Koala.Yedpa.Service.Providers
{
    public class SqlProvider : ISqlProvider
    {
        private string _connection;
        private readonly ISettingsService _settingsService;
        public SqlProvider(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            var sqlSettingsRes = _settingsService.GetLogoSqlSettingsAsync().Result;
            if (sqlSettingsRes.IsSuccess)
            {
                _connection = Tools.CreateConnectionString(sqlSettingsRes.Data.Server, sqlSettingsRes.Data.Database,
                    sqlSettingsRes.Data.UserName, sqlSettingsRes.Data.Password);

            }
            else
            {
                throw new Exception("Veri Tabanı Ayarları Doğru Yapılandırılmamış.");
            }
        }
        public ResponseDto<DataTable> SqlReader(string query)
        {
            try
            {
                var conn = new SqlConnection(_connection);
                var cmd = new SqlCommand(query, conn);
                conn.Open();
                var da = new SqlDataAdapter(cmd);
                var res = new DataTable();

                da.Fill(res);
                conn.Close();
                da.Dispose();
                return ResponseDto<DataTable>.SuccessData(200, "Sorgu Başarıyla Çalıştırıldı", res);
            }
            catch (Exception ex)
            {
                return ResponseDto<DataTable>.FailData(400, "Sorgu Çalıştırılırken Bir Sorunla Karşılaşıldı", ex.Message, true);
            }
        }

        public ResponseDto<string> WriteToSql(string query)
        {
            var cnn = new SqlConnection(_connection);
            var cmd = new SqlCommand(query, cnn);
            try
            {
                cmd.Connection.Open();
                var res = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                return ResponseDto<string>.SuccessData(200, "Sql Server Yazma İşlemi Başarılı", res.ToString());
            }
            catch (Exception ex)
            {
                return ResponseDto<string>.FailData(400, "Sql Server'a Yazma Sırasında Bir Sorunla Karşılaşıldı", ex.Message, true);
            }
        }

        public ResponseDto<string> SqlUpdate(string table, Dictionary<string, dynamic> queryParams, string condition)
        {
            try
            {
                var query = new StringBuilder("Update " + table + " set ");
                var i = 1;
                foreach (var item in queryParams)
                {
                    query.Append(item.Key);
                    query.Append(" = ");
                    Type unknown = item.Value.GetType();

                    if (unknown == typeof(string))
                    {
                        query.Append(" '" + item.Value + "'");
                    }
                    else if (unknown == typeof(bool))
                    {
                        query.Append(" " + (item.Value ? "1" : "0"));
                    }
                    else
                    {
                        query.Append(" " + item.Value);
                    }
                    if (i < queryParams.Count)
                    {
                        query.Append(",");
                    }
                    i++;
                }
                query.Append(" where " + condition);

                return WriteToSql(query.ToString());
            }
            catch (Exception ex)
            {
                return ResponseDto<string>.FailData(400, "Sql Server Update İşlemi Sırasında Bir Sorunla Karşılaşıldı", ex.Message, true);
            }
        }

        public ResponseDto<string> SqlInsert(string table, Dictionary<string, dynamic> queryParams)
        {
            try
            {
                var sorgu = new StringBuilder("INSERT INTO " + table + " ");
                var cols = new StringBuilder("(");
                var vals = new StringBuilder("VALUES (");

                var i = 1;
                foreach (var item in queryParams)
                {
                    cols.Append(item.Key);

                    Type unknown = item.Value.GetType();

                    if (unknown == typeof(string))
                    {
                        vals.Append(" '" + item.Value + "'");
                    }
                    else if (unknown == typeof(bool))
                    {
                        vals.Append(" " + (item.Value ? "1" : "0"));
                    }
                    else
                    {
                        vals.Append(" " + item.Value);
                    }
                    if (i < queryParams.Count)
                    {
                        cols.Append(",");
                        vals.Append(",");
                    }
                    else
                    {
                        cols.Append(") ");
                        vals.Append(") ");
                    }
                    i++;
                }
                sorgu.Append(cols.ToString());
                sorgu.Append(vals.ToString());
                return WriteToSql(sorgu.ToString());


            }
            catch (Exception ex)
            {
                return ResponseDto<string>.FailData(400, "Sql Server Insert İşlemi Sırasında Bir Sorunla Karşılaşıldı", ex.Message, true);
            }
        }

        public ResponseDto<string> CheckDatabaseExists()
        {
            try
            {
                SqlConnection con = new SqlConnection(_connection);
                SqlCommand cmd = new SqlCommand("select 1", con);
                con.Open();
                var rowCount = cmd.ExecuteScalar();
                con.Close();
                return ResponseDto<string>.SuccessData(200, "Sql Server Bağlantısı Başarılı", "Sql Server Bağlantısı Başarılı");
            }
            catch (Exception ex)
            {
                return ResponseDto<string>.FailData(200, "Sql Server Bağlantısı Başarısız", ex.Message, true);
            }
        }
    }
}
