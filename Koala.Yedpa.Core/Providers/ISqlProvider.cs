using Koala.Yedpa.Core.Dtos;
using System.Data;

namespace Koala.Yedpa.Core.Providers
{
    public interface ISqlProvider
    {
        ResponseDto<DataTable> SqlReader(string query);
        ResponseDto<string> WriteToSql(string query);
        ResponseDto<string> SqlUpdate(string table, Dictionary<string, dynamic> queryParams, string condition);
        ResponseDto<string> SqlInsert(string table, Dictionary<string, dynamic> queryParams);
        ResponseDto<string> CheckDatabaseExists();
    }
}
