using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace ModuleManagementBackend.DAL.DapperServices
{
    public interface IDapperService
    {
       Task<T?> Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<IEnumerable<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<T?> Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        DbConnection GetDbconnection();
        Task<int> ExecuteAsync(string storedProcedure, DynamicParameters parameters, CommandType commandType = CommandType.StoredProcedure);
        SqlConnection GetConnection();
    }
}